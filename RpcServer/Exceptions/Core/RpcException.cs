namespace ModalStrikeServer.RpcServer.Exceptions.Core {
    public abstract class RpcException : Exception {
        public abstract int RpcCode { get; }
    }
}
