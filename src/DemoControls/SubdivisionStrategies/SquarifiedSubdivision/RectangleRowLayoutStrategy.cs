using System.Collections.Generic;
using System.Windows;

namespace DemoControls.SubdivisionStrategies.SquarifiedSubdivision
{
    internal delegate IEnumerable<Rect> RectangleRowLayoutStrategy(Rect parentRectangle, IEnumerable<double> rectangleAreas);
}
