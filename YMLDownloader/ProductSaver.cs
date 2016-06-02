using Dapper;
using System.Collections.Generic;

namespace YMLDownloader
{
    public class ProductSaver : IProductSaver
    {
        private ConnectionFactory _factory;

        public ProductSaver(ConnectionFactory factory)
        {
            _factory = factory;
        }

        // Тут неплохо сделать бы булк инсерт
        public void SaveProducts(IEnumerable<Product> products)
        {
            const string query = @"insert into [products] ([id], [name], [price], [categoryId]) values (@id, @name, @price, @categoryId)";

            using (var con = _factory.Create())
                con.Execute(query, products);
        }

        public void SaveCategories(IEnumerable<Category> products)
        {
            const string query = @"insert into [categories] ([id], [name]) values (@id, @name)";

            using (var con = _factory.Create())
                con.Execute(query, products);
        }
    }
}
