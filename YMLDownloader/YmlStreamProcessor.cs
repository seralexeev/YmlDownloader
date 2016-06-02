using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using static YMLDownloader.YmlStreamReader;

namespace YMLDownloader
{
    public class YmlStreamProcessor
    {
        private Dictionary<long, Category> _categories { get; } = new Dictionary<long, Category>();
        private int _categoriesCount = 0;

        private LinkedList<Product> _products { get; } = new LinkedList<Product>();
        private int _productsCount = 0;
        public int _invalidProducts = 0;

        private readonly int _bufferSize;
        private readonly IProductSaver _productSaver;
        private YmlStreamReader _reader;
        private Stopwatch _stopWatch;
        private Exception _exception;
        private XmlProductParser _parser;
        private Validator _validator;
        private ILogger _logger;

        public string Url { get; }

        public YmlStreamProcessor(string url,
            YmlStreamReader reader, XmlProductParser parser, Validator validator, ILogger logger,
            IProductSaver productSaver, int bufferSize)
        {
            _reader = reader;
            _parser = parser;
            _validator = validator;
            _logger = logger;
            _productSaver = productSaver;
            _bufferSize = bufferSize;
            _stopWatch = Stopwatch.StartNew();

            Url = url;
        }

        public YmlResult Process(Stream stream)
        {
            try
            {
                _reader.ProcessElements(stream,
                    new HandlersCollection
                    {
                        ["offer"] = e => HandleProduct(e),
                        ["category"] = e => HandleCategory(e),
                    }, HandleSwitch);
            }
            catch (Exception e)
            {
                _exception = e;
            }


            if (_products.Count > 0)
                SaveProducts();

            _stopWatch.Stop();

            return new YmlResult(this);
        }

        private void HandleSwitch(string from, string to)
        {
            if (from == "category")
                _productSaver.SaveCategories(_categories.Values);
        }

        private void HandleProduct(XElement el)
        {
            Product p;
            if (_parser.TryParseProduct(el, out p))
            {
                ValidateResult res;
                if ((res = _validator.Validate(p)).IsValid)
                {
                    if (CategoryExist(p))
                    {
                        AddProduct(p);
                        _productsCount++;
                    }
                    else
                    {
                        _logger.Write($"Product [{p.ID}] has invalid category: {p.CategoryID}");
                        _invalidProducts++;
                    }
                }
                else
                {
                    _logger.Write($"Product [{p.ID}] invalid: {res.Message}");
                    _invalidProducts++;
                }
            }
            else
            {
                _logger.Write($"Unable to parse: {el}");
                _invalidProducts++;
            }
        }

        bool CategoryExist(Product p) => _categories.ContainsKey(p.CategoryID);

        private void HandleCategory(XElement el)
        {
            Category p;
            if (_parser.TryParseCategory(el, out p))
            {
                ValidateResult res;
                if ((res = _validator.Validate(p)).IsValid)
                {
                    _categories.Add(p.ID, p);
                    _categoriesCount++;
                }
                else
                {
                    _logger.Write($"Product invalid: {res.Message}");
                }
            }
            else
            {
                _logger.Write($"Unable to parse: {el}");
            }
        }

        private void AddProduct(Product product)
        {
            // будем флашить данные пачками когда набертся нужное количество
            if (_products.Count < _bufferSize)
            {
                _products.AddLast(product);
            }
            else
            {
                SaveProducts();
            }
        }

        private void SaveProducts()
        {
            _productSaver.SaveProducts(_products);
            _products.Clear();
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
                Processed = processor._productsCount;
                Invalid = processor._invalidProducts;
                Categories = processor._categoriesCount;
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
