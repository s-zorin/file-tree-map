using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace FileTreeMap.SubdivisionStrategies.SquarifiedSubdivision
{
    public class SquarifiedSubdivisionStrategy : ISubdivisionStrategy
    {
        private const double EPSILON = 0.00001;

        public IEnumerable<Rect> Subdivide(Rect rectangle, IEnumerable<double> rectangleAreas)
        {
            if (rectangle.IsEmpty)
            {
                throw new SubdivisionStrategyException("Rectangle could not be empty.");
            }

            if (rectangle.Width < EPSILON || rectangle.Height < EPSILON)
            {
                throw new SubdivisionStrategyException("Rectangle could not have zero area.");
            }

            if (Math.Abs(rectangle.Width * rectangle.Height - rectangleAreas.Sum()) > EPSILON)
            {
                throw new SubdivisionStrategyException("Total subrectangle area is unequal to parent rectangle area.");
            }

            var rectangleRowLayouts = rectangle.Width > rectangle.Height
                ? WideParentRectangleRowLayoutIterator()
                : TallParentRectangleRowLayoutIterator();

            var skipAmount = 0;
            var rectangles = new List<Rect>();

            foreach (var rowLayoutStrategy in rectangleRowLayouts)
            {
                var result = SubdivideInternal(rectangle, rectangleAreas.Skip(skipAmount), rowLayoutStrategy);

                skipAmount += result.Rectangles.Count();
                rectangles.AddRange(result.Rectangles);
                rectangle = result.RemainingRectangle;

                if (result.IsSubdivisionCompleted)
                {
                    break;
                }
            }

            return rectangles;
        }

        private SubdivideResult SubdivideInternal(Rect rectangle, IEnumerable<double> rectangleAreas, RectangleRowLayoutStrategy rowLayoutStrategy)
        {
            var takeAmount = 1;
            var maxAspectRatio = double.MaxValue;
            var row = Enumerable.Empty<Rect>();

            while (true)
            {
                var isProcessedAllRectangles = takeAmount > rectangleAreas.Count();
                if (isProcessedAllRectangles)
                {
                    //var remainingRectangle = CutRowFromRectangle(rectangle, row);
                    return new SubdivideResult(Rect.Empty, row, true);
                }

                var candidateRow = rowLayoutStrategy(rectangle, rectangleAreas.Take(takeAmount));
                var candidateRowMaxAspectRatio = MaxRectangleAspectRatioInRow(candidateRow);
                var isMaxAspectRatioIncreased = candidateRowMaxAspectRatio > maxAspectRatio;
                
                if (isMaxAspectRatioIncreased)
                {
                    var remainingRectangle = CutRowFromRectangle(rectangle, row);
                    return new SubdivideResult(remainingRectangle, row, false);
                }
                else
                {
                    takeAmount++;
                }

                maxAspectRatio = candidateRowMaxAspectRatio;
                row = candidateRow;
            }
        }

        private Rect CutRowFromRectangle(Rect rectangle, IEnumerable<Rect> row)
        {
            var rowRectangle = RowToRectangle(row);
            return CutRowRectangleFromRectangle(rectangle, rowRectangle);
        }

        private Rect RowToRectangle(IEnumerable<Rect> row)
        {
            return row.Aggregate((a, b) => Rect.Union(a, b));
        }

        private Rect CutRowRectangleFromRectangle(Rect rectangle, Rect rowRectangle)
        {
            var isHorizontalRow = Math.Abs(rectangle.Width - rowRectangle.Width) < EPSILON;

            if (isHorizontalRow)
            {
                rectangle = CutBottomRowFromRectangle(rectangle, rowRectangle);
            }
            else
            {
                rectangle = CutLeftRowFromRectangle(rectangle, rowRectangle);
            }

            return rectangle;
        }

        private Rect CutBottomRowFromRectangle(Rect rectangle, Rect rowRectangle)
        {
            var x = (rectangle.X);
            var y = (rectangle.Y + rowRectangle.Height);
            var width = (rectangle.Width);
            var height = (rectangle.Height - rowRectangle.Height);

            return new Rect(x, y, width, height);
        }

        private Rect CutLeftRowFromRectangle(Rect rectangle, Rect rowRectangle)
        {
            var x = (rectangle.X + rowRectangle.Width);
            var y = (rectangle.Y);
            var width = (rectangle.Width - rowRectangle.Width);
            var height = (rectangle.Height);

            return new Rect(x, y, width, height);
        }

        private double MaxRectangleAspectRatioInRow(IEnumerable<Rect> row)
        {
            return row.Select(rectangle => Math.Max(rectangle.Height, rectangle.Width) / Math.Min(rectangle.Height, rectangle.Width)).Max();
        }

        private IEnumerable<RectangleRowLayoutStrategy> WideParentRectangleRowLayoutIterator()
        {
            while (true)
            {
                yield return LayoutRectanglesInVerticalRow;
                yield return LayoutRectanglesInHorizontalRow;
            }
        }

        private IEnumerable<RectangleRowLayoutStrategy> TallParentRectangleRowLayoutIterator()
        {
            while (true)
            {
                yield return LayoutRectanglesInHorizontalRow;
                yield return LayoutRectanglesInVerticalRow;
            }
        }

        private static IEnumerable<Rect> LayoutRectanglesInHorizontalRow(Rect parentRectangle, IEnumerable<double> rectangleAreas)
        {
            var row = LayoutRectanglesInRow(parentRectangle.Width, rectangleAreas);
            var x = parentRectangle.Left;
            var y = parentRectangle.Top;

            foreach (var abstractRectangle in row)
            {
                var width = abstractRectangle.SideAlongRow;
                var height = abstractRectangle.OtherSide;

                yield return new Rect(x, y, width, height);

                x += abstractRectangle.SideAlongRow;
            }
        }

        private static IEnumerable<Rect> LayoutRectanglesInVerticalRow(Rect parentRectangle, IEnumerable<double> rectangleAreas)
        {
            var row = LayoutRectanglesInRow(parentRectangle.Height, rectangleAreas);
            var x = parentRectangle.Left;
            var y = parentRectangle.Top;

            foreach (var abstractRectangle in row)
            {
                var width = abstractRectangle.OtherSide;
                var height = abstractRectangle.SideAlongRow;

                yield return new Rect(x, y, width, height);

                y += abstractRectangle.SideAlongRow;
            }
        }

        private static IEnumerable<AbstractRect> LayoutRectanglesInRow(double rowLength, IEnumerable<double> rectangleAreas)
        {
            var totalArea = rectangleAreas.Sum();
            return rectangleAreas
                .Select(rectangleArea => CalculateRectangleSideAlongRow(rectangleArea))
                .Zip(rectangleAreas, (rectangleSideAlongRow, rectangleArea) => new UnsolvedAbstractRect(rectangleSideAlongRow, rectangleArea))
                .Select(unsolvedRectangle => SolveRectangle(unsolvedRectangle))
                .ToList();

            double CalculateRectangleSideAlongRow(double rectangleArea)
            {
                return (rectangleArea / totalArea * rowLength);
            }

            static AbstractRect SolveRectangle(UnsolvedAbstractRect unsolvedRectangle)
            {
                var otherSide = (unsolvedRectangle.Area / unsolvedRectangle.SideAlongRow);
                return new AbstractRect(unsolvedRectangle.SideAlongRow, otherSide);
            }
        }
    }
}
