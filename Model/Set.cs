namespace AutoNav.Library.RecursivePartitioning.Model
{
    public class Set
    {
        public int Size;
        public int[] Points;

        public Set()
        {
        }

        public int MaxSize;

        public Set(int maxSize)
        {
            Points = new int[maxSize];
            MaxSize = maxSize;
        }

        public void ClearPoints()
        {
            Size = 0;
            Points = new int[MaxSize];
        }
    }
}