using System.Collections.Generic;
using System.Windows;

namespace DemoControls.SubdivisionStrategies
{
    public interface ISubdivisionStrategy
    {
        IEnumerable<Rect> Subdivide(Rect rectangle, IEnumerable<double> rectangleAreas);
    }
}
