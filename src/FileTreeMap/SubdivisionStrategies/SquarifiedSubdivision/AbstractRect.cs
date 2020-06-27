namespace FileTreeMap.SubdivisionStrategies.SquarifiedSubdivision
{
    internal struct AbstractRect
    {
        public double SideAlongRow { get; set; }

        public double OtherSide { get; set; }

        public AbstractRect(double sideAlongRow, double otherSide) : this()
        {
            SideAlongRow = sideAlongRow;
            OtherSide = otherSide;
        }
    }
}
