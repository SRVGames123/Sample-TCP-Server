namespace ModalStrikeServer.RpcServer.Core {
    public class ReceiveException : System.Exception {
        public ReceiveException() : base("Failed to receive data.") { }
        public ReceiveException(string message) : base(message) { }
    }
}
