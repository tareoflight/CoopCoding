using Node;
using NodeServer.handlers;

namespace NodeServer;

public interface IHandlerMap
{
    public void AddHandler<T>(IRequestHandler<T> handler) where T: Google.Protobuf.IMessage<T>;

    public IRequestHandler<T>? GetHandlerOrNull<T>(Request.RequestTypeOneofCase requestType) where T: Google.Protobuf.IMessage<T>;
}