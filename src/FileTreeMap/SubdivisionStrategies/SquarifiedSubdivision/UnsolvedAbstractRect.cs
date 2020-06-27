namespace FileTreeMap.SubdivisionStrategies.SquarifiedSubdivision
{
    internal struct UnsolvedAbstractRect
    {
        public double SideAlongRow { get; set; }

        public double Area { get; set; }

        public UnsolvedAbstractRect(double sideAlongRow, double area) : this()
        {
            SideAlongRow = sideAlongRow;
            Area = area;
        }
    }
}
