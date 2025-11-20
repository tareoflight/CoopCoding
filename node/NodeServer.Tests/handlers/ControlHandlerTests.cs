using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Node;
using NodeServer.handlers;

namespace NodeServer.Tests.handlers;

public sealed class ControlHandlerTests : BaseHandlerTests<ControlHandler>
{
    private readonly ControlHandler handler;
    private readonly Mock<IHostApplicationLifetime> lifetimeMock = new();

    public ControlHandlerTests()
    {
        handler = new(loggerMock.Object, lifetimeMock.Object);
    }

    [Fact]
    public async Task Handle_Unknown()
    {
        ControlRequest request = new();
        await handler.Handle(request);
        loggerMock.Verify(m => m.IsEnabled(LogLevel.Debug));
        loggerMock.Verify(m => m.IsEnabled(LogLevel.Warning));
    }

    [Fact]
    public async Task Handle_Shutdown()
    {
        ControlRequest request = new() { Shutdown = new Shutdown() };
        await handler.Handle(request);
        lifetimeMock.Verify(m => m.StopApplication());
        loggerMock.Verify(m => m.IsEnabled(LogLevel.Debug));
    }
}
