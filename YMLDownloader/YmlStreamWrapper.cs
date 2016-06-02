using System;
using System.IO;
using System.Net.Http;

namespace YMLDownloader
{

    // Хочется видеть прогрессбар для этого мы можем счиатать кол-во прочитанных
    // байтов из стрима и сравнивать с длиной контента что прилетела в заголовках
    // ... но не факт что сервер отдаст нам этот заголовок (а что если gzip?)
    public class YmlStreamWrapper : Stream
    {
        public long? ContentLength { get; }
        private HttpResponseMessage _response { get; }
        private Stream _inner { get; }
        public event EventHandler OnPositionChanged;
        private int _position;

        // Мы не будем оповещать подписчиков о каждом риде
        // достаточно будет оповещать каждые 4 KB 
        // ... почему 4?
        public static int Granularity { get; } = 4 * 1024;

        public int GetMaxTicks() =>
            ContentLength != null ? (int)(ContentLength.Value / Granularity) : 1;

        public YmlStreamWrapper(HttpResponseMessage response, Stream stream)
        {
            _response = response;
            _inner = stream;
            ContentLength = _response.Content.Headers?.ContentLength;
        }

        protected void Increment(int value)
        {
            if (value > 0)
            {
                _position += value;
                if (_position >= Granularity)
                {
                    OnPositionChanged?.Invoke(this, EventArgs.Empty);
                    _position = _position % Granularity;
                }
            }
        }

        #region Decorated
        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position
        {
            get { return _inner.Position; }
            set { _inner.Position = value; }
        }

        public override void Flush() => _inner.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var result = _inner.Read(buffer, offset, count);
            Increment(result);
            return result;
        }
            
        public override long Seek(long offset, SeekOrigin origin) =>
            _inner.Seek(offset, origin);

        public override void SetLength(long value) =>
            _inner.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) =>
            _inner.Write(buffer, offset, count);

        #endregion   
    }
}
