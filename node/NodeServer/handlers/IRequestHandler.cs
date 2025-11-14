using Node;

namespace NodeServer.handlers;

public interface IRequestHandler<T> where T: Google.Protobuf.IMessage<T>
{
    public Request.RequestTypeOneofCase RequestType { get; }

    public Task Handle(T request);
}