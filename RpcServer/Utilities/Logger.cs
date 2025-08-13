namespace ModalStrikeServer.RpcServer.Utilities {
    public class Logger {
        public static bool LogDebug = true;

        public static void Log(string message) 
            => Console.WriteLine($"[{GetDate()}] - {message}\n");

        public static void Debug(string message) 
            => Console.WriteLine($"[{GetDate()}] - {message}\n");

        public static void Error(string message) 
            => Console.WriteLine($"[{GetDate()}] - {message}\n");

        public static void Exception(Exception message) 
            => Console.WriteLine($"[{GetDate()}] - {message}\n");
        
        public static string GetDate() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
