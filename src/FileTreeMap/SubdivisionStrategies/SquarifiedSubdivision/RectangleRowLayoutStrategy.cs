using System.Collections.Generic;
using System.Windows;

namespace FileTreeMap.SubdivisionStrategies.SquarifiedSubdivision
{
    internal delegate IEnumerable<Rect> RectangleRowLayoutStrategy(Rect parentRectangle, IEnumerable<double> rectangleAreas);
}
