using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YMLDownloader
{
    public class ProductSaver
    {
        private ConnectionFactory _factory;

        public ProductSaver(ConnectionFactory factory)
        {
            _factory = factory;
        }

        // Тут неплохо сделать бы булк инсерт
        public async Task SaveProducts(IEnumerable<Product> products)
        {
            const string query = @"insert into [products] ([name], [price], [vendor]) values (@name, @price, @vendor)";

            using (var con = _factory.Create())
                await con.ExecuteAsync(query, products);
        }
    }
}
