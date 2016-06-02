using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YMLDownloader
{
    // Можно прочитать из бд
    public class ResourceProvider
    {
        public IEnumerable<string> GetResources()
        {
            yield return @"http://static.ozone.ru/multimedia/yml/facet/mobile_catalog/1133677.xml";
            yield return @"http://static.ozone.ru/multimedia/yml/facet/div_soft.xml";
            yield return @"http://static.ozone.ru/multimedia/yml/facet/business.xml";

        }
    }
}
