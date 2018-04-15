using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace polyglot_parallel
{
    public class LayoutHelper
    {
        private readonly PdfDocument _document;
        private readonly XUnit _topPosition;
        private readonly XUnit _bottomMargin;
        private readonly XUnit[] _leftMargin;
        private readonly XUnit[] _rightMargin;
        private XUnit _currentPosition;

        public LayoutHelper(
            PdfDocument document, 
            XUnit topPosition, 
            XUnit bottomMargin,
            XUnit[] leftMargin,
            XUnit[] rightMargin)
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

        public Tuple<XUnit, XGraphics> GetLinePosition(XUnit requestedHeight)
        {
            return GetLinePosition(requestedHeight, -1f);
        }

        public Tuple<XUnit, XGraphics> GetLinePosition(XUnit requestedHeight, XUnit requiredHeight)
        {
            XUnit required = requiredHeight == -1f ? requestedHeight : requiredHeight;
            if (_currentPosition + required > _bottomMargin)
                CreatePage();
            XUnit result = _currentPosition;
            _currentPosition += requestedHeight;
            return Tuple.Create(result, this.Gfx);
        }

        public string[,] GetLines(XUnit leftIndent, string[] testFit, XFont font)
        {
            bool doesNotFit = false;
            for (int i = 0; i < testFit.Length; i++)
            {
                XSize sizeTestFit = Gfx.MeasureString(testFit[i], font);
                double targetWidth = _rightMargin[i] - (_leftMargin[i] + leftIndent);
                if (sizeTestFit.Width > targetWidth)
                {
                    doesNotFit = true;
                }
            }
            if (!doesNotFit)
            {
                string[,] temp = new string[testFit.Length, 1];
                for (int i = 0; i <testFit.Length; i++)
                {
                    temp[i, 0] = testFit[i];
                }
                return temp;
            }

            List<string[]> stringLines = new List<string[]>();
            for (int i = 0; i < testFit.Length; i++)
            {
                XSize sizeTestFit = Gfx.MeasureString(testFit[i], font);
                double targetWidth = _rightMargin[i] - (_leftMargin[i] + leftIndent);
                if (sizeTestFit.Width > targetWidth)
                {
                    bool done = false;
                    int j = testFit.Length;
                    string shorter = testFit[i];
                    string spill = testFit[i];
                    int k = 0;
                    while (!done)
                    {
                        XSize smallestNonFitting = Gfx.MeasureString(shorter, font);
                        if (smallestNonFitting.Width < targetWidth)
                        {
                            // this means that contrary to variable
                            // name it does fit
                            if (i == 0)
                            {
                                string[] temp = new string[testFit.Length];
                                temp[0] = shorter.Trim();
                                stringLines.Add(temp);
                            }
                            else
                            {
                                if (stringLines.Count <= k)
                                {
                                    stringLines.Add(new string[testFit.Length]);
                                }
                                stringLines[k][i] = shorter.Trim();
                            }
                            done = true;
                            break;
                        }
                        j = shorter.LastIndexOf(' ');
                        string newShorter = shorter.Substring(0, j);
                        XSize largestFitting = Gfx.MeasureString(newShorter, font);
                        if (smallestNonFitting.Width > targetWidth &&
                            largestFitting.Width <= targetWidth)
                        {
                            spill = spill.Substring(newShorter.Length);
                            shorter = spill;
                            if (i == 0)
                            {
                                string[] temp = new string[testFit.Length];
                                temp[0] = newShorter.Trim();
                                stringLines.Add(temp);
                            }
                            else
                            {
                                if (stringLines.Count <= k)
                                {
                                    stringLines.Add(new string[testFit.Length]);
                                }
                                stringLines[k][i] = newShorter.Trim();
                                k++;
                            }

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
                }
            }
            string[,] result = new string[testFit.Length, stringLines.Count];
            for (int i = 0; i < testFit.Length; i++)
            {
                for (int j = 0; j < stringLines.Count; j++)
                {
                    result[i, j] = stringLines[j][i];
                }
            }
            return result;
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

        public void PrintCentered(string[] text, XFont font)
        {
            string[,] lines = this.GetLines(0, text, font);

            Tuple<XUnit,XGraphics>[] lineTop = new Tuple<XUnit, XGraphics>[lines.GetLength(1)];

            for (int i = 0; i < lines.GetLength(0); i++)
            {
                for (int j = 0; j < lines.GetLength(1); j++)
                {
                    XSize size = this.Gfx.MeasureString(lines[i,j] ?? "A", font);
                    if (i == 0)
                    {
                        lineTop[j] = this.GetLinePosition(size.Height);
                    }
                    XUnit leftIndentByCentering = (this._rightMargin[i] - this._leftMargin[i] - size.Width) / 2d;
                    XPoint p = new XPoint(this._leftMargin[i] + leftIndentByCentering, lineTop[j].Item1);
                    if (lines[i, j] != null)
                    {
                        lineTop[j].Item2.DrawString(lines[i, j], font, XBrushes.Black, p);
                    }
                }
            }
        }

        public void PrintChapter(int chapter, XFont font)
        {
            Tuple<XUnit, XGraphics> lineTop = new Tuple<XUnit, XGraphics>(default(XUnit), null);
            for (int i = 0; i < this._leftMargin.Length; i++)
            {
                string text = string.Format("Chapter {0}", chapter);
                XSize size = this.Gfx.MeasureString(text, font);
                if (i == 0)
                {
                    lineTop = this.GetLinePosition(size.Height);
                }
                XPoint p = new XPoint(this._leftMargin[i], lineTop.Item1);
                lineTop.Item2.DrawString(text, font, XBrushes.Black, p);
            }
        }

        public void PrintVerse(int verse, XUnit indentLeft, XFont font, string[] text)
        {
            string[,] lines = this.GetLines(indentLeft, text, font);

            Tuple<XUnit,XGraphics>[] lineTop = new Tuple<XUnit,XGraphics>[lines.GetLength(1)];

            for (int i = 0; i < lines.GetLength(0); i++)
            {
                for (int j = 0; j < lines.GetLength(1); j++)
                {
                    XSize size = this.Gfx.MeasureString(lines[i, j] ?? "A", font);
                    if (i == 0)
                    {
                        lineTop[j] = this.GetLinePosition(size.Height);
                    }
                    if (j == 0)
                    {
                        XPoint np = new XPoint(this._leftMargin[i], lineTop[j].Item1);
                        lineTop[j].Item2.DrawString(string.Format("{0}.", verse), font, XBrushes.Black, np);
                    }

                    XPoint p = new XPoint(this._leftMargin[i] + indentLeft, lineTop[j].Item1);
                    if (lines[i, j] != null)
                    {
                        lineTop[j].Item2.DrawString(lines[i, j], font, XBrushes.Black, p);
                    }
                }
            }
        }
    }
}
