using DemoControls.SubdivisionStrategies;
using DemoControls.Trees;
using System.Threading;
using System.Windows;

namespace DemoControls.TreeMaps
{
    public interface ITreeMapFactory<T> where T : ITreeItem<T>
    {
        ITreeMap<T> CreateTreeMap(Rect rectangle, ITree<T> tree, ISubdivisionStrategy subdivisionStrategy, CancellationToken cancellationToken = default);
    }
}
