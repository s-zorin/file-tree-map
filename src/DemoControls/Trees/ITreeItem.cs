using System.Collections.Generic;

namespace DemoControls.Trees
{
    public interface ITreeItem<T> where T : ITreeItem<T>
    {
        string? Title { get; set; }

        double Size { get; set; }

        IList<T> Items { get; set; }
    }
}
