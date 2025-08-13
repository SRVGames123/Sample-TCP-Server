namespace ModalStrikeServer.RpcServer.Utilities.CustomLogger {
    public class ServiceLogger {
        private readonly ILogger _logger;

        public ServiceLogger(Type type) => _logger = new CustomLogger(type);

        public void Debug(string message, params object[] args) {
            _logger.Debug(message, args);
        }

        public void Warning(string message, params object[] args) {
            _logger.Warning(message, args);
        }

        public void Error(string message, params object[] args) {
            _logger.Error(message, args);
        }
    }
}