using System.Collections.Generic;

namespace AutoNav.Library.RecursivePartitioning.Model.Shared
{
    public class LBlockShared
    {
        public FiveBlockShared FiveBlockData { get; set; }

        // --------------- Core Structures LBlock -------------------------//

        public int MemoryType;

        public int[] IndexRasterX;
        public int[] IndexRasterY;
        public int NumRasterX;
        public int NumRasterY;


        /* Store the solutions of the sub problems. */
        public int[] Solution;
        public SortedDictionary<int, int>[] SolutionMap;

        /* Store the division points in the rectangular and in the L-shaped
         * pieces associated to the solutions found. */
        public int[] DivisionPoint;
        public SortedDictionary<int, int>[] DivisionPointMap;

        /*
         * Normalize the L-piece (q0,q1,q2,q3) considering the symmetries
         * below, defined in [3].
         *
         * [3] R. Morabito and S. Morales. A simple and effective recursive
         *     procedure for the manufacturer's pallet loading
         *     problem. Journal of the Operational Research Society, volume
         *     49, number 8, pages 819-828, 1998.
         *
         * Symmetry considerations for (i,j,i',j'):
         *
         * (1) i >= i' and j >= j', from the definition of standard positioned
         * L's;
         *
         * (2) i >= j, otherwise we could use (j, i, j', i');
         *
         * (3) i = j implies i' >= j', otherwise we again could use (j,i,j',i');
         *
         * (4) i = i' if and only if j = j'. This equivalence follows to avoid
         * degenerated L's which are not explicit rectangles: in terms of
         * occupancy, (i,j,i,j') with j' < j can be replaced by (i,j,i,j) and
         * (i,j,i',j) with i' < i can also be replaced by (i,j,i,j).
         *
         * The normalization (i,j,i',j')^N of a quadruple (i,j,i',j') is defined
         * as:
         *
         * - if (i,j,i',j') satisfies (1)-(4), then (i,j,i',j')^N = (i,j,i',j');
         *
         * - if 0 = i' < i and 0 < j' < j, then (i,j,i',j')^N = (i,j',i,j');
         *
         * - if 0 < i' < i and 0 = j' < j, then (i,j,i',j')^N = (i',j,i',j);
         *
         * - if 0 < i' = i and 0 < j' < j, then (i,j,i',j')^N = (i,j,i,j);
         *
         * - if 0 < i' < i and 0 < j' = j, then (i,j,i',j')^N = (i,j,i,j);
         *
         * - if 0 < i' < i, 0 < j' < j and i < j, then (i,j,i',j')^N = (j,i,j',i');
         *
         * - if 0 < i' < i, 0 < j' < j, i = j and i' < j', then (i,j,i',j')^N =
         *   (j,i,j',i');
         *
         *
         * Parameters:
         * q - The L-piece to be normalized.
         */

        public void NormalizePiece(int[] q)
        {
            int i;
            int j;
            int i1;
            int j1;

            i = q[0];
            j = q[1];
            i1 = q[2];
            j1 = q[3];

            /* Rule (4) for degenerated L's. */
            if (i1 == 0)
            {
                i1 = i;
                j = j1;
            }
            else if (j1 == 0)
            {
                j1 = j;
                i = i1;
            }
            else if (i1 == i || j1 == j)
            {
                i1 = i;
                j1 = j;
            }

            /* If the area of this L-piece is less than the area of the box,
             * this L-piece is discarded. */
            if (i*j - (i - i1)*(j - j1) < FiveBlockData.Parameters.l*FiveBlockData.Parameters.w)
            {
                q[0] = -1;
                return;
            }

            if (i == i1 && j == j1 && i < j)
            {
                Util.Swap(ref i, ref j);
                Util.Swap(ref i1, ref j1);
            }

            if (0 < i1 && i1 < i && 0 < j1 && j1 < j && i < j)
            {
                Util.Swap(ref i, ref j);
                Util.Swap(ref i1, ref j1);
            }
            else if (0 < i1 && i1 < i && 0 < j1 && j1 < j && i == j && i1 < j1)
            {
                Util.Swap(ref i1, ref j1);
            }

            q[0] = i;
            q[1] = j;
            q[2] = i1;
            q[3] = j1;
        }

        /**
         * Return the index associated to the L-shaped piece (q0, q1, q2, q3).
         */

        public int GetKey(int[] q)
        {
            switch (MemoryType)
            {
                case Constants.MEM_TYPE_4:
                    return 0;

                case Constants.MEM_TYPE_3:
                    return q[3];

                case Constants.MEM_TYPE_2:
                    return ((IndexRasterX[q[2]]*NumRasterY) + IndexRasterY[q[3]]);

                case Constants.MEM_TYPE_1:
                    return ((IndexRasterY[q[1]])*NumRasterX + IndexRasterX[q[2]])*NumRasterY + IndexRasterY[q[3]];

                default:
                    return 0;
            }
        }

        /**
         * Determine how to cut the L-piece.
         *
         * Parameter:
         * q - The L-piece.
         */

        public int LCut(int[] q)
        {
            /* Divide the L-piece in two rectangles and calculate their lower
             * bounds to compose the lower bound of the L-piece. */
            int a = R_LowerBound(q[2], q[1]) + R_LowerBound(q[0] - q[2], q[3]);
            int b = R_LowerBound(q[2], q[1] - q[3]) + R_LowerBound(q[0], q[3]);

            return (a > b) ? Constants.VERTICAL_CUT : Constants.HORIZONTAL_CUT;
        }

        /**
         * Calculate the lower bound of a given rectangle R(x,y) (degenerated L).
         *
         * Parameters:
         * x - Length of the rectangle.
         *
         * y - Width of the rectangle.
         *
         * Return:
         * The computed lower bound.
         */

        public int R_LowerBound(int x, int y)
        {
            x = FiveBlockData.Normalized[x];
            y = FiveBlockData.Normalized[y];
            return FiveBlockData.LowerBound[FiveBlockData.IndexX[x]][FiveBlockData.IndexY[y]];
        }

        /**
         * Return the index associated to the L-shaped piece (q0, q1, q2, q3).
         */

        public int LIndex(int[] q)
        {
            switch (MemoryType)
            {
                case Constants.MEM_TYPE_4:
                    return (((IndexRasterX[q[0]]*NumRasterY) +
                             IndexRasterY[q[1]])*NumRasterX +
                            IndexRasterX[q[2]])*NumRasterY
                           + IndexRasterY[q[3]];

                case Constants.MEM_TYPE_3:
                    return (((IndexRasterX[q[0]]*NumRasterY) +
                             IndexRasterY[q[1]])*NumRasterX +
                            IndexRasterX[q[2]]);

                case Constants.MEM_TYPE_2:
                    return ((IndexRasterX[q[0]]*NumRasterY) +
                            IndexRasterY[q[1]]);

                case Constants.MEM_TYPE_1:
                    return IndexRasterX[q[0]];

                default:
                    return 0;
            }
        }

        /**
         * Divide the L-shaped piece in two new L-shaped pieces, according to
         * the subdivision B1, and put them in the standard position.
         *
         *                  (X,Y)
         * +------------+     o
         * |            |
         * |            |(x,y)                     (x,Y-y')                     (X,y)
         * |      +-----o-----+         +------+     o         +-----------+      o
         * |  L1  |           |         |      |               |           |
         * |      |     L2    |   -->   |      |(x',Y-y)       |           |(X-x',y')
         * +------o           |         |  L1  o-----+         |     L2    o------+
         * |   (x',y')        |         |            |         |                  |
         * |                  |         |            |         |                  |
         * +------------------+         +------------+         +------------------+
         *
         * Parameters:
         * i  - Array of two elements such that i[0] = x' and i[1] = y'.
         *
         * q  - The L-shaped piece to be divided. q = {X, Y, x, y}.
         *
         * q1 - Array to store L1.
         *
         * q2 - Array to store L2.
         */

        public void StandardPositionB1(ref int[] i, ref int[] q, ref int[] q1, ref int[] q2)
        {
            /* L1 */
            q1[0] = q[2];
            q1[1] = FiveBlockData.Normalized[q[1] - i[1]];
            q1[2] = i[0];
            q1[3] = FiveBlockData.Normalized[q[1] - q[3]];

            /* L2 */
            q2[0] = q[0];
            q2[1] = q[3];
            q2[2] = FiveBlockData.Normalized[q[0] - i[0]];
            q2[3] = i[1];
        }

        /**
         * Divide the L-shaped piece in two new L-shaped pieces, according to
         * the subdivision B2, and put them in the standard position.
         *
         *                  (X,Y)
         * +------------+     o
         * |            |
         * |   (x',y')  |                          (x,Y-y)                     (X,y')
         * +------o     |               +-----+      o        +------+           o
         * |      | L1  |               |     |               |      |
         * |      |     |(x,y)    -->   |     |(x-x',Y-y')    |      |(x',y)
         * |      +-----o-----+         |     o------+        |      o-----------+
         * |  L2              |         |  L1        |        |  L2              |
         * |                  |         |            |        |                  |
         * +------------------+         +------------+        +------------------+
         *
         * Parameters:
         * i  - Array of two elements such that i[0] = x' and i[1] = y'.
         *
         * q  - The L-shaped piece to be divided. q = {X, Y, x, y}.
         *
         * q1 - Array to store L1.
         *
         * q2 - Array to store L2.
         */

        public void StandardPositionB2(ref int[] i, ref int[] q, ref int[] q1, ref int[] q2)
        {
            /* L1 */
            q1[0] = q[2];
            q1[1] = FiveBlockData.Normalized[q[1] - q[3]];
            q1[2] = FiveBlockData.Normalized[q[2] - i[0]];
            q1[3] = FiveBlockData.Normalized[q[1] - i[1]];

            /* L2 */
            q2[0] = q[0];
            q2[1] = i[1];
            q2[2] = i[0];
            q2[3] = q[3];
        }

        /**
         * Divide the L-shaped piece in two new L-shaped pieces, according to
         * the subdivision B3, and put them in the standard position.
         *
         *                  (X,Y)                         (X,Y)
         * +------+-----+     o         +------+           o
         * |      |     |               |      |
         * |      |     |(x,y)          |      |                         (X-x',Y-y')
         * |      | L2  o-----+         |      |                  +-----+     o
         * |      |           |         |      |                  |     |
         * |  L1  |           |   -->   |  L1  |(x',y')           |     |(x-x',y-y')
         * |      o-----------+         |      o-----------+      | L2  o-----+ 
         * |   (x',y')        |         |                  |      |           |
         * |                  |         |                  |      |           |
         * +------------------+         +------------------+      +-----------+
         *
         * Parameters:
         * i  - Array of two elements such that i[0] = x' and i[1] = y'.
         *
         * q  - The L-shaped piece to be divided. q = {X, Y, x, y}.
         *
         * q1 - Array to store L1.
         *
         * q2 - Array to store L2.
         */

        public void StandardPositionB3(ref int[] i, ref int[] q, ref int[] q1, ref int[] q2)
        {
            /* L1 */
            q1[0] = q[0];
            q1[1] = q[1];
            q1[2] = i[0];
            q1[3] = i[1];

            /* L2 */
            q2[0] = FiveBlockData.Normalized[q[0] - i[0]];
            q2[1] = FiveBlockData.Normalized[q[1] - i[1]];
            q2[2] = FiveBlockData.Normalized[q[2] - i[0]];
            q2[3] = FiveBlockData.Normalized[q[3] - i[1]];
        }

        /**
         * Divide the L-shaped piece in two new L-shaped pieces, according to
         * the subdivision B4, and put them in the standard position.
         *
         *                  (X,Y)                 (x',Y)
         * +------+           o         +------+     o
         * |      |                     |      |
         * |      |(x,y)                |      |                     (X-x,y)
         * |      o-----------+         |      |             +-----+     o
         * |  L1  |           |         |  L1  |             |     |
         * |      |  (x',y')  |   -->   |      |(x,y')       |     |(X-x',y-y')
         * |      +-----o     |         |      o-----+       |     o-----+
         * |            | L2  |         |            |       |  L2       |
         * |            |     |         |            |       |           |
         * +------------+-----+         +------------+       +-----------+
         *
         * Parameters:
         * i  - Array of two elements such that i[0] = x' and i[1] = y'.
         *
         * q  - The L-shaped piece to be divided. q = {X, Y, x, y}.
         *
         * q1 - Array to store L1.
         *
         * q2 - Array to store L2.
         */

        public void StandardPositionB4(ref int[] i, ref int[] q, ref int[] q1, ref int[] q2)
        {
            /* L1 */
            q1[0] = i[0];
            q1[1] = q[1];
            q1[2] = q[2];
            q1[3] = i[1];

            /* L2 */
            q2[0] = FiveBlockData.Normalized[q[0] - q[2]];
            q2[1] = q[3];
            q2[2] = FiveBlockData.Normalized[q[0] - i[0]];
            q2[3] = FiveBlockData.Normalized[q[3] - i[1]];
        }

        /**
         * Divide the L-shaped piece in two new L-shaped pieces, according to
         * the subdivision B5, and put them in the standard position.
         *
         *                  (X,Y)                  (x,Y)
         * +------------+     o         +------+     o
         * |            |               |      |
         * |     L1     |(x,y)          |      |                    (X-x',y)
         * |            o-----+         |      |(x',Y-y')    +-----+     o
         * |   (x',y')  |     |         |      o-----+       |     |
         * |      o-----+     |   -->   |            |       |     o-----+
         * |      |           |         |     L1     |       | (X-x,y')  |
         * |      |     L2    |         |            |       |           |
         * |      |           |         |            |       |    L2     |
         * +------+-----------+         +------------+       +-----------+
         *
         * Parameters:
         * i  - Array of two elements such that i[0] = x' and i[1] = y'.
         *
         * q  - The L-shaped piece to be divided. q = {X, Y, x, y}.
         *
         * q1 - Array to store L1.
         *
         * q2 - Array to store L2.
         */

        public void StandardPositionB5(ref int[] i, ref int[] q, ref int[] q1, ref int[] q2)
        {
            /* L1 */
            q1[0] = q[2];
            q1[1] = q[1];
            q1[2] = i[0];
            q1[3] = FiveBlockData.Normalized[q[1] - i[1]];

            /* L2 */
            q2[0] = FiveBlockData.Normalized[q[0] - i[0]];
            q2[1] = q[3];
            q2[2] = FiveBlockData.Normalized[q[0] - q[2]];
            q2[3] = i[1];
        }

        /**
         * Divide the rectangle in two L-shaped pieces, according to the
         * subdivision B6, and put them in the standard position.
         *
         *                      (X,Y)                 (x'',Y)              (X-x',Y)
         * +-------------+--------o         +------+      o     +--------+      o
         * |             |        |         |      |            |        |
         * |   (x',y')   |   L2   |         |      |            |        |(X-x'',y')
         * |      o------o        |   -->   |      |(x',Y-y')   |        o------+
         * |      |  (x'',y')     |         |  L1  o------+     |   L2          |
         * |  L1  |               |         |             |     |               |
         * |      |               |         |             |     |               |
         * +------+---------------+         +-------------+     +---------------+
         *
         * Parameters:
         * i  - Array of three elements such that i[0] = x', i[1] = y' and i[2] = x''.
         *
         * q  - The rectangle to be divided. q = {X, Y, X, Y}.
         *
         * q1 - Array to store L1.
         *
         * q2 - Array to store L2.
         */

        public void StandardPositionB6(ref int[] i, ref int[] q, ref int[] q1, ref int[] q2)
        {
            /* L1 */
            q1[0] = i[2];
            q1[1] = q[1];
            q1[2] = i[0];
            q1[3] = FiveBlockData.Normalized[q[1] - i[1]];

            /* L2 */
            q2[0] = FiveBlockData.Normalized[q[0] - i[0]];
            q2[1] = q[1];
            q2[2] = FiveBlockData.Normalized[q[0] - i[2]];
            q2[3] = i[1];
        }

        /**
         * Divide the rectangle in two L-shaped pieces, according to the
         * subdivision B7, and put them in the standard position.
         *
         *             (X,Y)
         * +-------------o
         * |             |
         * |   (x',y'')  |                                           (X,y'')
         * |      o------+                     (X,Y-y')    +------+      o
         * |      |      |         +------+      o         |      |
         * |  L1  |  L2  |         |      |                |      |
         * |      |      |   -->   |      |                |      |(X-x',y')
         * +------o      |         |      |(x',Y-y'')      |      o------+
         * |   (x',y')   |         |  L1  o------+         |             |
         * |             |         |             |         |      L2     |
         * |             |         |             |         |             |
         * +-------------+         +-------------+         +-------------+
         *
         * Parameters:
         * i  - Array of three elements such that i[0] = x', i[1] = y' and i[2] = y''.
         *
         * q  - The rectangle to be divided. q = {X, Y, X, Y}.
         *
         * q1 - Array to store L1.
         *
         * q2 - Array to store L2.
         */

        public void StandardPositionB7(ref int[] i, ref int[] q, ref int[] q1, ref int[] q2)
        {
            /* L1 */
            q1[0] = q[0];
            q1[1] = FiveBlockData.Normalized[q[1] - i[1]];
            q1[2] = i[0];
            q1[3] = FiveBlockData.Normalized[q[1] - i[2]];

            /* L2 */
            q2[0] = q[0];
            q2[1] = i[2];
            q2[2] = FiveBlockData.Normalized[q[0] - i[0]];
            q2[3] = i[1];
        }

        /**
         * Divide the L-shaped piece in two new L-shaped pieces, according to
         * the subdivision B8, and put them in the standard position.
         *
         *                  (X,Y)                  (x,Y)
         * +------------+     o         +------+     o
         * |            |               |      |
         * |   (x',y')  |               |      |                     (X-x',y')
         * |      o-----+               |      |              +-----+     o
         * |      |     |               |  L1  |              |     |
         * |  L1  |     |(x,y)    -->   |      |(x',Y-y')     |     |(x-x',y)
         * |      |     o-----+         |      o-----+        |     o-----+
         * |      |  L2       |         |            |        |  L2       |
         * |      |           |         |            |        |           |
         * +------+-----------+         +------------+        +-----------+
         *
         * Parameters:
         * i  - Array of two elements such that i[0] = x' and i[1] = y'.
         *
         * q  - The L-shaped piece to be divided. q = {X, Y, x, y}.
         *
         * q1 - Array to store L1.
         *
         * q2 - Array to store L2.
         */

        public void StandardPositionB8(ref int[] i, ref int[] q, ref int[] q1, ref int[] q2)
        {
            /* L1 */
            q1[0] = q[2];
            q1[1] = q[1];
            q1[2] = i[0];
            q1[3] = FiveBlockData.Normalized[q[1] - i[1]];

            /* L2 */
            q2[0] = FiveBlockData.Normalized[q[0] - i[0]];
            q2[1] = i[1];
            q2[2] = FiveBlockData.Normalized[q[2] - i[0]];
            q2[3] = q[3];
        }

        /**
         * Divide the L-shaped piece in two new L-shaped pieces, according to
         * the subdivision B9, and put them in the standard position.
         *
         *                  (X,Y)
         * +---------+        o
         * |         |
         * |         |(x,y)                                                      (X,y)
         * |   L1    o---+----+                    (x',Y-y')    +----+             o
         * |             |    |         +---------+   o         |    |
         * |             |    |         |         |             |    |(X-x',y')
         * +-------------o    |   -->   |         |(x,y-y')     |    o-------------+
         * |          (x',y') |         |   L1    o---+         |                  |
         * |     L2           |         |             |         |        L2        |
         * |                  |         |             |         |                  |
         * +------------------+         +-------------+         +------------------+
         *
         * Parameters:
         * i  - Array of two elements such that i[0] = x' and i[1] = y'.
         *
         * q  - The L-shaped piece to be divided. q = {X, Y, x, y}.
         *
         * q1 - Array to store L1.
         *
         * q2 - Array to store L2.
         */

        public void StandardPositionB9(ref int[] i, ref int[] q, ref int[] q1, ref int[] q2)
        {
            /* L1 */
            q1[0] = i[0];
            q1[1] = FiveBlockData.Normalized[q[1] - i[1]];
            q1[2] = q[2];
            q1[3] = FiveBlockData.Normalized[q[3] - i[1]];

            /* L2 */
            q2[0] = q[0];
            q2[1] = q[3];
            q2[2] = FiveBlockData.Normalized[q[0] - i[0]];
            q2[3] = i[1];
        }
    }
}