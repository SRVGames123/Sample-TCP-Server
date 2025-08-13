using ModalStrikeServer.RpcServer.Exceptions.Core;

namespace ModalStrikeServer.RpcServer.Exceptions.Account {
    public class IsHaveAccountException : RpcException {
        public override int RpcCode => 1002;
    }
}
