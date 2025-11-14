using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Node;

namespace NodeServer.handlers;

public partial class ControlHandler : IRequestHandler
{
    private readonly ILogger logger;
    private readonly IHostApplicationLifetime applicationLifetime;
    public Request.RequestTypeOneofCase RequestType => Request.RequestTypeOneofCase.ControlRequest;

    public ControlHandler(ILogger<ControlHandler> logger, IHandlerMap requestMap, IHostApplicationLifetime applicationLifetime)
    {
        this.logger = logger;
        this.applicationLifetime = applicationLifetime;
        requestMap.AddHandler(this);
    }

    public async Task Handle(Request request)
    {
        logger.DebugProto(request);
        switch (request.ControlRequest.ControlTypeCase)
        {
            case ControlRequest.ControlTypeOneofCase.Shutdown:
                if (request.ControlRequest.Shutdown.Delay > 0)
                {
                    await Task.Delay((int)request.ControlRequest.Shutdown.Delay);
                }
                applicationLifetime.StopApplication();
                break;
            default:
                WarnUnknownControl(request.ControlRequest.ControlTypeCase);
                break;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unknown Control Type: '{ControlType}'")]
    private partial void WarnUnknownControl(ControlRequest.ControlTypeOneofCase controlType);
}