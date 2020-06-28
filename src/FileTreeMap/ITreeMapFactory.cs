using FileTreeMap.SubdivisionStrategies;
using System.Threading;
using System.Windows;

namespace FileTreeMap
{
    public interface ITreeMapFactory<T> where T : ITreeItem<T>
    {
        ITreeMap<T> CreateTreeMap(Rect rectangle, ITree<T> tree, ISubdivisionStrategy subdivisionStrategy, CancellationToken cancellationToken = default);
    }
}
