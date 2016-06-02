namespace YMLDownloader
{
    public interface ILogger
    {
        void Write(string s, LogType type = LogType.Error);
    }
}