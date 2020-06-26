using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Xunit;

namespace FileTreeMap.Tests
{
    public class SquarifiedSubdivisionStrategyTests
    {
        [Fact]
        public void Test1()
        {
            var s = new SquarifiedSubdivisionStrategy();
            var rects = s.Subdivide(new Rect(0, 0, 100, 100), new double[] { 5500, 1300, 1200, 1000, 733, 267 });
            DrawDebugImage(new Rect(0, 0, 100, 100), rects);
        }

        private void DrawDebugImage(Rect parentRectangle, IEnumerable<Rect> rectangles)
        {
            var imageWidth = (int)parentRectangle.Right + 1;
            var imageHeight = (int)parentRectangle.Bottom + 1;

            var image = new Bitmap(imageWidth, imageHeight);

            using (var graphics = Graphics.FromImage(image))
            {
                graphics.DrawRectangle(Pens.Red, parentRectangle.ToDrawingRectangle());

                foreach (var rectangle in rectangles)
                {
                    graphics.DrawRectangle(Pens.Red, rectangle.ToDrawingRectangle());
                }
            }

            // 1. Install Debugger Image Visualizer extension.
            // 2. Put breakpoint below.
            // 3. Enjoy debug images.
            var output = image;
        }
    }

    public static class RectExtensions
    {
        public static Rectangle ToDrawingRectangle(this Rect rectangle)
        {
            return new Rectangle(
                (int)rectangle.X,
                (int)rectangle.Y,
                (int)rectangle.Width,
                (int)rectangle.Height);
        }
    }
}
