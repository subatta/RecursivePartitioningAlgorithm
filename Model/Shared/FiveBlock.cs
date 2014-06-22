namespace AutoNav.Library.RecursivePartitioning.Model.Shared
{
    public class FiveBlockShared
    {
        // -------------- Core Structures Five Block ----------------------------------//

        /* Arrays of indices for indexing the matrices that store information
         * about rectangular sub problems (L,W), where (L,W) belongs to X' x Y'
         * and X' and Y' are the raster points sets associated to (L,l,w) and
         * (W,l,w), respectively. */

        public int[] IndexX { get; set; }
        public int[] IndexY { get; set; }


        /* Set of integer conic combinations of l and w:
        * X = {x | x = rl + sw, with r,w in Z and r,w >= 0} */

        public Set NormalSetX { get; set; }

        /* Array that stores the normalized values of each integer between 0 and
         * L (dimension of the problem):
         * normalize[x] = max {r in X' | r <= x} */

        public int[] Normalized { get; set; }

        /* Store the points that determine the divisions of the rectangles. */
        public CutPoint[][] CutPoints { get; set; }

        /* Lower and upper bounds of each sub problem. */
        public int[][] LowerBound { get; set; }
        public int[][] UpperBound { get; set; }

        /// <summary>
        /// Input data:
        /// L - Length of the pallet.
        /// W - Width of the pallet.
        /// l - Length of the boxes.
        /// w - Width of the boxes.
        /// </summary>
        public Parameters Parameters { get; set; }

        // --------------- Convenience methods ----------------------------//

        public int NormalizedL
        {
            get { return Normalized[Parameters.L]; }
        }

        public int NormalizedW
        {
            get { return Normalized[Parameters.W]; }
        }
    }
}