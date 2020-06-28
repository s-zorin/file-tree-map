using DemoControls.Trees;

namespace DemoControls.TreeMaps
{
    public class FileTreeMapItem : ITreeMapItem<FileTreeItem>
    {
        public FileTreeItem TreeItem { get; private set; }

        public TreeMapTitleDescription TitleDescription { get; private set; }

        public TreeMapRectangleDescription RectangleDescription { get; private set; }

        public FileTreeMapItem(FileTreeItem treeItem, TreeMapTitleDescription titleDescription, TreeMapRectangleDescription rectangleDescription)
        {
            TreeItem = treeItem;
            TitleDescription = titleDescription;
            RectangleDescription = rectangleDescription;
        }
    }
}
