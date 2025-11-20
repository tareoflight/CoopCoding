namespace NodeServer.handlers;

public interface IOneofHandler<TMessage> where TMessage : Google.Protobuf.IMessage
{
    public Task Handle(TMessage message);
}
