namespace NodeServer.Tests;

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Node;
using NodeServer;
using NodeServer.handlers;

public sealed class RequestDispatcherTests : IDisposable
{
    private class TestDispatcher(ILogger<RequestDispatcher> logger, IAsyncQueue<Request> requestQueue, IOneofHandler<ControlRequest> controlHandler, IOneofHandler<MatRequest> materialHandler) : RequestDispatcher(logger, requestQueue, controlHandler, materialHandler)
    {
        public Task TestExecuteAsync(CancellationToken cancellationToken)
        {
            return base.ExecuteAsync(cancellationToken);
        }
    }

    private readonly CancellationTokenSource tokenSource = new();
    private readonly TestDispatcher dispatcher;
    private readonly Mock<ILogger<RequestDispatcher>> loggerMock = new();
    private readonly Mock<IOneofHandler<ControlRequest>> controlMock = new();
    private readonly Mock<IOneofHandler<MatRequest>> materialMock = new();
    private readonly Mock<IAsyncQueue<Request>> queueMock = new();

    public RequestDispatcherTests()
    {
        loggerMock.Setup(m => m.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
        dispatcher = new(loggerMock.Object, queueMock.Object, controlMock.Object, materialMock.Object);
    }

    public void Dispose()
    {
        loggerMock.VerifyNoOtherCalls();
        controlMock.VerifyNoOtherCalls();
        materialMock.VerifyNoOtherCalls();
        queueMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteTask_PreCancel()
    {
        await tokenSource.CancelAsync();
        await dispatcher.TestExecuteAsync(tokenSource.Token);
        // no exception means it didn't get into the waits
    }

    [Fact]
    public async void ExecuteTask_Cancels()
    {
        // mock the dequeue with a task that we'll simulate the cancel with an OperationCanceledException
        TaskCompletionSource<Request> taskCompletionSource = new();
        queueMock.Setup(m => m.DequeueAsync(It.IsAny<CancellationToken>())).Returns(taskCompletionSource.Task);
        // run the execute
        Task task = dispatcher.TestExecuteAsync(tokenSource.Token);
        // cancel
        await tokenSource.CancelAsync();
        taskCompletionSource.SetException(new OperationCanceledException());
        // should complete with the exception
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
        // logs every loop
        loggerMock.Verify(m => m.IsEnabled(LogLevel.Debug));
        queueMock.Verify(m => m.DequeueAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async void ExecuteAsync_HandleControl()
    {
        // will call DequeueAsync twice, first time return the request, 2nd return a task we'll cancel
        Request request = new() { ControlRequest = new()};
        TaskCompletionSource<Request> taskCompletionSource = new();
        queueMock.SetupSequence(m => m.DequeueAsync(It.IsAny<CancellationToken>())).ReturnsAsync(request).Returns(taskCompletionSource.Task);
        // run it
        Task task = dispatcher.TestExecuteAsync(tokenSource.Token);
        // cancel
        await tokenSource.CancelAsync();
        taskCompletionSource.SetException(new OperationCanceledException());
        // should throw
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
        // verify the handler was called
        controlMock.Verify(m => m.Handle(request.ControlRequest));
        loggerMock.Verify(m => m.IsEnabled(LogLevel.Debug));
        queueMock.Verify(m => m.DequeueAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]

    public async void ExecuteAsync_HandleMaterial()
    {
        // will call DequeueAsync twice, first time return the request, 2nd return a task we'll cancel
        Request request = new() { MatRequest = new()};
        TaskCompletionSource<Request> taskCompletionSource = new();
        queueMock.SetupSequence(m => m.DequeueAsync(It.IsAny<CancellationToken>())).ReturnsAsync(request).Returns(taskCompletionSource.Task);
        // run it
        Task task = dispatcher.TestExecuteAsync(tokenSource.Token);
        // cancel
        await tokenSource.CancelAsync();
        taskCompletionSource.SetException(new OperationCanceledException());
        // should throw
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
        // verify the handler was called
        materialMock.Verify(m => m.Handle(request.MatRequest));
        loggerMock.Verify(m => m.IsEnabled(LogLevel.Debug));
        queueMock.Verify(m => m.DequeueAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async void ExecuteAsync_Unknown()
    {
        // will call DequeueAsync twice, first time return the request, 2nd return a task we'll cancel
        Request request = new();
        TaskCompletionSource<Request> taskCompletionSource = new();
        queueMock.SetupSequence(m => m.DequeueAsync(It.IsAny<CancellationToken>())).ReturnsAsync(request).Returns(taskCompletionSource.Task);
        // run it
        Task task = dispatcher.TestExecuteAsync(tokenSource.Token);
        // cancel
        await tokenSource.CancelAsync();
        taskCompletionSource.SetException(new OperationCanceledException());
        // should throw
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
        // verify warning was logged
        loggerMock.Verify(m => m.IsEnabled(LogLevel.Warning));
        loggerMock.Verify(m => m.IsEnabled(LogLevel.Debug));
        queueMock.Verify(m => m.DequeueAsync(It.IsAny<CancellationToken>()));
    }

    // [Fact]
    // public async Task Dispatch_Unknown()
    // {
    //     Request request = new();
    //     loggerMock.Setup(m => m.IsEnabled(LogLevel.Warning)).Returns(true);
    //     await dispatcher.Dispatch(request);
    //     loggerMock.Verify(m => m.IsEnabled(LogLevel.Warning));
    //     loggerMock.Verify(m => m.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    // }
}