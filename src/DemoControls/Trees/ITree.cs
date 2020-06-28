namespace DemoControls.Trees
{
    public interface ITree<T> where T : class, ITreeItem<T>
    {
        public T? Root { get; }
    }
}
