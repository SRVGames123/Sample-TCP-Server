using ModalStrikeServer.RpcServer.Exceptions.Core;

namespace ModalStrikeServer.RpcServer.Exceptions.Account {
    public class AccountIsBannedException : RpcException {
        public override int RpcCode => 1001;
    }
}
