using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Node;
using NodeServer.handlers;

namespace NodeServer;

public partial class RequestDispatcher(ILogger<RequestDispatcher> logger, IAsyncQueue<Request> requestQueue, IOneofHandler<ControlRequest> controlHandler, IOneofHandler<MatRequest> materialHandler) : BackgroundService
{
    private readonly Dictionary<Request.RequestTypeOneofCase, Func<Request, Task>> handlerMap = new()
    {
        [Request.RequestTypeOneofCase.ControlRequest] = request => controlHandler.Handle(request.ControlRequest),
        [Request.RequestTypeOneofCase.MatRequest] = request => materialHandler.Handle(request.MatRequest),
    };

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            DebugWaitRequest();
            Request request = await requestQueue.DequeueAsync(cancellationToken);
            DebugRequestType(request.RequestTypeCase);
            if (handlerMap.TryGetValue(request.RequestTypeCase, out Func<Request, Task>? handler))
            {
                await handler.Invoke(request);
            }
            else
            {
                WarnUnknownRequest(request.RequestTypeCase);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Waiting for request")]
    private partial void DebugWaitRequest();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Dequeued Request: {RequestType}")]
    private partial void DebugRequestType(Request.RequestTypeOneofCase requestType);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unknown Request Type '{RequestType}'")]
    private partial void WarnUnknownRequest(Request.RequestTypeOneofCase requestType);
}