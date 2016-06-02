using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using YMLDownloader;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using System;
using static YMLDownloader.YmlStreamReader;

namespace YMLDownloader.UnitTests
{
    [TestFixture]
    public class ProductParserTests
    {
        public static TestDataProvider GetProvider()
        {
            return new TestDataProvider();
        }

        [Test]
        public void ProcessElements_CorrectData_DelegatesInvoked()
        {
            var reader = new YmlStreamReader();
            var parser = new XmlProductParser();
            var categories = new LinkedList<Category>();
            var products = new LinkedList<Product>();

            using (var stream = GetProvider().GetFileStream("correct_data1.xml"))
            {
                reader.ProcessElements(stream,
                    new HandlersCollection
                    {
                        ["category"] = e => categories.AddLast(parser.ParseCategory(e)),
                        ["offer"] = e => products.AddLast(parser.ParseProduct(e))
                    });
            }

            Assert.That(categories.Count, Is.EqualTo(1));
            Assert.That(products.Count, Is.EqualTo(2));
        }

        [Test]
        public void FindElements_CorrectData_ReturnsElements()
        {
            var parser = new YmlStreamReader();

            using (var stream = GetProvider().GetFileStream("correct_data1.xml"))
            {
                var result = parser.FindElements(stream, "offer").ToList();

                Assert.That(result, Is.Not.Null);
                Assert.That(result.Count, Is.EqualTo(2));
            }
        }

        [TestCaseSource(nameof(GetProductsTestData))]
        public void Parse_CorrectData_ExpectedProducts(Product expectedProduct, XElement el)
        {
            var parser = new XmlProductParser();

            //var res = parser.ParseProduct(el);

            res.ShouldBeEquivalentTo(expectedProduct);
        }

        public static IEnumerable<TestCaseData> GetProductsTestData()
        {
            var parser = new YmlStreamReader();

            using (var stream = GetProvider().GetFileStream("correct_data1.xml"))
            {
                var elemEnumerator = parser
                    .FindElements(stream, "offer").GetEnumerator();

                var productEnumerator = GetProducts();

                while (productEnumerator.MoveNext() & elemEnumerator.MoveNext())

                    yield return new TestCaseData(
                        productEnumerator.Current, elemEnumerator.Current);
            }
        }

        public static IEnumerator<Product> GetProducts()
        {
            yield return new Product
            {
                CategoryID = 1133677,
                Name = "Philips Xenium E103, Red",
                Price = 1690,
                Vendor = "Philips"
            };

            yield return new Product
            {
                CategoryID = 1133677,
                Name = "Philips Xenium E103, Black",
                Price = 1690,
                Vendor = "Philips"
            };
        }
    }
}
