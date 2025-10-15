namespace ArenaGame.Shared.Core
{
    /// <summary>
    /// Logging interface that works in both Unity and non-Unity contexts
    /// </summary>
    public interface IGameLogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
    
    /// <summary>
    /// Static logger instance - set by client on startup
    /// </summary>
    public static class GameLogger
    {
        private static IGameLogger instance;
        
        public static void Initialize(IGameLogger logger)
        {
            instance = logger;
        }
        
        public static void Log(string message)
        {
            instance?.Log(message);
        }
        
        public static void LogWarning(string message)
        {
            instance?.LogWarning(message);
        }
        
        public static void LogError(string message)
        {
            instance?.LogError(message);
        }
    }
}

