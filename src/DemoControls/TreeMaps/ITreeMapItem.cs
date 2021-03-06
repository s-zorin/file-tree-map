﻿namespace DemoControls.TreeMaps
{
    public interface ITreeMapItem<T>
    {
        T TreeItem { get; }

        TreeMapTitleDescription TitleDescription { get; }

        TreeMapRectangleDescription RectangleDescription { get; }
    }
}
