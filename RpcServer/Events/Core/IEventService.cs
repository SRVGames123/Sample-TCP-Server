namespace ModalStrikeServer.RpcServer.Events.Core
{
    public interface IEventService
    {
        string NameEventService { get; }
        Task SendEvent(string eventName, object[] data);
    }
}