using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YMLDownloader.UnitTests
{
    public class TestDataProvider
    {
        public Stream GetFileStream(string filename = "data1.xml") => 
            Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"YMLDownloader.UnitTests.Testdata.{filename}");
    }
}
