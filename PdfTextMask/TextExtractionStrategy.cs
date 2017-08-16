using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf.parser;

namespace PdfTextMask
{
    public class TextExtractionStrategy : LocationTextExtractionStrategy
    {
        private List<Rectangle> _rectangles = new List<Rectangle>();

        public TextExtractionStrategy(string search)
        {
            if (search == null)
                throw new ArgumentNullException(nameof(search));

            if (search.Length == 0)
                throw new ArgumentException(null, nameof(search));

            Search = search;
        }

        public IReadOnlyList<Rectangle> Rectangles => _rectangles;
        public string Search { get; }

        public override void RenderText(TextRenderInfo renderInfo)
        {
            base.RenderText(renderInfo);

            var pos = renderInfo.GetText().IndexOf(Search, StringComparison.CurrentCultureIgnoreCase);
            if (pos < 0)
                return;

            var chars = renderInfo.GetCharacterRenderInfos();
            var start = chars[pos].GetDescentLine().GetStartPoint();
            var end = chars[pos + Search.Length - 1].GetAscentLine().GetEndPoint();
            var rect = new Rectangle(start[Vector.I1], start[Vector.I2], end[Vector.I1], end[Vector.I2]);
            _rectangles.Add(rect);
        }
    }
}
