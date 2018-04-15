using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Diagnostics;
using System.Xml;

namespace polyglot_pdfsharp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                System.Console.Out.WriteLine("usage: polyglot_pdfsharp filename outfile");
                return;
            }

            string filename = args[0];
            string outfile = args[1];

            PdfDocument doc = new PdfDocument();
            LayoutHelper layoutHelper = new LayoutHelper(doc, XUnit.FromInch(1.0), XUnit.FromInch(10.0), XUnit.FromInch(1.0), XUnit.FromInch(7.5));

            XFont font = new XFont("Verdana", 12);
            XUnit indentLeft = XUnit.FromInch(.5);

            XmlDocument xml = new XmlDocument();
            xml.Load(filename);
            foreach (XmlNode bookNode in xml.SelectNodes(@"//BIBLEBOOK"))
            {
                string bookName = bookNode.Attributes["bname"].Value;
                layoutHelper.PrintCentered(bookName, font);
                foreach (XmlNode chapterNode in bookNode.SelectNodes("CHAPTER"))
                {
                    int chapterNumber = int.Parse(chapterNode.Attributes["cnumber"].Value);
                    layoutHelper.PrintChapter(chapterNumber, font);
                    foreach (XmlNode verseNode in chapterNode.SelectNodes("VERS"))
                    {
                        int verseNumber = int.Parse(verseNode.Attributes["vnumber"].Value);
                        string verseText = verseNode.InnerText;
                        layoutHelper.PrintVerse(verseNumber, indentLeft, font, verseText);
                    }
                }
            }

            // Save the document... 
            doc.Save(outfile);
        }
    }
}
