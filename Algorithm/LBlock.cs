using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoNav.Library.RecursivePartitioning.Algorithm
{
    using Model;
    using Model.Shared;

    public class LBlock
    {
        public LBlockShared SharedData { get; set; }

        private void makeIndices()
        {
            var L = SharedData.FiveBlockData.NormalizedL;
            var W = SharedData.FiveBlockData.NormalizedW;

            var parameters = new Parameters {L = L, W = W};

            Set X = new Set();
            Set Y = new Set();
            Set raster = new Set();
            Sets.Instance(parameters)
                .ConstructRasterPoints(ref X, ref Y, SharedData.FiveBlockData.Normalized,
                    SharedData.FiveBlockData.NormalSetX);

            int j = 0;
            int k = 0;
            int i = 0;

            raster = new Set(L + 2);
            while (i < X.Size && X.Points[i] <= L && j < Y.Size && Y.Points[j] <= W)
            {
                if (X.Points[i] == Y.Points[j])
                {
                    raster.Points[k++] = X.Points[i++];
                    raster.Size++;
                    j++;
                }
                else if (X.Points[i] < Y.Points[j])
                {
                    raster.Points[k++] = X.Points[i++];
                    raster.Size++;
                }
                else
                {
                    raster.Points[k++] = Y.Points[j++];
                    raster.Size++;
                }
            }
            while (i < X.Size && X.Points[i] <= L)
            {
                if (X.Points[i] > raster.Points[k - 1])
                {
                    raster.Points[k++] = X.Points[i];
                    raster.Size++;
                }
                i++;
            }
            if (k > 0 && raster.Points[k - 1] < L)
            {
                raster.Points[k++] = L;
                raster.Size++;
            }
            raster.Points[k] = L + 1;
            raster.Size++;

            try
            {
                SharedData.IndexRasterX = new int[L + 2];
                SharedData.IndexRasterY = new int[W + 2];
            }
            catch (Exception)
            {
                Console.Write("Error allocating memory.");
                Console.Write("\n");
                Environment.Exit(0);
            }

            j = 0;
            SharedData.NumRasterX = 0;
            for (var a = 0; a <= L; a++)
            {
                if (raster.Points[j] == a)
                {
                    SharedData.IndexRasterX[a] = SharedData.NumRasterX++;
                    j++;
                }
                else
                {
                    SharedData.IndexRasterX[a] = SharedData.IndexRasterX[a - 1];
                }
            }
            SharedData.IndexRasterX[L + 1] = SharedData.IndexRasterX[L] + 1;

            j = 0;
            SharedData.NumRasterY = 0;
            for (var b = 0; b <= W; b++)
            {
                if (raster.Points[j] == b)
                {
                    SharedData.IndexRasterY[b] = SharedData.NumRasterY++;
                    j++;
                }
                else
                {
                    SharedData.IndexRasterY[b] = SharedData.IndexRasterY[b - 1];
                }
            }
            SharedData.IndexRasterY[W + 1] = SharedData.IndexRasterY[W] + 1;

            raster.ClearPoints();
            X.ClearPoints();
            Y.ClearPoints();
        }

        private bool tryAllocateMemory(int size)
        {
            try
            {
                SharedData.SolutionMap = new SortedDictionary<int, int>[size];
            }
            catch (Exception)
            {
                if (size == 0)
                {
                    Console.Write("Error allocating memory.");
                    Console.Write("\n");
                    Environment.Exit(0);
                }
                return false;
            }
            try
            {
                SharedData.DivisionPointMap = new SortedDictionary<int, int>[size];
            }
            catch (Exception)
            {
                SharedData.SolutionMap = null;
                SharedData.DivisionPointMap = null;
                if (size == 0)
                {
                    Console.Write("Error allocating memory.");
                    Console.Write("\n");
                    Environment.Exit(0);
                }
                return false;
            }
            return true;
        }

        private int roundToNearest(double a)
        {
            return (int) Math.Floor(a + 0.5);
        }

        private void allocateMemory()
        {
            SharedData.MemoryType = Constants.MEM_TYPE_4;

            int nL =
                roundToNearest(
                    (Math.Pow((double) SharedData.NumRasterX, Math.Ceiling((double) SharedData.MemoryType/2.0))*
                     Math.Pow((double) SharedData.NumRasterY, Math.Floor((double) SharedData.MemoryType/2.0))));

            SharedData.MemoryType--;

            if (nL >= 0)
            {
                try
                {
                    SharedData.Solution = new int[nL];
                    try
                    {
                        SharedData.DivisionPoint = new int[nL];
                        for (int i = 0; i < nL; i++)
                        {
                            SharedData.Solution[i] = -1;
                        }
                    }
                    catch (Exception)
                    {
                        SharedData.Solution = null;
                        do
                        {
                            nL =
                                roundToNearest(
                                    (Math.Pow((double) SharedData.NumRasterX,
                                        Math.Ceiling((double) SharedData.MemoryType/2.0))*
                                     Math.Pow((double) SharedData.NumRasterY,
                                         Math.Floor((double) SharedData.MemoryType/2.0))));

                            SharedData.MemoryType--;

                            if (nL >= 0 && tryAllocateMemory(nL))
                            {
                                break;
                            }
                        } while (SharedData.MemoryType >= 0);
                    }
                }
                catch (Exception)
                {
                    do
                    {
                        nL =
                            roundToNearest(
                                (Math.Pow((double) SharedData.NumRasterX,
                                    Math.Ceiling((double) SharedData.MemoryType/2.0))*
                                 Math.Pow((double) SharedData.NumRasterY, Math.Floor((double) SharedData.MemoryType/2.0))));

                        SharedData.MemoryType--;

                        if (nL >= 0 && tryAllocateMemory(nL))
                        {
                            break;
                        }
                    } while (SharedData.MemoryType >= 0);
                }
            }
            else
            {
                do
                {
                    nL =
                        roundToNearest(
                            (Math.Pow((double) SharedData.NumRasterX, Math.Ceiling((double) SharedData.MemoryType/2.0))*
                             Math.Pow((double) SharedData.NumRasterY, Math.Floor((double) SharedData.MemoryType/2.0))));

                    SharedData.MemoryType--;

                    if (nL >= 0 && tryAllocateMemory(nL))
                    {
                        break;
                    }
                } while (SharedData.MemoryType >= 0);
            }
            SharedData.MemoryType++;
        }


        /**
         * Store the point where the division in the L-piece was made.
         *
         * Parameters:
         * L     - Index of the L-piece.
         *
         * key   - Key for this L-piece.
         *
         * point - Representation of the point where the division was made.
         *
         */

        private void storeDivisionPoint(int L, int key, int point)
        {
            if (SharedData.MemoryType == Constants.MEM_TYPE_4)
            {
                SharedData.DivisionPoint[L] = point;
            }
            else
            {
                SharedData.DivisionPointMap[L][key] = point;
            }
        }

        /**
         * Store the LBlockData.Solution of an L-piece.
         *
         * Parameters:
         * L         - Index of the L-piece.
         *
         * key       - Key for this L-piece.
         *
         * LSolution - Solution to be stored.
         *
         */

        private void storeSolution(int L, int key, int LSolution)
        {
            if (SharedData.MemoryType == Constants.MEM_TYPE_4)
            {
                SharedData.Solution[L] = LSolution;
            }
            else
            {
                SharedData.SolutionMap[L][key] = LSolution;
            }
        }

        /**
         * Calculate the upper bound of a given rectangle R(x,y) (degenerated L).
         *
         * Parameters:
         * x - Length of the rectangle.
         *
         * y - Width of the rectangle.
         *
         * Return:
         * The computed upper bound.
         */

        private int R_UpperBound(int x, int y)
        {
            /* A(R) / lw */
            x = SharedData.FiveBlockData.Normalized[x];
            y = SharedData.FiveBlockData.Normalized[y];
            return
                SharedData.FiveBlockData.UpperBound[SharedData.FiveBlockData.IndexX[x]][
                    SharedData.FiveBlockData.IndexY[y]];
        }

        /**
         * Calculate the upper bound of a given L.
         *
         * Parameters:
         * q - The L-piece.
         *
         * Return:
         * The computed upper bound for this L-piece.
         */

        private int L_UpperBound(int[] q)
        {
            /* Area(L) / lw */
            return (q[0]*q[1] - (q[0] - q[2])*(q[1] - q[3]))/
                   (SharedData.FiveBlockData.Parameters.l*SharedData.FiveBlockData.Parameters.w);
        }

        /**
         * Calculate the lower bound of a given L. It divides the L in two
         * rectangles and calculates their lower bounds to compose the lower
         * bound of the L-piece.
         *
         * +-----+              +-----+              +-----+
         * |     |              |     |              |     |
         * |     |              |     |              |     |
         * |     +----+   -->   +-----+----+    or   |     +----+
         * |          |         |          |         |     |    |
         * |          |         |          |         |     |    |
         * +----------+         +----------+         +-----+----+
         *                           (a)                  (b)
         *
         * Parameters:
         * q - The L-piece.
         *
         * Return:
         * The computed lower bound.
         */

        private int L_LowerBound(int[] q, ref bool horizontalCut)
        {
            int a =
                SharedData.FiveBlockData.LowerBound[
                    SharedData.FiveBlockData.IndexX[SharedData.FiveBlockData.Normalized[q[2]]]][
                        SharedData.FiveBlockData.IndexY[SharedData.FiveBlockData.Normalized[q[1] - q[3]]]] +
                SharedData.FiveBlockData.LowerBound[
                    SharedData.FiveBlockData.IndexX[SharedData.FiveBlockData.Normalized[q[0]]]][
                        SharedData.FiveBlockData.IndexX[SharedData.FiveBlockData.Normalized[q[3]]]];

            int b =
                SharedData.FiveBlockData.LowerBound[
                    SharedData.FiveBlockData.IndexX[SharedData.FiveBlockData.Normalized[q[2]]]][
                        SharedData.FiveBlockData.IndexY[SharedData.FiveBlockData.Normalized[q[1]]]] +
                SharedData.FiveBlockData.LowerBound[
                    SharedData.FiveBlockData.IndexX[SharedData.FiveBlockData.Normalized[q[0] - q[2]]]][
                        SharedData.FiveBlockData.IndexY[SharedData.FiveBlockData.Normalized[q[3]]]];

            if (a > b)
            {
                horizontalCut = true;
                return a;
            }
            else
            {
                horizontalCut = false;
                return b;
            }
        }

        /**
         * Solve the problem of packing rectangular (l,w)-boxes into the
         * specified L-shaped piece.
         *
         * Parameters:
         * L - Index of the L-piece.
         *
         * q - The L-piece. q = {X, Y, x, y}.
         */

        public int Solve(int L, int[] q)
        {
            makeIndices();

            allocateMemory();

            int key = 0;
            if (SharedData.MemoryType == Constants.MEM_TYPE_4)
            {
                if (SharedData.Solution[L] != -1)
                {
                    /* This problem has already been solved. */
                    return SharedData.Solution[L];
                }
            }
            else
            {
                key = SharedData.GetKey(q);
                if (SharedData.SolutionMap[L].Count(x => x.Key == key) > 0)
                {
                    /* This problem has already been solved. */
                    return SharedData.SolutionMap[L][key];
                }
            }

            if (q[0] != q[2])
            {
                bool horizontalCut = false;
                int lowerBound = L_LowerBound(q, ref horizontalCut);
                int upperBound = L_UpperBound(q);
                int LSolution = lowerBound | (Constants.B1 << Constants.descSol);

                if (horizontalCut)
                    storeDivisionPoint(L, key, 0 | (q[3] << Constants.descPtoDiv2));
                else
                    storeDivisionPoint(L, key, q[2] | (0 << Constants.descPtoDiv2));

                storeSolution(L, key, LSolution);

                /* Try to solve this problem with homogeneous packing (or other
                 * better LBlockData.Solution already computed). */
                if ((LSolution & Constants.nRet) != upperBound)
                {
                    /* It was not possible to solve this problem with homogeneous
                     * packing. */
                    var constraints = new int[4];
                    int startX = 0;
                    int startY = 0;

                    /* Construct the raster points sets X and Y. */
                    var X = new Set();
                    var Y = new Set();
                    var parameters = new Parameters {L = q[0], W = q[1]};
                    Sets.Instance(parameters)
                        .ConstructRasterPoints(ref X, ref Y, SharedData.FiveBlockData.Normalized,
                            SharedData.FiveBlockData.NormalSetX);
                    for (startX = 0; X.Points[startX] < q[2]; startX++) ;
                    for (startY = 0; Y.Points[startY] < q[3]; startY++) ;

                    /***********************************
                     * 0 <= x' <= x  and  0 <= y' <= y *
                     ***********************************/
                    constraints[0] = 0;
                    constraints[1] = q[2];
                    constraints[2] = 0;
                    constraints[3] = q[3];

                    /* B1 subdivision. 
                     *
                     * +------------+
                     * |            |
                     * |            |(x,y)
                     * |      +-----o-----+
                     * |  L1  |           |
                     * |      |     L2    |
                     * +------o           |
                     * |   (x',y')        |
                     * |                  |
                     * +------------------+
                     */
                    LSolution = divideL(L, ref q, constraints, Constants.B1, SharedData.StandardPositionB1, X, 0, Y, 0);
                    if ((LSolution & Constants.nRet) == upperBound)
                    {
                        X.ClearPoints();
                        Y.ClearPoints();
                        return LSolution;
                    }

                    /* B3 subdivision.
                     *
                     * +------+-----+
                     * |      |     |
                     * |      |     |(x,y)
                     * |      | L2  o-----+
                     * |      |           |
                     * |  L1  |           |
                     * |      o-----------+
                     * |   (x',y')        |
                     * |                  |
                     * +------------------+
                     */
                    LSolution = divideL(L, ref q, constraints, Constants.B3, SharedData.StandardPositionB3, X, 0, Y, 0);
                    if ((LSolution & Constants.nRet) == upperBound)
                    {
                        X.ClearPoints();
                        Y.ClearPoints();
                        return LSolution;
                    }

                    /* B5 subdivision.
                     *
                     * +------------+
                     * |            |
                     * |     L1     |(x,y)
                     * |            o-----+
                     * |   (x',y')  |     |
                     * |      o-----+     |
                     * |      |           |
                     * |      |     L2    |
                     * |      |           |
                     * +------+-----------+
                     */
                    LSolution = divideL(L, ref q, constraints, Constants.B5, SharedData.StandardPositionB5, X, 0, Y, 0);
                    if ((LSolution & Constants.nRet) == upperBound)
                    {
                        X.ClearPoints();
                        Y.ClearPoints();
                        return LSolution;
                    }

                    /***********************************
                     * 0 <= x' <= x  and  y <= y' <= Y *
                     ***********************************/
                    constraints[0] = 0;
                    constraints[1] = q[2];
                    constraints[2] = q[3];
                    constraints[3] = Y.Points[Y.Size - 1];

                    /* B2 subdivision. 
                     *
                     * +------------+
                     * |            |
                     * |   (x',y')  |
                     * +------o     |
                     * |      | L1  |
                     * |      |     |(x,y)
                     * |      +-----o-----+
                     * |  L2              |
                     * |                  |
                     * +------------------+
                     */
                    LSolution = divideL(L, ref q, constraints, Constants.B2, SharedData.StandardPositionB2,
                        X, 0, Y, startY);
                    if ((LSolution & Constants.nRet) == upperBound)
                    {
                        X.ClearPoints();
                        Y.ClearPoints();
                        return LSolution;
                    }

                    /* B8 subdivision.
                     *
                     * +------------+
                     * |            |
                     * |   (x',y')  |
                     * |      o-----+
                     * |      |     |
                     * |  L1  |     |(x,y)
                     * |      |     o-----+
                     * |      |  L2       |
                     * |      |           |
                     * +------+-----------+
                     */
                    LSolution = divideL(L, ref q, constraints, Constants.B8, SharedData.StandardPositionB8,
                        X, 0, Y, startY);
                    if ((LSolution & Constants.nRet) == upperBound)
                    {
                        X.ClearPoints();
                        Y.ClearPoints();
                        return LSolution;
                    }

                    /***********************************
                     * x <= x' <= X  and  0 <= y' <= y *
                     ***********************************/
                    constraints[0] = q[2];
                    constraints[1] = X.Points[X.Size - 1];
                    constraints[2] = 0;
                    constraints[3] = q[3];

                    /* B4 subdivision.
                     *
                     * +------+
                     * |      |
                     * |      |(x,y)
                     * |      o-----------+
                     * |  L1  |           |
                     * |      |  (x',y')  |
                     * |      +-----o     |
                     * |            | L2  |
                     * |            |     |
                     * +------------+-----+
                     */
                    LSolution = divideL(L, ref q, constraints, Constants.B4, SharedData.StandardPositionB4,
                        X, startX, Y, 0);
                    if ((LSolution & Constants.nRet) == upperBound)
                    {
                        X.ClearPoints();
                        Y.ClearPoints();
                        return LSolution;
                    }

                    /* B9 subdivision.
                     *
                     * +---------+
                     * |         |
                     * |         |(x,y)
                     * |   L1    o---+----+
                     * |             |    |
                     * |             |    |
                     * +-------------o    |
                     * |          (x',y') |
                     * |     L2           |
                     * |                  |
                     * +------------------+
                     */
                    LSolution = divideL(L, ref q, constraints, Constants.B9, SharedData.StandardPositionB9,
                        X, startX, Y, 0);
                    X.ClearPoints();
                    Y.ClearPoints();
                }
                return LSolution;
            } /* if q[0] != q[2] */
            else
            {
                /* Degenerated L (a rectangle) */

                int LSolution = SharedData.R_LowerBound(q[0], q[1]) | (Constants.HOMOGENEOUS << Constants.descSol);
                int upperBound = R_UpperBound(q[0], q[1]);
                storeSolution(L, key, LSolution);

                /* Verify whether it could not be solved with homogeneous packing. */
                if ((LSolution & Constants.nRet) != upperBound)
                {
                    /* Construct the raster points sets X and Y. */
                    var X = new Set();
                    var Y = new Set();
                    Sets.Instance()
                        .ConstructRasterPoints(ref X, ref Y, SharedData.FiveBlockData.Normalized,
                            SharedData.FiveBlockData.NormalSetX);

                    /* Try the subdivisions B6 and B7. */

                    /* B6 subdivision.
                     *
                     * +-------------+--------+
                     * |             |        |
                     * |   (x',y')   |   L2   |
                     * |      o------o        |
                     * |      |  (x'',y')     |
                     * |  L1  |               |
                     * |      |               |
                     * +------+---------------+
                     */
                    LSolution = divideB6(L, q, X, Y);
                    if ((LSolution & Constants.nRet) == upperBound)
                    {
                        X.ClearPoints();
                        Y.ClearPoints();

                        /* Update the lower bound for this rectangular piece. */
                        SharedData.FiveBlockData.LowerBound[SharedData.FiveBlockData.IndexX[q[0]]][
                            SharedData.FiveBlockData.IndexY[q[1]]] = LSolution & Constants.nRet;

                        return LSolution;
                    }

                    /* B7 subdivision.
                     *
                     * +-------------+
                     * |             |
                     * |   (x',y'')  |
                     * |      o------+
                     * |      |      |
                     * |  L1  |  L2  |
                     * |      |      |
                     * +------o      |
                     * |   (x',y')   |
                     * |             |
                     * |             |
                     * +-------------+
                     */
                    LSolution = divideB7(L, q, X, Y);

                    X.ClearPoints();
                    Y.ClearPoints();

                    /* Update the lower bound for this rectangular piece. */
                    SharedData.FiveBlockData.LowerBound[SharedData.FiveBlockData.IndexX[q[0]]][
                        SharedData.FiveBlockData.IndexY[q[1]]] = LSolution & Constants.nRet;
                }
                return LSolution;
            }
        }

        /**
         * Return the LBlockData.Solution of the L-piece related to the index L.
         *
         * Parameters:
         * L   - Index of the L-piece.
         *
         * key - Key for this L-piece.
         *
         * Return:
         * The current LBlockData.Solution of the specified L-piece.
         */

        public int GetSolution(int L, int key)
        {
            if (SharedData.MemoryType == Constants.MEM_TYPE_4)
            {
                return SharedData.Solution[L] & Constants.nRet;
            }
            else
            {
                return SharedData.SolutionMap[L][key] & Constants.nRet;
            }
        }

        /**
         * Return the LBlockData.Solution of the L-piece related to the index L.
         *
         * Parameters:
         * L - Index of the L-piece.
         *
         * q - The L-piece.
         *
         * Returns:
         * The current LBlockData.Solution of the specified L-piece.
         */

        public int GetSolution(int L, int[] q)
        {
            if (SharedData.MemoryType == Constants.MEM_TYPE_4)
            {
                return SharedData.Solution[L] & Constants.nRet;
            }
            else
            {
                int key = SharedData.GetKey(q);
                return SharedData.SolutionMap[L][key] & Constants.nRet;
            }
        }

        /**
         * Return the LBlockData.Solution of the L-piece related to the index L.
         *
         * Parameters:
         * L   - Index of the L-piece.
         *
         * q   - The L-piece.
         *
         * key - Key for this L-piece.
         *
         * Return:
         * The current LBlockData.Solution of the specified L-piece.
         */

        private int getSolution(int L, int[] q, ref int key)
        {
            if (SharedData.MemoryType == Constants.MEM_TYPE_4)
            {
                return SharedData.Solution[L];
            }
            else
            {
                key = SharedData.GetKey(q);
                return SharedData.SolutionMap[L][key];
            }
        }

        /*
         * Parameters:
         * i  - Array of three elements such that i[0] = x', i[1] = y' and i[2] = x''.
         *
         * q  - The rectangle to be divided. q = {X, Y, X, Y}.
         *
         * q1 - Array to store L1.
         *
         * q2 - Array to store L2.
         */

        private delegate void standardPositionDelegate(ref int[] i, ref int[] q, ref int[] q1, ref int[] q2);

        /**
         * Divide an L-piece in two new L-pieces, according to the specified
         * subdivision, and normalize the two ones.
         *
         * Parameters:
         * i  - Point that determines the division int the L-piece.
         *
         * q  - The L-piece to be divided.
         *
         * q1 - It will store a new L-piece.
         *
         * q2 - It will store the other new L-piece.
         *
         * standardPosition - Pointer to the function that will divide the L-piece.
         */

        private void divide(ref int[] i, ref int[] q, ref int[] q1, ref int[] q2,
            standardPositionDelegate standardPosition)
        {
            /* Divide the L-piece in two new ones. */
            standardPosition(ref i, ref q, ref q1, ref q2);

            /* Normalize the new L-pieces. */
            SharedData.NormalizePiece(q1);
            SharedData.NormalizePiece(q2);
        }


        /**
         * Divide the L-piece in every possible way, according to the specified
         * subdivision B.
         *
         * Parameters:
         * L                - Index of the L-piece.
         *
         * q                - The L-piece.
         *
         * constraints      - Constraints that determine the interval of x' and y'.
         *
         * B                - The subdivision.
         *
         * standardPosition - Pointer to the function that divides the L-piece
         *                    according to the subdivision B.
         *
         * X                - Set of raster points.
         *
         * startX           - Index to start the divisions on the set X.
         *
         * Y                - Set of raster points.
         *
         * startY           - Index to start the divisions on the set Y.
         */

        private int divideL(int L, ref int[] q, int[] constraints, int B, standardPositionDelegate standardPosition,
            Set X, int startX, Set Y, int startY)
        {
            /* i_k[0] <- x'
             * i_k[1] <- y'
             */
            int[] i_k = new int[2];
            int i_x;
            int i_y;
            int[] q1 = new int[4];
            int[] q2 = new int[4];

            int key = 0;
            int LSolution = getSolution(L, q, ref key);
            int upperBound = L_UpperBound(q);

            for (i_x = startX; i_x < X.Size; i_x++)
            {
                i_k[0] = X.Points[i_x];
                if (i_k[0] > constraints[1])
                {
                    break;
                }

                for (i_y = startY; i_y < Y.Size; i_y++)
                {
                    i_k[1] = Y.Points[i_y];
                    if (i_k[1] > constraints[3])
                    {
                        break;
                    }

                    divide(ref i_k, ref q, ref q1, ref q2, standardPosition);
                    if (q1[0] < 0 || q2[0] < 0)
                    {
                        continue;
                    }

                    if (L_UpperBound(q1) + L_UpperBound(q2) > (LSolution & Constants.nRet))
                    {
                        /* It is possible that this division gets a better LBlockData.Solution. */
                        int L1 = SharedData.LIndex(q1);
                        int L2 = SharedData.LIndex(q2);
                        int L1Solution = Solve(L1, q1);
                        int L2Solution = Solve(L2, q2);

                        if ((L1Solution & Constants.nRet) + (L2Solution & Constants.nRet) > (LSolution & Constants.nRet))
                        {
                            /* A better LBlockData.Solution was found. */
                            LSolution = ((L1Solution & Constants.nRet) + (L2Solution & Constants.nRet)) |
                                        (B << Constants.descSol);
                            storeSolution(L, key, LSolution);
                            storeDivisionPoint(L, key, i_k[0] | (i_k[1] << Constants.descPtoDiv2));
                            if ((LSolution & Constants.nRet) == upperBound)
                            {
                                return LSolution;
                            }
                        }
                    }
                }
            }
            return LSolution;
        }


        /**
         * Divide the L-piece in every possible way, according to the B6
         * subdivision.
         *
         * +-------------+--------+
         * |             |        |
         * |   (x',y')   |   L2   |
         * |      o------o        |
         * |      |  (x'',y')     |
         * |  L1  |               |
         * |      |               |
         * +------+---------------+
         *
         * Parameters:
         * L - Index of the L-piece.
         *
         * q - The L-piece.
         *
         * X - Set of raster points.
         *
         * Y - Set of raster points.
         */

        private int divideB6(int L, int[] q, Set X, Set Y)
        {
            /* i_k[0] <- x'
             * i_k[1] <- y'
             * i_k[2] <- x''
             */
            var i_k = new int[3];
            int[] q1 = new int[4];
            int[] q2 = new int[4];
            int key = 0;
            int LSolution = getSolution(L, q, ref key);
            int upperBound = R_UpperBound(q[0], q[1]);

            int i = 0;
            for (i_k[0] = X.Points[i]; i < X.Size; i++)
            {
                i_k[0] = X.Points[i];

                int j = i;
                for (i_k[2] = X.Points[j]; j < X.Size; j++)
                {
                    i_k[2] = X.Points[j];
                    if (i_k[0] == 0 && i_k[2] == 0)
                    {
                        continue;
                    }

                    int k = 0;
                    for (i_k[1] = Y.Points[k]; k < Y.Size; k++)
                    {
                        i_k[1] = Y.Points[k];
                        divide(ref i_k, ref q, ref q1, ref q2, SharedData.StandardPositionB6);
                        if (q1[0] < 0 || q2[0] < 0)
                        {
                            continue;
                        }

                        if (L_UpperBound(q1) + L_UpperBound(q2) > (LSolution & Constants.nRet))
                        {
                            /* It is possible that this division gets a better LBlockData.Solution. */
                            int L1 = SharedData.LIndex(q1);
                            int L2 = SharedData.LIndex(q2);
                            int L1Solution = Solve(L1, q1);
                            int L2Solution = Solve(L2, q2);

                            if ((L1Solution & Constants.nRet) + (L2Solution & Constants.nRet) >
                                (LSolution & Constants.nRet))
                            {
                                /* A better LBlockData.Solution was found. */
                                LSolution = ((L1Solution & Constants.nRet) + (L2Solution & Constants.nRet)) |
                                            (Constants.B6 << Constants.descSol);
                                storeSolution(L, key, LSolution);
                                storeDivisionPoint(L, key,
                                    i_k[0] | (i_k[1] << Constants.descPtoDiv2) | (i_k[2] << Constants.descPtoDiv3));

                                if ((LSolution & Constants.nRet) == upperBound)
                                {
                                    return LSolution;
                                }
                            }
                        }
                    }
                }
            }
            return LSolution;
        }

        /**
         * Divide the L-piece in every possible way, according to the B7
         * subdivision.
         *
         * +-------------+
         * |             |
         * |   (x',y'')  |
         * |      o------+
         * |      |      |
         * |  L1  |  L2  |
         * |      |      |
         * +------o      |
         * |   (x',y')   |
         * |             |
         * |             |
         * +-------------+
         *
         * Parameters:
         * L - Index of the L-piece.
         *
         * q - The L-piece.
         *
         * X - Set of raster points.
         *
         * Y - Set of raster points.
         */

        private int divideB7(int L, int[] q, Set X, Set Y)
        {
            /* i_k[0] <- x'
             * i_k[1] <- y'
             * i_k[2] <- y''
             */
            int[] i_k = new int[3];
            int[] q1 = new int[4];
            int[] q2 = new int[4];
            int key = 0;
            int LSolution = getSolution(L, q, ref key);
            int upperBound = R_UpperBound(q[0], q[1]);

            int j = 0;
            for (i_k[1] = Y.Points[j]; j < Y.Size; j++)
            {
                i_k[1] = Y.Points[j];

                int k = j;
                for (i_k[2] = Y.Points[k]; k < Y.Size; k++)
                {
                    i_k[2] = Y.Points[k];
                    if (i_k[1] == 0 && i_k[2] == 0)
                    {
                        continue;
                    }

                    int i = 0;
                    for (i_k[0] = X.Points[i]; i < X.Size; i++)
                    {
                        i_k[0] = X.Points[i];
                        divide(ref i_k, ref q, ref q1, ref q2, SharedData.StandardPositionB7);
                        if (q1[0] < 0 || q2[0] < 0)
                        {
                            continue;
                        }

                        if (L_UpperBound(q1) + L_UpperBound(q2) > (LSolution & Constants.nRet))
                        {
                            /* It is possible that this division gets a better LBlockData.Solution. */
                            int L1 = SharedData.LIndex(q1);
                            int L2 = SharedData.LIndex(q2);
                            int L1Solution = Solve(L1, q1);
                            int L2Solution = Solve(L2, q2);

                            if (((L1Solution & Constants.nRet) + (L2Solution & Constants.nRet)) >
                                (LSolution & Constants.nRet))
                            {
                                /* A better LBlockData.Solution was found. */
                                LSolution = ((L1Solution & Constants.nRet) + (L2Solution & Constants.nRet)) |
                                            (Constants.B7 << Constants.descSol);
                                storeSolution(L, key, LSolution);
                                storeDivisionPoint(L, key,
                                    i_k[0] | (i_k[1] << Constants.descPtoDiv2) | (i_k[2] << Constants.descPtoDiv3));

                                if ((LSolution & Constants.nRet) == upperBound)
                                {
                                    return LSolution;
                                }
                            }
                        }
                    }
                }
            }
            return LSolution;
        }
    }
}