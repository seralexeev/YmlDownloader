using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace YMLDownloader
{
    public class YmlStreamReader
    {
        // В спецификации не нашел может ли категории быть после продуктов
        // т.к смысл в потоковой обработке, поэтому будут экшены для каждого
        // ... класс потому что так лучше отражает семантику и писать короче
        public class HandlersCollection : Dictionary<string, Action<XElement>> { }

        public IEnumerable<XElement> FindElements(Stream stream, string elementName)
        {
            var settings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Parse
            };

            using (var reader = XmlReader.Create(stream, settings))
            {
                foreach (var elem in FindElements(reader, elementName))
                {
                    yield return elem;
                }
            }
        }

        public IEnumerable<XElement> FindElements(XmlReader reader, string elementName)
        {
            reader.MoveToContent();
            reader.Read();
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals(elementName))
                {
                    var matchedElement = XNode.ReadFrom(reader) as XElement;
                    if (matchedElement != null)
                        yield return matchedElement;
                }
                else
                    reader.Read();
            }
        }

        public void ProcessElements(Stream stream, HandlersCollection handlers)
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore
            };

            using (var reader = XmlReader.Create(stream, settings))
            {
                ProcessElements(reader, handlers);
            }
        }

        public void ProcessElements(XmlReader reader, HandlersCollection handlers)
        {
            reader.MoveToContent();
            reader.Read();
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                var nodeName = reader.Name;
                if (reader.NodeType == XmlNodeType.Element
                    && handlers.ContainsKey(nodeName))
                {
                    var matchedElement = XNode.ReadFrom(reader) as XElement;
                    if (matchedElement != null)
                    {
                        // Вызов делегата медленнее
                        // может ли это быть боттлнеком
                        // ... побенчмаркать бы
                        handlers[nodeName](matchedElement);
                    }
                }
                else
                {
                    reader.Read();
                }
            }
        }
    }
}
