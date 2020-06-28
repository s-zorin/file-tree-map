using System.Collections.Generic;

namespace FileTreeMap
{
    public interface ITreeMap<T> : IEnumerable<ITreeMapItem<T>> where T : ITreeItem<T>
    {
        ITreeMapItem<T>? HitTest(System.Windows.Point point, ITree<T> tree);
    }
}
