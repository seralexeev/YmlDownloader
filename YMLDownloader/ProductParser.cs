using System.Xml.Linq;

namespace YMLDownloader
{
    public class XmlProductParser
    {
        private delegate bool TryParseDelegate<T>(string value, out T result);
        private static TryParseDelegate<double> s_tryDouble = double.TryParse;
        private static TryParseDelegate<long> s_tryLong = long.TryParse;

        private T TryParse<T>(string value, TryParseDelegate<T> handler, ref bool success) where T : struct
        {
            T result;
            if (!string.IsNullOrEmpty(value) && handler(value, out result))
                return result;

            success = false;
            return default(T);
        }

        public bool TryParseProduct(XElement element, out Product product)
        {
            var success = true;
            product = new Product
            {
                Name = element.Element("name")?.Value,
                Price = TryParse(element.Element("price")?.Value, s_tryDouble, ref success),
                Vendor = element.Element("vendor")?.Value,
                CategoryID = TryParse(element.Element("categoryId")?.Value, s_tryLong, ref success)
            };

            return success;
        }

        public bool TryParseCategory(XElement element, out Category cat)
        {
            var success = true;
            cat = new Category
            {
                Name = element?.Value,
                ID = TryParse(element.Attribute("id")?.Value, s_tryLong, ref success),
            };

            return success;
        }
    }
}
