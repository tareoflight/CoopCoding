using Node;
using NodeServer.handlers;

namespace NodeServer;

public interface IHandlerMap
{
    public void AddHandler(IRequestHandler handler);

    public IRequestHandler? GetHandlerOrNull(Request.RequestTypeOneofCase requestType);
}