using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace polyglot_parallel
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                System.Console.Out.WriteLine("usage: polyglot_pdfsharp filename1 filename2 outfile");
                return;
            }

            string filename1 = args[0];
            string filename2 = args[1];
            string outfile = args[2];

            PdfDocument doc = new PdfDocument();
            XUnit[] leftMargin = new XUnit[] { XUnit.FromInch(1.0), XUnit.FromInch(4.5) };
            XUnit[] rightMargin = new XUnit[] { XUnit.FromInch(4.0), XUnit.FromInch(7.5) };
            LayoutHelper layoutHelper = new LayoutHelper(doc, XUnit.FromInch(1.0), XUnit.FromInch(10.0), leftMargin, rightMargin);

            XFont font = new XFont("Verdana", 12);
            XUnit indentLeft = XUnit.FromInch(.5);

            XmlDocument xml = new XmlDocument();
            xml.Load(filename1);
            XmlDocument xml2 = new XmlDocument();
            xml2.Load(filename2);
            foreach (XmlNode bookNode in xml.SelectNodes(@"//BIBLEBOOK"))
            {
                int bookNumber = int.Parse(bookNode.Attributes["bnumber"].Value);
                string bookName = bookNode.Attributes["bname"].Value;
                string[] bookArray = new string[2];
                bookArray[0] = bookName;

                XmlNode bookNode2 = xml2.SelectSingleNode(string.Format(@"//BIBLEBOOK[@bnumber='{0}']", bookNumber));

                bookArray[1] = bookNode2.Attributes["bname"].Value;

                layoutHelper.PrintCentered(bookArray, font);
                foreach (XmlNode chapterNode in bookNode.SelectNodes("CHAPTER"))
                {
                    int chapterNumber = int.Parse(chapterNode.Attributes["cnumber"].Value);

                    XmlNode chapterNode2 = bookNode2.SelectSingleNode(string.Format("CHAPTER[@cnumber='{0}']", chapterNumber));

                    layoutHelper.PrintChapter(chapterNumber, font);
                    foreach (XmlNode verseNode in chapterNode.SelectNodes("VERS"))
                    {
                        int verseNumber = int.Parse(verseNode.Attributes["vnumber"].Value);
                        string verseText = verseNode.InnerText;
                        string[] verseArray = new string[2];
                        verseArray[0] = verseText;

                        XmlNode verseNode2 = chapterNode2.SelectSingleNode(string.Format("VERS[@vnumber='{0}']", verseNumber));

                        verseArray[1] = verseNode2.InnerText;

                        layoutHelper.PrintVerse(verseNumber, indentLeft, font, verseArray);

                        //if (bookNumber >= 5 && chapterNumber >= 5 && verseNumber >= 10)
                        //{
                        //    goto test;
                        //}
                    }
                }
            }

            //test:

            // Save the document... 
            doc.Save(outfile);
        }
    }
}
