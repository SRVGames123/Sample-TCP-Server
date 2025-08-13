namespace ModalStrikeServer.RpcServer.Utilities.CustomLogger {
    public class CustomLogger(Type type) : ILogger {
        private const string _loggerName = "[SRV and Klobir(govnocoder)]";

        private const bool _debug = true;

        private const bool _savingLogsToFile = true;

        private const string _pattern
            = "\x1b[32m[{0}]\x1b[0m type: {1} fullName {2};\nmsg: {3}\nparameters: {4}\n";

        public void Debug(string msg, params object[] parameters)
            => WriteLog(LogType.Debug, msg, parameters);

        public void Warning(string msg, params object[] parameters)
            => WriteLog(LogType.Warning, msg, parameters);

        public void Error(string msg, params object[] parameters)
            => WriteLog(LogType.Error, msg, parameters);

        private void WriteLog(LogType type, string msg, params object[] parameters) {
            var pattern = GetPattern(type, msg, parameters);

            if(_debug)
                Console.WriteLine(pattern);

            if(_savingLogsToFile)
                WriteLogToFile(pattern, type);
        }

        private string GetPattern(LogType logType, string msg, params object[] parameters) {
            var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var fullName = $"{type.FullName}.{type.Name}";

            return string.Format(_pattern, date, GetTextLogType(logType), fullName, msg, parameters);
        }

        private string GetTextLogType(LogType type) {
            return type == LogType.Debug ? $"\x1b[32m{type.ToString()}\x1b[0m" : type == LogType.Error ? $"\x1b[31m{type.ToString()}\x1b[0m" : $"\x1b[33m{type.ToString()}\x1b[0m";
        }

        private void WriteLogToFile(string pattern, LogType type) {
            var patch = type == LogType.Debug ? LoggerFilesData.LogFileName
                : type == LogType.Warning ? LoggerFilesData.LogWarningFileName : LoggerFilesData.LogErrorFileName;

            var allLogsPatch = LoggerFilesData.AllLogsFileName;

            Log(pattern, patch, allLogsPatch);
        }

        public void Log(string message, params string[] logFilePath) {
            foreach(var path in logFilePath) {
                try {
                    using(StreamWriter writer = File.AppendText(path)) {
                        writer.WriteLine($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                    }
                }
                catch(Exception ex) {
                    Console.WriteLine($"Ошибка при записи в лог-файл: {ex.Message}");
                }
            }
        }
    }
}