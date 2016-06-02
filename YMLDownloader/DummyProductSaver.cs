using System.Collections.Generic;

namespace YMLDownloader
{
    public class DummyProductSaver : IProductSaver
    {
        public void SaveProducts(IEnumerable<Product> products) { }
        
        public void SaveCategories(IEnumerable<Category> products) { }
    }
}
