using System;
using System.Collections;
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
        public class HandlersCollection
        {
            private Dictionary<string, Action<XElement>> _handlers =
                new Dictionary<string, Action<XElement>>();

            public Action<XElement> this[string key]
            {
                set { _handlers[key] = value; }
            }

            public bool CanBeHandled(string key) => _handlers.ContainsKey(key);

            public void Handle(string node, XElement elem) => _handlers[node](elem);
        }

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

        public void ProcessElements(Stream stream, HandlersCollection handlers, Action<string, string> switchHandle = null)
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore
            };

            using (var reader = XmlReader.Create(stream, settings))
            {
                ProcessElements(reader, handlers, switchHandle);
            }
        }

        public void ProcessElements(XmlReader reader, HandlersCollection handlers, Action<string, string> switchHandle = null)
        {
            reader.MoveToContent();
            reader.Read();
            string current = null;
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                var nodeName = reader.Name;

                if (!string.IsNullOrEmpty(nodeName) && current != nodeName)
                {
                    var tmp = current;
                    current = nodeName;

                    if (tmp != null && switchHandle != null)
                        switchHandle(tmp, current);
                }

                if (reader.NodeType == XmlNodeType.Element
                    && handlers.CanBeHandled(nodeName))
                {
                    var matchedElement = XNode.ReadFrom(reader) as XElement;
                    if (matchedElement != null)
                    {
                        handlers.Handle(nodeName, matchedElement);
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
