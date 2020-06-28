using DemoControls.Trees;
using System.Collections.Generic;

namespace DemoControls.TreeMaps
{
    public interface ITreeMap<T> : IEnumerable<ITreeMapItem<T>> where T : ITreeItem<T>
    {
        ITreeMapItem<T>? HitTest(System.Windows.Point point, ITree<T> tree);
    }
}
