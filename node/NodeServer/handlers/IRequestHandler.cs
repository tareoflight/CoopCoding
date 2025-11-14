using Node;

namespace NodeServer.handlers;

public interface IRequestHandler
{
    public Request.RequestTypeOneofCase RequestType { get; }

    public Task Handle(Request request);
}