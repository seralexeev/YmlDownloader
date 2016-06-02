using Dapper;
using System;

namespace YMLDownloader
{
    public class Logger
    {
        private readonly ConnectionFactory _factory;

        public Logger(ConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Write(string s, LogType type = LogType.Error)
        {
            // в нормальном логгере будем кидать куда нить в трубу
            const string query = @"insert into [logs] ([text]) values (@text)";

            using (var con = _factory.Create())
                con.Execute(query, new { text = $"{DateTime.UtcNow.ToShortTimeString()} {type}: {s}" });
        }
    }
}
