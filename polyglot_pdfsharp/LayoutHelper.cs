using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace polyglot_pdfsharp
{
    public class LayoutHelper
    {
        private readonly PdfDocument _document;
        private readonly XUnit _topPosition;
        private readonly XUnit _bottomMargin;
        private readonly XUnit _leftMargin;
        private readonly XUnit _rightMargin;
        private XUnit _currentPosition;

        public LayoutHelper(
            PdfDocument document, 
            XUnit topPosition, 
            XUnit bottomMargin,
            XUnit leftMargin,
            XUnit rightMargin)
        {
            _document = document;
            _topPosition = topPosition;
            _bottomMargin = bottomMargin;
            _currentPosition = _topPosition;
            _leftMargin = leftMargin;
            _rightMargin = rightMargin;
            // Set a value outside the page - a new page will be created on the first request.
            //bottomMargin + 10000;
            CreatePage();
        }

        public XUnit GetLinePosition(XUnit requestedHeight)
        {
            return GetLinePosition(requestedHeight, -1f);
        }

        public XUnit GetLinePosition(XUnit requestedHeight, XUnit requiredHeight)
        {
            XUnit required = requiredHeight == -1f ? requestedHeight : requiredHeight;
            if (_currentPosition + required > _bottomMargin)
                CreatePage();
            XUnit result = _currentPosition;
            _currentPosition += requestedHeight;
            return result;
        }

        public string[] GetLines(XUnit leftIndent, string testFit, XFont font)
        {
            XSize sizeTestFit = Gfx.MeasureString(testFit, font);
            double targetWidth = _rightMargin - (_leftMargin + leftIndent);
            if (sizeTestFit.Width > targetWidth)
            {
                List<string> stringLines = new List<string>();
                bool done = false;
                int i = testFit.Length;
                string shorter = testFit;
                string spill = testFit;
                while (!done)
                {
                    XSize smallestNonFitting = Gfx.MeasureString(shorter, font);
                    if (smallestNonFitting.Width < targetWidth)
                    {
                        // this means that contrary to variable
                        // name it does fit
                        stringLines.Add(shorter.Trim());
                        done = true;
                        break;
                    }
                    i = shorter.LastIndexOf(' ');
                    string newShorter = shorter.Substring(0, i);
                    XSize largestFitting = Gfx.MeasureString(newShorter, font);
                    if (smallestNonFitting.Width > targetWidth &&
                        largestFitting.Width <= targetWidth)
                    {
                        spill = spill.Substring(newShorter.Length);
                        shorter = spill;
                        stringLines.Add(newShorter.Trim());
                    }
                    else
                    {
                        shorter = newShorter;
                    }

                    if (string.IsNullOrWhiteSpace(shorter))
                    {
                        done = true;
                    }
                }

                return stringLines.ToArray();
            }
            else
            {
                return new string[] { testFit };
            }
        }

        public XGraphics Gfx { get; private set; }
        public PdfPage Page { get; private set; }

        public void CreatePage()
        {
            Page = _document.AddPage();
            Page.Size = PageSize.Letter;
            Gfx = XGraphics.FromPdfPage(Page);
            _currentPosition = _topPosition;
        }

        public void PrintCentered(string text, XFont font)
        {
            string[] lines = this.GetLines(0, text, font);

            for (int i = 0; i < lines.Length; i++)
            {
                XSize size = this.Gfx.MeasureString(lines[i], font);
                XUnit lineTop = this.GetLinePosition(size.Height);
                XUnit leftIndentByCentering = (this._rightMargin - this._leftMargin - size.Width) / 2d;
                XPoint p = new XPoint(this._leftMargin + leftIndentByCentering, lineTop);
                this.Gfx.DrawString(lines[i], font, XBrushes.Black, p);
            }
        }

        public void PrintChapter(int chapter, XFont font)
        {
            string text = string.Format("Chapter {0}", chapter);
            XSize size = this.Gfx.MeasureString(text, font);
            XUnit lineTop = this.GetLinePosition(size.Height);
            XPoint p = new XPoint(this._leftMargin, lineTop);
            this.Gfx.DrawString(text, font, XBrushes.Black, p);
        }

        public void PrintVerse(int verse, XUnit indentLeft, XFont font, string text)
        {
            string[] lines = this.GetLines(indentLeft, text, font);

            for (int i = 0; i < lines.Length; i++)
            {
                XSize size = this.Gfx.MeasureString(lines[i], font);
                XUnit lineTop = this.GetLinePosition(size.Height);

                if (i == 0)
                {
                    XPoint np = new XPoint(this._leftMargin, lineTop);
                    this.Gfx.DrawString(string.Format("{0}.", verse), font, XBrushes.Black, np);
                }

                XPoint p = new XPoint(this._leftMargin + indentLeft, lineTop);
                this.Gfx.DrawString(lines[i], font, XBrushes.Black, p);
            }
        }
    }
}
