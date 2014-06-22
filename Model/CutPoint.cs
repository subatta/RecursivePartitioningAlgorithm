namespace AutoNav.Library.RecursivePartitioning.Model
{
    public class CutPoint
    {
        public int X1;
        public int X2;
        public int Y1;
        public int Y2;
        public int Homogeneous;

        public void Extend(CutPoint source)
        {
            X2 = source.X2;
            Y2 = source.Y2;
        }

        public CutPoint Clone()
        {
            return new CutPoint()
            {
                X1 = this.X1,
                X2 = this.X2,
                Y1 = this.Y1,
                Y2 = this.Y2
            };
        }
    }
}