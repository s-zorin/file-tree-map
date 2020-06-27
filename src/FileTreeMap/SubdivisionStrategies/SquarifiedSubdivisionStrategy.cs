using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace FileTreeMap.SubdivisionStrategies
{
    public class SquarifiedSubdivisionStrategy : ISubdivisionStrategy
    {
        private const double EPSILON = 0.00001;

        /* TODO : RectangleLayoutStrategy */

        private Rect parentRectangle;

        private List<Rect> rectangles;

        private struct AbstractRect
        {
            public double SideAlongRow { get; set; }

            public double OtherSide { get; set; }

            public AbstractRect(double sideAlongRow, double otherSide) : this()
            {
                SideAlongRow = sideAlongRow;
                OtherSide = otherSide;
            }
        }

        private struct UnsolvedAbstractRect
        {
            public double SideAlongRow { get; set; }

            public double Area { get; set; }

            public UnsolvedAbstractRect(double sideAlongRow, double area) : this()
            {
                SideAlongRow = sideAlongRow;
                Area = area;
            }
        }

        private struct RectangleLayoutResult
        {
            public double MaxRectangleAspectRatio { get; set; }

            public RectangleLayoutResult(double maxRectangleAspectRatio)
            {
                MaxRectangleAspectRatio = maxRectangleAspectRatio;
            }
        }

        public SquarifiedSubdivisionStrategy()
        {
            
            rectangles = new List<Rect>();
        }

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

            var sortedRectangleAreas = rectangleAreas.OrderBy(a => a);

            parentRectangle = rectangle;

            var rectangleRowLayouts = parentRectangle.Width > parentRectangle.Height
                ? WideParentRectangleRowLayoutIterator()
                : TallParentRectangleRowLayoutIterator();

            var skipAmount = 0;
            var takeAmount = 1;
            var previousMaxRectangleAspectRatio = double.MaxValue;
            var previousRow = Enumerable.Empty<Rect>();

            var rects = new List<Rect>();

            foreach (var rectangleRowLayout in rectangleRowLayouts)
            {
                if (skipAmount + takeAmount > rectangleAreas.Count())
                {
                    break;
                }

                while (true)
                {
                    if (skipAmount + takeAmount > rectangleAreas.Count())
                    {
                        rects.AddRange(previousRow);
                        break;
                    }

                    var row = rectangleRowLayout(rectangleAreas.Skip(skipAmount).Take(takeAmount)).ToList();
                    //for (var i = 0; i < row.Count; i++)
                    //{
                    //    var r = row[i];
                    //    r.Offset(parentRectangle.Left, parentRectangle.Top);
                    //    row[i] = r;
                    //}

                    /**/
                    Debug.WriteLine(string.Concat(row.Select(r => r.ToString() + " | ")));

                    var currentMaxRectangleAspectRatio = MaxRectangleAspectRatioInRow(row);
                    var maxRectangleAspectRatioIncreased = currentMaxRectangleAspectRatio > previousMaxRectangleAspectRatio;
                    previousMaxRectangleAspectRatio = currentMaxRectangleAspectRatio;

                    if (maxRectangleAspectRatioIncreased)
                    {
                        var rowAggregate = previousRow.Aggregate((a, b) => Rect.Union(a, b));

                        if (Math.Abs(rowAggregate.Width - parentRectangle.Width) < 0.01)
                        {
                            parentRectangle = new Rect(parentRectangle.X, parentRectangle.Y + rowAggregate.Height, parentRectangle.Width, parentRectangle.Height - rowAggregate.Height);
                        }
                        else
                        {
                            parentRectangle = new Rect(parentRectangle.X + rowAggregate.Width, parentRectangle.Y, parentRectangle.Width - rowAggregate.Width, parentRectangle.Height);
                        }


                        rects.AddRange(previousRow);
                        skipAmount += previousRow.Count();
                        takeAmount = 1;
                        previousMaxRectangleAspectRatio = double.MaxValue;
                        break;
                    }
                    else
                    {
                        takeAmount++;
                    }

                    previousRow = row;
                }
            }

            return rects;
        }

        private void BuildRow(RectangleRowLayout rectangleRowLayout, IEnumerable<double> rectangleAreas)
        {

        }

        private double MaxRectangleAspectRatioInRow(IEnumerable<Rect> row)
        {
            return row.Select(rectangle => Math.Max(rectangle.Height, rectangle.Width) / Math.Min(rectangle.Height, rectangle.Width)).Max();
        }

        private IEnumerable<RectangleRowLayout> WideParentRectangleRowLayoutIterator()
        {
            while (true)
            {
                yield return LayoutRectanglesInVerticalRow;
                yield return LayoutRectanglesInHorizontalRow;
            }
        }

        private IEnumerable<RectangleRowLayout> TallParentRectangleRowLayoutIterator()
        {
            while (true)
            {
                yield return LayoutRectanglesInHorizontalRow;
                yield return LayoutRectanglesInVerticalRow;
            }
        }

        private IEnumerable<Rect> LayoutRectanglesInHorizontalRow(IEnumerable<double> rectangleAreas)
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

        private IEnumerable<Rect> LayoutRectanglesInVerticalRow(IEnumerable<double> rectangleAreas)
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

        private IEnumerable<AbstractRect> LayoutRectanglesInRow(double rowLength, IEnumerable<double> rectangleAreas)
        {
            var totalArea = rectangleAreas.Sum();
            return rectangleAreas
                .Select(rectangleArea => CalculateRectangleSideAlongRow(rectangleArea))
                .Zip(rectangleAreas, (rectangleSideAlongRow, rectangleArea) => new UnsolvedAbstractRect(rectangleSideAlongRow, rectangleArea))
                .Select(unsolvedRectangle => SolveRectangle(unsolvedRectangle))
                .ToList();

            double CalculateRectangleSideAlongRow(double rectangleArea)
            {
                return rectangleArea / totalArea * rowLength;
            }

            AbstractRect SolveRectangle(UnsolvedAbstractRect unsolvedRectangle)
            {
                var otherSide = unsolvedRectangle.Area / unsolvedRectangle.SideAlongRow;
                return new AbstractRect(unsolvedRectangle.SideAlongRow, otherSide);
            }
        }

        //private void AddArea(double area)
        //{
        //    var rectangleA = GenerateHorizontallyAlignedRectangle(area);
        //    var rectangleB = GenerateVerticallyAlignedRectangle(area);
        //    var generatedRectangle = ChooseRectangleWithSmallestAspectRatio(rectangleA, rectangleB);
        //}

        //private Rect ChooseRectangleWithSmallestAspectRatio(Rect rectangleA, Rect rectangleB)
        //{
        //    var aspectRatioA = CalculateRectangleAspectRatio(rectangleA);
        //    var aspectRatioB = CalculateRectangleAspectRatio(rectangleB);
        //    return aspectRatioA < aspectRatioB ? rectangleA : rectangleB;
        //}

        //private double CalculateRectangleAspectRatio(Rect rectangle)
        //{
        //    return rectangle.Height / rectangle.Width;
        //}

        //private Rect GenerateHorizontallyAlignedRectangle(double area)
        //{
        //    var height = rectangle.Height;
        //    var width = SolveRectangle(area, height);
        //    return new Rect(0, 0, width, height);
        //}

        //private Rect GenerateVerticallyAlignedRectangle(double area)
        //{
        //    var width = rectangle.Width;
        //    var height = SolveRectangle(area, width);
        //    return new Rect(0, 0, width, height);
        //}

        //private double SolveRectangle(double area, double side)
        //{
        //    return area / side;
        //}
    }
}
