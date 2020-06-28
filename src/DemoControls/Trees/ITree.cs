namespace DemoControls.Trees
{
    public interface ITree<T> where T : ITreeItem<T>
    {
        public T Root { get; set; }
    }
}
