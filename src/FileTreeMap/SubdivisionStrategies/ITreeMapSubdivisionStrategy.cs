using System.Collections.Generic;
using System.Windows;

namespace FileTreeMap.SubdivisionStrategies
{
    public interface ISubdivisionStrategy
    {
        IEnumerable<Rect> Subdivide(Rect rectangle, IEnumerable<double> rectangleAreas);
    }
}
