using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Node;
using SpacetimeDB;
using SpacetimeDB.Types;

namespace NodeServer;

public partial class SpacetimeService(ILogger<SpacetimeService> logger, IOptions<NodeServerOptions> options, ISocketConnection socketConnection) : BackgroundService
{
    private readonly NodeServerOptions options = options.Value;
    private CancellationToken token = new();

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        token = cancellationToken;
        // Identity? local_identity = null;
        // ConcurrentQueue<(string Command, string Args)> input_queue = new();

        // AuthToken.Init();
        DbConnection conn = DbConnection.Builder()
            .WithUri("http://localhost:3000")
            .WithModuleName("noot")
            // .WithToken(AuthToken.Token)
            .OnConnect(OnConnect)
            .OnConnectError((e) =>
            {
                Console.Write($"Error while connecting: {e}");
            })
            .OnDisconnect((conn, e) =>
            {
                if (e != null)
                {
                    Console.Write($"Disconnected abnormally: {e}");
                }
                else
                {
                    Console.Write($"Disconnected normally.");
                }
            })
            .Build();

        while (!cancellationToken.IsCancellationRequested)
        {
            conn.FrameTick();
            await Task.Delay(100, cancellationToken);
        }
        conn.Disconnect();
    }

    private void OnConnect(DbConnection conn, Identity identity, string authToken)
    {
        InfoConnected();
        conn.Db.NodeData.OnInsert += async (ctx, row) => await SendNodeData(row);
        conn.Db.NodeData.OnUpdate += async (ctx, oldRow, newRow) => await SendNodeData(newRow);
        conn.SubscriptionBuilder().OnApplied((ctx) =>
        {
            InfoSubscribed();
        }).SubscribeToAllTables();
    }

    private async Task SendNodeData(NodeData row)
    {
        NodeEvent nodeEvent = new()
        {
            MapEvent = new MapEvent()
        };
        nodeEvent.MapEvent.Nodes.Add(new PBNodeData()
        {
            Id = row.Id,
            ChunkId = row.ChunkId,
            Location = new PBVector3() { X = row.X, Y = row.Y, Z = row.Z },
            Contents = ByteString.CopyFrom([.. row.Contents]),
        });
        await socketConnection.SendNodeEventAsync(nodeEvent, token);
    }

    [LoggerMessage(Message = "Connected to Spacetime", Level = LogLevel.Information)]
    private partial void InfoConnected();

    [LoggerMessage(Message = "Subscribed to Spacetime", Level = LogLevel.Information)]
    private partial void InfoSubscribed();
}