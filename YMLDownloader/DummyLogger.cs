using System;

namespace YMLDownloader
{
    public class DummyLogger : ILogger
    {
        public void Write(string s, LogType type = LogType.Error)
        {
        }
    }
}