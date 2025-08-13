using ModalStrikeServer.RpcServer.Exceptions.Core;

namespace ModalStrikeServer.RpcServer.Exceptions.DeviceInfo
{
    public class DeviceInfoAlreadyExistException : RpcException {
        public override int RpcCode => 1003;
    }
}