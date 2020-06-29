using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace DemoControls.TreeMaps
{
    public class FileTreeMapPalette
    {
        private readonly Brush[] brushes;

        public FileTreeMapPalette(int length)
        {
            if (length < 1)
            {
                throw new ArgumentException(nameof(length));
            }

            Length = length;
            brushes = GenerateBrushes(length).ToArray();
        }

        public int Length { get; }

        public Brush this[int index]
        {
            get => brushes[index];
        }

        private IEnumerable<Brush> GenerateBrushes(int length)
        {
            for (int i = 0; i < length; i++)
            {
                var g = (byte)(255 - 180d * i / length);
                var brush = new SolidColorBrush(Color.FromRgb(0, g, 0));
                brush.Freeze();
                yield return brush;
            }
        }
    }
}
