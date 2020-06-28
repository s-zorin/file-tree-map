using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace DemoControls.SubdivisionStrategies.SquarifiedSubdivision
{
    internal class SubdivideResult
    {
        public Rect RemainingRectangle { get; set; }

        public IEnumerable<Rect> Rectangles { get; set; }

        public bool IsSubdivisionCompleted { get; set; }

        public SubdivideResult(Rect remainingRectangle, IEnumerable<Rect> rectangles, bool isSubdvisionCompleted)
        {
            RemainingRectangle = remainingRectangle;
            Rectangles = rectangles;
            IsSubdivisionCompleted = isSubdvisionCompleted;
        }
    }
}
