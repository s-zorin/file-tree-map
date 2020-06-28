using System.Collections.Generic;

namespace DemoControls.Trees
{
    public interface ITreeItem<T> where T : ITreeItem<T>
    {
        string Title { get; }

        double Size { get; }

        IList<T> Items { get; }
    }
}
