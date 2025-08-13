using ModalStrikeServer.RpcServer.Exceptions.Core;

namespace ModalStrikeServer.RpcServer.Exceptions.Account {
    public class NotValidPasswordAndLoginException : RpcException {
        public override int RpcCode => 1002;
    }
}
