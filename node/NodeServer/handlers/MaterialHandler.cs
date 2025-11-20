using Microsoft.Extensions.Logging;
using Node;

namespace NodeServer.handlers;

public partial class MaterialHandler(ILogger<ControlHandler> logger) : IOneofHandler<MatRequest>
{
    private readonly ILogger logger = logger;

    public async Task Handle(MatRequest request)
    {
        logger.DebugProto(request);
    }
}