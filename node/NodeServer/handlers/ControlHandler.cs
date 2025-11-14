using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Node;

namespace NodeServer.handlers;

public partial class ControlHandler : IRequestHandler<ControlRequest>
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

    public async Task Handle(ControlRequest request)
    {
        logger.DebugProto(request);
        switch (request.ControlTypeCase)
        {
            case ControlRequest.ControlTypeOneofCase.Shutdown:
                if (request.Shutdown.Delay > 0)
                {
                    await Task.Delay((int)request.Shutdown.Delay);
                }
                applicationLifetime.StopApplication();
                break;
            default:
                WarnUnknownControl(request.ControlTypeCase);
                break;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unknown Control Type: '{ControlType}'")]
    private partial void WarnUnknownControl(ControlRequest.ControlTypeOneofCase controlType);
}