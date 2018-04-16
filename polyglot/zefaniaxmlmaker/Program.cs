using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace zefaniaxmlmaker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                System.Console.WriteLine("usage: zefaniaxmlmaker inputpath outfile");
                return;
            }

            string inputPath = args[0];
            string outfile = args[1];

            XmlDocument doc = new XmlDocument();
            XmlNode xmlBibleNode = doc.CreateElement("XMLBIBLE");
            doc.AppendChild(xmlBibleNode);

            BibleConfiguration.ApplyBibleConfiguration(xmlBibleNode, inputPath);

            var bookDirs = Directory.EnumerateDirectories(Path.Combine(inputPath, "books"), "*");
            foreach (var bookDir in bookDirs)
            {
                DirectoryInfo dib = new DirectoryInfo(bookDir);
                int bookNumber;
                if (int.TryParse(dib.Name, out bookNumber))
                {
                    string chapterBaseDirectory = Path.Combine(bookDir, "chapters");
                    if (Directory.Exists(chapterBaseDirectory))
                    {
                        XmlNode bibleBook = AddBibleBook(xmlBibleNode, bookDir);
                        var chapterDirs = Directory.EnumerateDirectories(chapterBaseDirectory, "*");
                        foreach (var chapterDir in chapterDirs)
                        {
                            DirectoryInfo dic = new DirectoryInfo(chapterDir);
                            int chapterNumber;
                            if (int.TryParse(dic.Name, out chapterNumber))
                            {
                                XmlNode chapterNode = AddChapter(bibleBook, chapterDir);

                                string versesBaseDirectory = Path.Combine(chapterDir, "verses");
                                if (Directory.Exists(versesBaseDirectory))
                                {
                                    var verseDirs = Directory.EnumerateDirectories(versesBaseDirectory, "*");
                                    foreach (var verseDir in verseDirs)
                                    {
                                        DirectoryInfo div = new DirectoryInfo(verseDir);
                                        int verseNumber;
                                        if (int.TryParse(div.Name, out verseNumber))
                                        {
                                            XmlNode verseNode = AddVerse(chapterNode, verseDir);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            doc.Save(outfile);
        }

        static XmlNode AddBibleBook(XmlNode parentNode, string path)
        {
            XmlNode bibleBook = parentNode.OwnerDocument.CreateElement("BIBLEBOOK");
            parentNode.AppendChild(bibleBook);
            string bookNameFilename = Path.Combine(path, "book_title_simple.txt");
            string bookName = File.ReadAllText(bookNameFilename).Trim(" \r\n\t".ToCharArray());
            string bookShortNameFilename = Path.Combine(path, "book_title_short.txt");
            string bookShortName = File.ReadAllText(bookShortNameFilename).Trim(" \r\n\t".ToCharArray());
            DirectoryInfo di = new DirectoryInfo(path);
            int bookNumber = int.Parse(di.Name);
            AddAttribute(bibleBook, "bnumber", bookNumber.ToString());
            AddAttribute(bibleBook, "bname", bookName);
            AddAttribute(bibleBook, "bsname", bookShortName);
            return bibleBook;
        }

        static XmlNode AddChapter(XmlNode parentNode, string path)
        {
            XmlNode chapterNode = parentNode.OwnerDocument.CreateElement("CHAPTER");
            parentNode.AppendChild(chapterNode);
            DirectoryInfo di = new DirectoryInfo(path);
            int chapterNumber = int.Parse(di.Name);
            AddAttribute(chapterNode, "cnumber", chapterNumber.ToString());
            return chapterNode;
        }

        static XmlNode AddVerse(XmlNode parentNode, string path)
        {
            XmlNode verseNode = parentNode.OwnerDocument.CreateElement("VERS");
            parentNode.AppendChild(verseNode);
            DirectoryInfo di = new DirectoryInfo(path);
            int verseNumber = int.Parse(di.Name);
            AddAttribute(verseNode, "vnumber", verseNumber.ToString());
            string verseFilename = Path.Combine(path, "verse.txt");
            string verseText = File.ReadAllText(verseFilename).Trim(" \r\n\t".ToCharArray());
            verseNode.InnerText = verseText;
            return verseNode;
        }

        static void AddAttribute(XmlNode node, string key, string value)
        {
            XmlAttribute attribute = node.OwnerDocument.CreateAttribute(key);
            node.Attributes.Append(attribute);
            attribute.Value = value;
        }
    }
}
