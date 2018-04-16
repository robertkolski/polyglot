using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace zefaniaxmlmaker
{
    public static class BibleConfiguration
    {
        public static void ApplyBibleConfiguration(XmlNode xmlBibleNode, string inputPath)
        {
            XmlNode informationNode = xmlBibleNode.OwnerDocument.CreateElement("INFORMATION");
            xmlBibleNode.AppendChild(informationNode);

            string filename = System.IO.Path.Combine(inputPath, "bible.config");
            XmlDocument configDoc = new XmlDocument();
            configDoc.Load(filename);
            CopyElement(configDoc, @"bible/title", informationNode, "title");
            CopyElement(configDoc, @"bible/creator", informationNode, "creator");
            CopyElement(configDoc, @"bible/subject", informationNode, "subject");
            CopyElement(configDoc, @"bible/description", informationNode, "description");
            CopyElement(configDoc, @"bible/publisher", informationNode, "publisher");
            CopyElement(configDoc, @"bible/contributors", informationNode, "contributors");
            AddElement(informationNode, "date", DateTime.Now.ToString("YYYY-MM-dd"));
            AddElement(informationNode, "type", "Bible");
            AddElement(informationNode, "format", "Zefania XML Bible Markup Language");
            CopyElement(configDoc, @"bible/contributors", informationNode, "contributors");
            CopyElement(configDoc, @"bible/identifier", informationNode, "identifier");
            CopyElement(configDoc, @"bible/source", informationNode, "source");
            CopyElement(configDoc, @"bible/language", informationNode, "language");
            CopyElement(configDoc, @"bible/coverage", informationNode, "coverage");
            CopyElement(configDoc, @"bible/rights", informationNode, "rights");
        }

        private static void CopyElement(XmlNode source, string xpathSource, XmlNode dest, string destName)
        {
            XmlNode input = source.SelectSingleNode(xpathSource);
            XmlNode output = dest.OwnerDocument.CreateElement(destName);
            dest.AppendChild(output);
            output.InnerText = input.InnerText;
        }

        private static void AddElement(XmlNode dest, string destName, string innerText)
        {
            XmlNode output = dest.OwnerDocument.CreateElement(destName);
            dest.AppendChild(output);
            output.InnerText = innerText;
        }
    }
}
