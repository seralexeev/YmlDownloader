using System.Collections.Generic;

namespace YMLDownloader
{
    public interface IProductSaver
    {
        void SaveProducts(IEnumerable<Product> products);
        void SaveCategories(IEnumerable<Category> products);
    }
}