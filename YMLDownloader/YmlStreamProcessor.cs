using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static YMLDownloader.YmlStreamReader;

namespace YMLDownloader
{
    public class YmlStreamProcessor
    {
        private LinkedList<Category> _categories { get; } = new LinkedList<Category>();
        private int _categoriesCount = 0;
        private LinkedList<Category> _invalidCategories { get; } = new LinkedList<Category>();
        private int _invalidCategoriesCount = 0;
        private LinkedList<Product> _products { get; } = new LinkedList<Product>();
        private int _productsCount = 0;
        private LinkedList<Product> _invalidProducts { get; } = new LinkedList<Product>();
        private int _invalidProductsCount = 0;


        private readonly int _bufferSize;
        private readonly ProductSaver _productSaver;
        private YmlStreamReader _reader;
        private Stopwatch _stopWatch;
        private Exception _exception;
        private XmlProductParser _parser;

        public string Url { get; }

        public YmlStreamProcessor(string url,
            YmlStreamReader reader, XmlProductParser parser,
            ProductSaver productSaver, int bufferSize)
        {
            _reader = reader;
            _parser = parser;
            _productSaver = productSaver;
            _bufferSize = bufferSize;
            _stopWatch = Stopwatch.StartNew();

            Url = url;
        }

        public async Task<YmlResult> Process(Stream stream)
        {
            try
            {
                // Тут все неправильно
                Product p;
                Category c;
                _reader.ProcessElements(stream,
                    new HandlersCollection
                    {
                        ["category"] = e => (_parser.TryParseCategory(e, out c) ? _categories : _invalidCategories).AddLast(c),
                        ["offer"] = e => (_parser.TryParseProduct(e, out p) ? _products : _invalidProducts).AddLast(p)
                    });

                SaveProductsImpl();
            }
            catch (Exception e)
            {
                _exception = e;
            }
            finally
            {
                _stopWatch.Stop();
            }

            return new YmlResult(this);
        }

        private void AddCategory(Category cat) => _categories.AddLast(cat);

        private void AddProduct(Product product)
        {
            // будем флашить данные пачками когда набертся нужное количество
            if (_products.Count < _bufferSize)
            {
                _products.AddLast(product);
            }
            else
            {
                var awaiter = SaveProductsImpl()
                    .ConfigureAwait(false).GetAwaiter();

                awaiter.GetResult();
            }
        }

        private async Task SaveProductsImpl()
        {
            if (_products.Count > 0)
            {
                await _productSaver.SaveProducts(_products);

                _products.Clear();
            }
        }

        public YmlResult FromException(Exception e)
        {
            _exception = e;

            var res = new YmlResult(this);

            return res;
        }

        public class YmlResult
        {
            public YmlResult(YmlStreamProcessor processor)
            {
                Url = processor.Url;
                Exception = processor._exception;
                ElapsedMilliseconds = processor._stopWatch.ElapsedMilliseconds;
                Processed = processor._products.Count;
                Invalid = processor._invalidProducts.Count;
                Categories = processor._categories.Count;
                Success = !processor._stopWatch.IsRunning && processor._exception != null;
            }

            private YmlResult() { }

            public string Url { get; }
            public Exception Exception { get; }
            public long ElapsedMilliseconds { get; }
            public int Processed { get; }
            public int Invalid { get; }
            public int Categories { get; }
            public bool Success { get; }
        }
    }
}
