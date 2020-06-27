using FileTreeMap.SubdivisionStrategies;
using FileTreeMap.SubdivisionStrategies.SquarifiedSubdivision;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace FileTreeMap.Tests.SubdivisionStrategies.SquarifiedSubdivision
{
    public class SquarifiedSubdivisionStrategyTests
    {
        private double[] rectangleAreas = { 2000, 3000, 1500, 3500 };
        private Rect parentRectangle = new Rect(50, 50, 100, 100);
        private SquarifiedSubdivisionStrategy? strategy;
        private Exception? exception;
        private IEnumerable<Rect>? result;

        [Fact]
        public void Subdivide_ValidInput_RectanglesCountEqualsAreasCount()
        {
            GivenSquarifiedSubdivisionStrategy();
            WhenSubdivideInvoked();
            ThenRectanglesCountEqualsAreasCount();
        }

        [Fact]
        public void Subdivide_ValidInput_RectanglesTotalAreaEqualsParentRectangleArea()
        {
            GivenSquarifiedSubdivisionStrategy();
            WhenSubdivideInvoked();
            ThenRectanglesTotalAreaEqualsParentRectangleArea();
        }

        [Fact]
        public void Subdivide_EmptyRectangle_Throws()
        {
            GivenSquarifiedSubdivisionStrategy();
            WhenSubdivideInvokedWithEmptyRectangle();
            ThenExceptionShouldBeThrown();
        }

        [Fact]
        public void Subdivide_ZeroRectangle_Throws()
        {
            GivenSquarifiedSubdivisionStrategy();
            WhenSubdivideInvokedWithZeroRectangle();
            ThenExceptionShouldBeThrown();
        }

        [Fact]
        public void Subdivide_TotalAreaUnequalToParentArea_Throws()
        {
            GivenSquarifiedSubdivisionStrategy();
            WhenSubdivideInvokedWithTotalAreaUnequalToParentArea();
            ThenExceptionShouldBeThrown();
        }

        [Fact]
        public void Subdivide_ValidInput_RectanglesAreWithinParentRectangle()
        {
            GivenSquarifiedSubdivisionStrategy();
            WhenSubdivideInvoked();
            ThenRectanglesAreWithinParentRectangle();
        }

        [Fact]
        public void Subdivide_ValidInput_RectanglesDoNotOverlap()
        {
            GivenSquarifiedSubdivisionStrategy();
            WhenSubdivideInvoked(); DrawDebugImage();
            ThenRectanglesDoNotOverlap();
        }

        #region States

        private void GivenSquarifiedSubdivisionStrategy()
        {
            strategy = new SquarifiedSubdivisionStrategy();
        }

        #endregion

        #region Behaviors

        private void WhenSubdivideInvoked()
        {
            exception = Record.Exception(() =>
            {
                result = strategy!.Subdivide(parentRectangle, rectangleAreas);
            });
        }

        private void WhenSubdivideInvokedWithEmptyRectangle()
        {
            exception = Record.Exception(() =>
            {
                result = strategy!.Subdivide(Rect.Empty, rectangleAreas);
            });
        }

        private void WhenSubdivideInvokedWithZeroRectangle()
        {
            exception = Record.Exception(() =>
            {
                result = strategy!.Subdivide(new Rect(0, 0, 0, 0), rectangleAreas);
            });
        }

        private void WhenSubdivideInvokedWithTotalAreaUnequalToParentArea()
        {
            exception = Record.Exception(() =>
            {
                result = strategy!.Subdivide(new Rect(0, 0, 100, 100), new double[] { 1 });
            });
        }

        #endregion

        #region Expectations

        private void ThenRectanglesDoNotOverlap()
        {
            Assert.NotNull(result);

            // Add a little bit of air around rectangles to avoid false positive overlaps.
            var scaledDownRectangles = result.Select(r =>
            {
                var scaleTransform = Matrix.Identity;
                scaleTransform.ScaleAt(0.99, 0.99, r.Left + r.Width * 0.5, r.Top + r.Height * 0.5);
                return Rect.Transform(r, scaleTransform);
            });

            var allCombinationsOfRectangles = scaledDownRectangles
                .SelectMany(x => scaledDownRectangles.Select(y => new { x, y }))
                .Where(pair => pair.x != pair.y);

            Assert.All(allCombinationsOfRectangles, r => Assert.False(r.x.IntersectsWith(r.y)));
        }

        private void ThenRectanglesAreWithinParentRectangle()
        {
            Assert.NotNull(result);
            Assert.All(result, r => Assert.True(parentRectangle.Contains(r)));
        }

        private void ThenRectanglesCountEqualsAreasCount()
        {
            Assert.NotNull(result);
            Assert.Equal(rectangleAreas.Count(), result.Count());
        }

        private void ThenRectanglesTotalAreaEqualsParentRectangleArea()
        {
            Assert.NotNull(result);
            var totalArea = result.Select(r => r.Width * r.Height).Sum();
            var parentArea = parentRectangle.Width * parentRectangle.Height;
            Assert.Equal(parentArea, totalArea);
        }

        private void ThenExceptionShouldBeThrown()
        {
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<SubdivisionStrategyException>(exception);
        }

        #endregion

        private void DrawDebugImage()
        {
            DrawDebugImage(parentRectangle, result!);
        }

        private void DrawDebugImage(Rect parentRectangle, IEnumerable<Rect> rectangles)
        {
            var imageWidth = (int)parentRectangle.Right + 1;
            var imageHeight = (int)parentRectangle.Bottom + 1;

            var image = new Bitmap(imageWidth, imageHeight);

            using (var graphics = Graphics.FromImage(image))
            {
                graphics.DrawRectangle(Pens.Red, ToDrawingRectangle(parentRectangle));

                foreach (var rectangle in rectangles)
                {
                    graphics.DrawRectangle(Pens.Red, ToDrawingRectangle(rectangle));
                }
            }

            // 1. Install Debugger Image Visualizer extension.
            // 2. Put breakpoint below.
            // 3. Enjoy debug images.
            var _ = image;

            static Rectangle ToDrawingRectangle(Rect rectangle)
            {
                return new Rectangle(
                    (int)rectangle.X,
                    (int)rectangle.Y,
                    (int)rectangle.Width,
                    (int)rectangle.Height);
            }
        }
    }
}