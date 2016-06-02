using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace YMLDownloader
{
    public class YmlStreamProvider : IDisposable
    {
        private HttpClient _client;
        private int _retryPolicy;
        private int _retryTimeout = 1000;

        public YmlStreamProvider(int concurrencyDegree, int retryPolicy = 0, TimeSpan? timeout = null)
        {
            _client = new HttpClient();
            _client.Timeout = timeout ?? Timeout.InfiniteTimeSpan;
            _retryPolicy = retryPolicy;
            ServicePointManager.DefaultConnectionLimit = concurrencyDegree;
        }

        public async Task<YmlStreamWrapper> GetYmlStream(string url)
        {
            var attempt = 0;
            while (true)
            {
                try
                {
                    // нам хватит прочитать заголовк, и далее уже работать с потоком
                    // не дожидаясь что прилетит весь контент
                    // что нам делать с медленными соединениями? (очередь?)
                    // что делать при обрыве? или это не наш уровень и за это ответит TCP?
                    var response = await _client.GetAsync(
                        url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                    return new YmlStreamWrapper(
                        response, await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
                }
                catch when (attempt++ < _retryPolicy)
                {
                    await Task.Delay(_retryTimeout).ConfigureAwait(false);
                }
            }
        }

        public void Dispose() => _client.Dispose();
    }
}
