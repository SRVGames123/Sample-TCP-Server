namespace ModalStrikeServer.RpcServer.Core.Invoke
{
    public interface ISystemInvoke
    {
        Task<object> InvokeAsync(string serviceName, string methodName, params object[] args);
    }
}