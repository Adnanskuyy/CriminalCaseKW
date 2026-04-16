using UnityEngine;
using System;
using System.Collections.Generic;

namespace CriminalCase2.Utils
{
    public enum LogLevel
    {
        Verbose,
        Debug,
        Info,
        Warning,
        Error,
        None
    }

    public static class LoggingUtility
    {
        private static LogLevel _currentLogLevel = LogLevel.Debug;
        private static readonly Dictionary<string, LogLevel> _categoryLogLevels = new();
        private static bool _enableTimestamp = true;
        private static bool _enableCategoryPrefix = true;

        public static LogLevel CurrentLogLevel
        {
            get => _currentLogLevel;
            set => _currentLogLevel = value;
        }

        public static bool EnableTimestamp
        {
            get => _enableTimestamp;
            set => _enableTimestamp = value;
        }

        public static bool EnableCategoryPrefix
        {
            get => _enableCategoryPrefix;
            set => _enableCategoryPrefix = value;
        }

        public static void SetCategoryLogLevel(string category, LogLevel level)
        {
            _categoryLogLevels[category] = level;
        }

        private static bool ShouldLog(string category, LogLevel level)
        {
            if (level < _currentLogLevel) return false;
            
            if (_categoryLogLevels.TryGetValue(category, out var categoryLevel))
            {
                return level >= categoryLevel;
            }
            
            return true;
        }

        private static string FormatMessage(string category, string message)
        {
            var sb = new System.Text.StringBuilder();
            
            if (_enableTimestamp)
            {
                sb.Append($"[{DateTime.Now:HH:mm:ss.fff}] ");
            }
            
            if (_enableCategoryPrefix && !string.IsNullOrEmpty(category))
            {
                sb.Append($"[{category}] ");
            }
            
            sb.Append(message);
            return sb.ToString();
        }

        public static void Verbose(string category, string message)
        {
            if (!ShouldLog(category, LogLevel.Verbose)) return;
            UnityEngine.Debug.Log(FormatMessage(category, message));
        }

        public static void LogDebug(string category, string message)
        {
            if (!ShouldLog(category, LogLevel.Debug)) return;
            UnityEngine.Debug.Log(FormatMessage(category, message));
        }

        public static void Info(string category, string message)
        {
            if (!ShouldLog(category, LogLevel.Info)) return;
            UnityEngine.Debug.Log(FormatMessage(category, message));
        }

        public static void Warning(string category, string message)
        {
            if (!ShouldLog(category, LogLevel.Warning)) return;
            UnityEngine.Debug.LogWarning(FormatMessage(category, message));
        }

        public static void Error(string category, string message)
        {
            if (!ShouldLog(category, LogLevel.Error)) return;
            UnityEngine.Debug.LogError(FormatMessage(category, message));
        }

        public static void LogException(string category, Exception ex, string context = null)
        {
            if (!ShouldLog(category, LogLevel.Error)) return;
            var message = string.IsNullOrEmpty(context) 
                ? ex.Message 
                : $"{context}: {ex.Message}";
            UnityEngine.Debug.LogError(FormatMessage(category, message));
            UnityEngine.Debug.LogException(ex);
        }

        #region Convenience Methods
        
        public static void LogVideo(string message, LogLevel level = LogLevel.Info)
        {
            LogWithLevel("Video", message, level);
        }

        public static void LogClue(string message, LogLevel level = LogLevel.Info)
        {
            LogWithLevel("Clue", message, level);
        }

        public static void LogState(string message, LogLevel level = LogLevel.Info)
        {
            LogWithLevel("State", message, level);
        }

        public static void LogUI(string message, LogLevel level = LogLevel.Info)
        {
            LogWithLevel("UI", message, level);
        }

        private static void LogWithLevel(string category, string message, LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Verbose:
                    Verbose(category, message);
                    break;
                case LogLevel.Debug:
                    LogDebug(category, message);
                    break;
                case LogLevel.Info:
                    Info(category, message);
                    break;
                case LogLevel.Warning:
                    Warning(category, message);
                    break;
                case LogLevel.Error:
                    Error(category, message);
                    break;
            }
        }

        #endregion
    }
}
