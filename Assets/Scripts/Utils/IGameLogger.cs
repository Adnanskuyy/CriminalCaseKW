namespace CriminalCase2.Utils
{
    public interface IGameLogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
    }
}
