using System;

namespace AutoNav.Library.RecursivePartitioning.Algorithm
{
    using Model;
    using Model.Shared;

    public class FiveBlock
    {
        private const int INFINITY = 200000000;

        /* Maximum level of recursion (maximum tree search depth). */
        private int _maxRecurDepth = INFINITY;

        /* Indicate in which level of the recursion (or in which depth of the
         * tree search) the LBlockData.Solution was found. Initially, solutionDepth[L][W]
         * = N, for all (L,W) sub problem. If the optimal LBlockData.Solution was found
         * for a sub problem (L,W), than solutionDepth[L][W] = -1. */
        private int[][] _solutionDepth;

        /* Indicate if the limit of the recursion was reached during the
         * resolution of a problem. */
        private int[][] _reachedLimit;

        /* Algorithm data shared with LBlock algorithm */
        public FiveBlockShared SharedData { get; set; }

        private int findLowerBound(int L, int W)
        {
            return Math.Max((L/SharedData.Parameters.l)*(W/SharedData.Parameters.w),
                (L/SharedData.Parameters.w)*(W/SharedData.Parameters.l));
        }

        /**
        * Compute the Barnes's upper bound [3].
        *
        * [3] F. W. Barnes. Packing the maximum number of m x n tiles in a
        *     large p x q rectangle. Discrete Mathematics, volume 26,
        *     pages 93-100, 1979.
        *
        * Parameters:
        * (L, W) Pallet dimensions.
        * (l, w) Dimensions of the boxes to be packed.
        *
        * Return:
        * - the computed Barnes's bound.
        */

        private int barnesBound(int L, int W)
        {
            int r, s, D;
            int minWaste = (L*W)%(SharedData.Parameters.l*SharedData.Parameters.w);

            /* (SharedData.Parameters.l,1)-boxes packing. */
            r = L%SharedData.Parameters.l;
            s = W%SharedData.Parameters.l;
            int A = Math.Min(r*s, (SharedData.Parameters.l - r)*(SharedData.Parameters.l - s));

            /* (1,SharedData.Parameters.w)-boxes packing. */
            r = L%SharedData.Parameters.w;
            s = W%SharedData.Parameters.w;
            int B = Math.Min(r*s, (SharedData.Parameters.w - r)*(SharedData.Parameters.w - s));

            /* Best unitary tile packing. */
            int maxAB = Math.Max(A, B);

            if (minWaste >= maxAB%(SharedData.Parameters.l*SharedData.Parameters.w))
            {
                /* Wasted area. */
                D = (maxAB/(SharedData.Parameters.l*SharedData.Parameters.w))*
                    (SharedData.Parameters.l*SharedData.Parameters.w) + minWaste;
            }
            else
            {
                /* Wasted area. */
                D = (maxAB/(SharedData.Parameters.l*SharedData.Parameters.w) + 1)*
                    (SharedData.Parameters.l*SharedData.Parameters.w) + minWaste;
            }

            return (L*W - D)/(SharedData.Parameters.l*SharedData.Parameters.w);
        }

        private void initialize()
        {
            /* Normalization of L and W. */
            int L_n;
            int W_n;

            int i;
            int j;


            /* Construct the conic combination set of l and w. */
            SharedData.NormalSetX = Sets.Instance(SharedData.Parameters).ConstructConicCombinations();

            /* Compute the values of L* and W*.
             * normalize[i] = max {x in X | x <= i} */
            SharedData.Normalized = new int[SharedData.Parameters.L + 1];
            i = 0;
            for (j = 0; j <= SharedData.Parameters.L; j++)
            {
                for (; i < SharedData.NormalSetX.Size && SharedData.NormalSetX.Points[i] <= j; i++)
                {
                    ;
                }
                SharedData.Normalized[j] = SharedData.NormalSetX.Points[i - 1];
            }

            /* Normalize (L, W). */
            L_n = SharedData.Normalized[SharedData.Parameters.L];
            W_n = SharedData.Normalized[SharedData.Parameters.W];

            var rasterX = new Set();
            var rasterY = new Set();
            Sets.Instance().ConstructRasterPoints(ref rasterX, ref rasterY, SharedData.Normalized, SharedData.NormalSetX);

            SharedData.NormalSetX.ClearPoints();
            SharedData.NormalSetX = new Set(L_n + 2);
            var k = 0;
            i = 0;
            j = 0;
            while (i < rasterX.Size && rasterX.Points[i] <= L_n && j < rasterY.Size && rasterY.Points[j] <= W_n)
            {
                if (rasterX.Points[i] == rasterY.Points[j])
                {
                    SharedData.NormalSetX.Points[k++] = rasterX.Points[i++];
                    SharedData.NormalSetX.Size++;
                    j++;
                }
                else if (rasterX.Points[i] < rasterY.Points[j])
                {
                    SharedData.NormalSetX.Points[k++] = rasterX.Points[i++];
                    SharedData.NormalSetX.Size++;
                }
                else
                {
                    SharedData.NormalSetX.Points[k++] = rasterY.Points[j++];
                    SharedData.NormalSetX.Size++;
                }
            }
            while (i < rasterX.Size && rasterX.Points[i] <= L_n)
            {
                if (rasterX.Points[i] > SharedData.NormalSetX.Points[k - 1])
                {
                    SharedData.NormalSetX.Points[k++] = rasterX.Points[i];
                    SharedData.NormalSetX.Size++;
                }
                i++;
            }
            if (k > 0 && SharedData.NormalSetX.Points[k - 1] < L_n)
            {
                SharedData.NormalSetX.Points[k++] = L_n;
                SharedData.NormalSetX.Size++;
            }
            SharedData.NormalSetX.Points[k] = L_n + 1;
            SharedData.NormalSetX.Size++;

            rasterX.ClearPoints();
            rasterY.ClearPoints();

            /* Construct the array of indices. */
            SharedData.IndexX = new int[L_n + 2];
            SharedData.IndexY = new int[W_n + 2];

            for (i = 0; i < SharedData.NormalSetX.Size; i++)
            {
                SharedData.IndexX[SharedData.NormalSetX.Points[i]] = i;
            }
            int ySize = 0;
            for (i = 0; i < SharedData.NormalSetX.Size; i++)
            {
                if (SharedData.NormalSetX.Points[i] > W_n)
                {
                    break;
                }
                ySize++;
                SharedData.IndexY[SharedData.NormalSetX.Points[i]] = i;
            }

            _solutionDepth = new int[SharedData.NormalSetX.Size][];
            SharedData.UpperBound = new int[SharedData.NormalSetX.Size][];
            SharedData.LowerBound = new int[SharedData.NormalSetX.Size][];
            _reachedLimit = new int[SharedData.NormalSetX.Size][];
            SharedData.CutPoints = new CutPoint[SharedData.NormalSetX.Size][];

            for (i = 0; i < SharedData.NormalSetX.Size; i++)
            {
                _solutionDepth[i] = new int[ySize];
                SharedData.UpperBound[i] = new int[ySize];
                SharedData.LowerBound[i] = new int[ySize];
                SharedData.CutPoints[i] = new CutPoint[ySize];
                for (j = 0; j < ySize; j++)
                {
                    SharedData.CutPoints[i][j] = new CutPoint();
                }
                _reachedLimit[i] = new int[ySize];
            }

            for (i = 0; i < SharedData.NormalSetX.Size; i++)
            {
                int x = SharedData.NormalSetX.Points[i];

                for (j = 0; j < ySize; j++)
                {
                    int y = SharedData.NormalSetX.Points[j];

                    _solutionDepth[i][j] = _maxRecurDepth;
                    _reachedLimit[i][j] = 1;
                    SharedData.UpperBound[i][j] = barnesBound(x, y);
                    SharedData.LowerBound[i][j] = findLowerBound(x, y);
                    SharedData.CutPoints[i][j].Homogeneous = 1;
                }
            }
        }

        private int localUpperBound(int iX, int iY)
        {
            if (_solutionDepth[iX][iY] == -1)
            {
                return SharedData.LowerBound[iX][iY];
            }
            else
            {
                return SharedData.UpperBound[iX][iY];
            }
        }

        /**
         * Guillotine and first order non-guillotine cuts recursive procedure.
         *
         * Parameters:
         * L - Length of the pallet.
         * W - Width of the pallet.
         * l - Length of the boxes.
         * w - Width of the boxes.
         * n - Maximum search depth.
         *
         * Return:
         * - the number of (l,w)-boxes packed into (L,W) pallet.
         */

        private int cutProcedure(int L, int W, int n)
        {
            /* Lower and upper bounds for the number of (l,w)-boxes that can be
             * packed into (L,W). */
            int z_lb, z_ub;

            /* We assume that L >= W. */
            if (W > L)
            {
                Util.Swap(ref L, ref W);
            }

            z_lb = SharedData.LowerBound[SharedData.IndexX[L]][SharedData.IndexY[W]];
            z_ub = localUpperBound(SharedData.IndexX[L], SharedData.IndexY[W]);

            if (z_lb == 0 || z_lb == z_ub)
            {
                /* An optimal LBlockData.Solution was found: no box fits into the pallet or
                 * lower and upper bounds are equal. */
                _solutionDepth[SharedData.IndexX[L]][SharedData.IndexY[W]] = -1;
                _reachedLimit[SharedData.IndexX[L]][SharedData.IndexY[W]] = 0;
                return z_lb;
            }
            else
            {
                /* Points that determine the pallet division. */
                int x1, x2, y1, y2;

                /* Indices of x1, x2, y1 and y2 in the raster points arrays. */
                int index_x1, index_x2, index_y1, index_y2;

                /* Size of the generated partitions:
                 * (L_[i], W_[i]) is the size of the partition i, for i = 1, ..., 5. */
                var L_ = new int[6];
                var W_ = new int[6];

                /* Raster points sets X' and Y' for this problem. */
                Set rasterX = null;
                Set rasterY = null;

                bool solved;

                /* Construct the raster points sets. */
                Sets.Instance().ConstructRasterPoints(ref rasterX, ref rasterY, SharedData.Normalized, SharedData.NormalSetX);

                _reachedLimit[SharedData.IndexX[L]][SharedData.IndexY[W]] = 0;

                /*
                 * Loop to generate the cut points (x1, x2, y1 and y2) considering
                 * the following symmetries.
                 *
                 * - First order non-guillotine cuts:
                 *   0 < x1 < x2 < L and 0 < y1 < y2 < W
                 *   (x1 + x2 < L) or (x1 + x2 = L and y1 + y2 <= W)
                 *
                 * - Vertical guillotine cuts:
                 *   0 < x1 = x2 <= L/2 and y1 = y2 = 0
                 *
                 * - Horizontal guillotine cuts:
                 *   0 < y1 = y2 <= W/2 and x1 = x2 = 0
                 * 
                 * Observation: In the loop of non-guillotine cuts it can appear less
                 * than five partitions in the case of the normalization of a side
                 * of a partition be zero.
                 */

                /*#################################*
                 * First order non-guillotine cuts *
                 *#################################*/

                /*
                     L_1     L_2
                    ----------------
                   |     |    2     |W_2
                W_1|  1  |          |
                   |     |----------|
                   |     | 3 |      |
                   |---------|      |
                   |         |  5   |W_5
                W_4|    4    |      |
                   |         |      |
                    ----------------
                       L_4     L_5
                */

                for (index_x1 = 1;
                    index_x1 < rasterX.Size &&
                    rasterX.Points[index_x1] <= L/2;
                    index_x1++)
                {
                    x1 = rasterX.Points[index_x1];

                    for (index_x2 = index_x1 + 1;
                        index_x2 < rasterX.Size &&
                        rasterX.Points[index_x2] + x1 <= L;
                        index_x2++)
                    {
                        x2 = rasterX.Points[index_x2];

                        for (index_y1 = 1;
                            index_y1 < rasterY.Size &&
                            rasterY.Points[index_y1] < W;
                            index_y1++)
                        {
                            y1 = rasterY.Points[index_y1];

                            for (index_y2 = index_y1 + 1;
                                index_y2 < rasterY.Size &&
                                rasterY.Points[index_y2] < W;
                                index_y2++)
                            {
                                y2 = rasterY.Points[index_y2];

                                /* Symmetry. When x1 + x2 = L, we can restrict y1 and y2
                                 * to y1 + y2 <= W. */
                                if (x1 + x2 == L && y1 + y2 > W)
                                    break;

                                /* The five partitions. */
                                L_[1] = x1;
                                W_[1] = W - y1;

                                L_[2] = L - x1;
                                W_[2] = W - y2;

                                L_[3] = x2 - x1;
                                W_[3] = y2 - y1;

                                L_[4] = x2;
                                W_[4] = y1;

                                L_[5] = L - x2;
                                W_[5] = y2;

                                solved = solve(L, W, n, 5, L_, W_, ref z_lb, z_ub,
                                    x1, x2, y1, y2);

                                if (solved)
                                {
                                    /* This problem was solved with optimality guarantee. */
                                    rasterX.ClearPoints();
                                    rasterY.ClearPoints();
                                    return z_lb;
                                }
                            } /* for y2 */
                        } /* for y1 */
                    } /* for x2 */
                } /* for x1 */

                /*###########################*
                 * Vertical guillotine cuts. *
                 *###########################*/

                /*
                   ----------------
                  |     |          |
                  |     |          |
                  |     |          |
                  |  1  |    2     |
                  |     |          |
                  |     |          |
                  |     |          |
                  |     |          |
                   ----------------
                */

                for (index_x1 = 1;
                    index_x1 < rasterX.Size &&
                    rasterX.Points[index_x1] <= L/2;
                    index_x1++)
                {
                    x1 = x2 = rasterX.Points[index_x1];
                    y1 = y2 = 0;

                    /* Partitions 1 and 2 generated by the vertical cut. */
                    L_[1] = x1;
                    W_[1] = W - y1;

                    L_[2] = L - x1;
                    W_[2] = W - y2;

                    solved = solve(L, W, n, 2, L_, W_, ref z_lb, z_ub, x1, x2, y1, y2);
                    if (solved)
                    {
                        /* This problem was solved with optimality guarantee. */
                        rasterX.ClearPoints();
                        rasterY.ClearPoints();
                        return z_lb;
                    }
                }

                /*#############################*
                 * Horizontal guillotine cuts. *
                 *#############################*/

                /*
                   ----------------
                  |       2        |
                  |                |
                  |----------------|
                  |                |
                  |                |
                  |       5        |
                  |                |
                  |                |
                   ----------------
                */

                for (index_y1 = 1;
                    index_y1 < rasterY.Size &&
                    rasterY.Points[index_y1] <= W/2;
                    index_y1++)
                {
                    y1 = y2 = rasterY.Points[index_y1];
                    x1 = x2 = 0;

                    /* Partitions 2 and 5 generated by the horizontal cut. */
                    L_[1] = L - x1;
                    W_[1] = W - y2;

                    L_[2] = L - x2;
                    W_[2] = y2;

                    solved = solve(L, W, n, 2, L_, W_, ref z_lb, z_ub, x1, x2, y1, y2);
                    if (solved)
                    {
                        /* This problem was solved with optimality guarantee. */
                        rasterX.ClearPoints();
                        rasterY.ClearPoints();
                        return z_lb;
                    }
                }

                rasterX.ClearPoints();
                rasterY.ClearPoints();
                return z_lb;
            }
        }

        /// <summary>
        /// The result
        /// </summary>
        private int _solution;


        /** 
         * Parameters:
         * Input Data: L, W, l, w
         * N_max - Maximum depth.
         */

        public int Solve(Parameters parameters)
        {
            this.SharedData = new FiveBlockShared();
            SharedData.Parameters = parameters;

            int L_n;
            int W_n;

            _maxRecurDepth = SharedData.Parameters.RecurDepth;
            if (_maxRecurDepth <= 0)
            {
                _maxRecurDepth = INFINITY;
            }

            /* We assume that L >= W. */
            if (SharedData.Parameters.W > SharedData.Parameters.L)
            {
                Util.Swap(ref SharedData.Parameters.L, ref SharedData.Parameters.W);
            }

            initialize();

            /* Normalize (L, W). */
            L_n = SharedData.Normalized[SharedData.Parameters.L];
            W_n = SharedData.Normalized[SharedData.Parameters.W];

            var solution = cutProcedure(L_n, W_n, _maxRecurDepth);

            if (solution != SharedData.UpperBound[SharedData.IndexX[L_n]][SharedData.IndexY[W_n]] && _maxRecurDepth != 1)
            {
                /* If the LBlockData.Solution found is not optimal (or, at least, if it is not
                 * possible to prove its optimality), solve from the first level. */
                solution = cutProcedure(L_n, W_n, 1);
            }

            SharedData.LowerBound[SharedData.IndexX[L_n]][SharedData.IndexY[W_n]] = solution;

            _reachedLimit = null;
            _solutionDepth = null;

            _solution = solution;

            return solution;
        }

        public bool IsSolutionOptimal()
        {
            var L_n = SharedData.Normalized[SharedData.Parameters.L];
            var W_n = SharedData.Normalized[SharedData.Parameters.W];

            return (_solution != SharedData.UpperBound[SharedData.IndexX[L_n]][SharedData.IndexY[W_n]]);
        }

        /**
         * Auxiliary function.
         *
         * Parameters:
         *
         * L - Length of the pallet.
         * W - Width of the pallet.
         * l - Length of the boxes.
         * w - Width of the boxes.
         * n - Maximum search depth.
         *
         * numBlocks - Number of blocks in the current division of the pallet.
         *
         * L_, W_ - Lengths and widths for each partition of the pallet.
         *
         * z_lb   - Current lower bound for (L,W).
         * z_ub   - Upper bound for (L,W).
         *
         * x1, x2, y1, y2 - Points that determine the division of the pallet.
         *
         * Return:
         * - the number of (l,w)-boxes packed into (L,W) pallet, using the
         *   division determined by (x1,x2,y1,y2).
         */

        private bool solve(int L, int W, int n, int numBlocks, int[] L_, int[] W_,
            ref int z_lb, int z_ub, int x1, int x2, int y1, int y2)
        {
            /* z[1..5] stores the amount of boxes packed into partitions 1 to 5. */
            int[] z = new int[6];

            /* Sum of the lower and upper bounds for the number of boxes that
             * can be packed into partitions 1 to 5. */
            int S_lb, S_ub;

            /* Indices of the sub problems in the indexing matrices. */
            var iX = new int[6];
            var iY = new int[6];

            /* Lower and upper bounds for each partition. */
            var zi_ub = new int[6];
            var zi_lb = new int[6];

            int i;

            /* Normalize each rectangle produced. */
            for (i = 1; i <= numBlocks; i++)
            {
                /* Normalize the size of the rectangle (Li,Wi). */
                L_[i] = SharedData.Normalized[L_[i]];
                W_[i] = SharedData.Normalized[W_[i]];

                /* We assume that Li >= Wi. */
                if (L_[i] < W_[i])
                {
                    Util.Swap(ref L_[i], ref W_[i]);
                }

                /* Get the indices of each sub problem in the indexing matrices. */
                iX[i] = SharedData.IndexX[L_[i]];
                iY[i] = SharedData.IndexY[W_[i]];
            }

            /* If maximum level of the recursion was not reached. */
            if (n < _maxRecurDepth)
            {
                /* Store the sum of best packing estimations in the 5 partitions
                 * until this moment. Initially, it receives the sum of the lower
                 * bounds. */
                S_lb = 0;

                /* Sum of the upper bounds in the 5 partitions. */
                S_ub = 0;

                /* Compute the lower (zi_lb) and upper (zi_ub) bounds of each
                 * partition. */
                for (i = 1; i <= numBlocks; i++)
                {
                    /* Lower bound of (Li, Wi). */
                    zi_lb[i] = SharedData.LowerBound[iX[i]][iY[i]];
                    S_lb += zi_lb[i];
                    /* Upper bound of (Li, Wi). */
                    zi_ub[i] = localUpperBound(iX[i], iY[i]);
                    S_ub += zi_ub[i];
                }

                if (z_lb < S_ub)
                {
                    /* The current lower bound is less than the sum of the partitions
                     * upper bounds. Then, there is a possibility of this division
                     * improve the LBlockData.Solution. */

                    for (i = 1; i <= numBlocks; i++)
                    {
                        if (_solutionDepth[iX[i]][iY[i]] > n)
                        {
                            /* Solve for the first time or give another chance for
                             * this problem. */
                            z[i] = cutProcedure(L_[i], W_[i], n + 1);
                            SharedData.LowerBound[iX[i]][iY[i]] = z[i];

                            if (_reachedLimit[iX[i]][iY[i]] == 0)
                            {
                                _solutionDepth[iX[i]][iY[i]] = -1;
                            }
                            else
                            {
                                _solutionDepth[iX[i]][iY[i]] = n;
                            }
                        }
                        else
                        {
                            /* This problem was already solved. It gets the
                             * LBlockData.Solution obtained previously. */
                            z[i] = SharedData.LowerBound[iX[i]][iY[i]];
                        }

                        if (_reachedLimit[iX[i]][iY[i]] == 1)
                        {
                            _reachedLimit[SharedData.IndexX[L]][SharedData.IndexY[W]] = 1;
                        }

                        /* Update lower and upper bounds for this partitioning. */
                        S_lb = S_lb - zi_lb[i] + z[i];
                        S_ub = S_ub - zi_ub[i] + z[i];

                        /* If z_lb >= S_ub, we have, at least, a LBlockData.Solution as good as
                         * the one that can be find with this partitioning. So this
                         * partitioning is discarded. */
                        if (z_lb >= S_ub)
                        {
                            break;
                        }

                            /* If the sum of packs in the current partitions is better
                         * than the previous, update the lower bound. */
                        else if (S_lb > z_lb)
                        {
                            z_lb = S_lb;
                            storeCutPoint(L, W, x1, x2, y1, y2);
                            if (z_lb == z_ub)
                            {
                                /* An optimal LBlockData.Solution was found. */
                                _solutionDepth[SharedData.IndexX[L]][SharedData.IndexY[W]] = -1;
                                _reachedLimit[SharedData.IndexX[L]][SharedData.IndexY[W]] = 0;
                                return true;
                            }
                        }
                    }
                }
            } /* if n < N */

                /* The maximum depth of recursion was reached. Then, each partition
             * will not be solved recursively. Each one receives the best
             * packing obtained until this moment. */
            else
            {
                _reachedLimit[SharedData.IndexX[L]][SharedData.IndexY[W]] = 1;
                S_lb = 0;

                /* Compute the lower bound of each partition and the sum is
                 * stored in S_lb. */
                for (i = 1; i <= numBlocks; i++)
                {
                    S_lb += SharedData.LowerBound[iX[i]][iY[i]];
                }

                /* If the sum of the homogeneous packing in all current
                 * partitions is better than the previous estimation for (L,W),
                 * update the lower bound. */
                if (S_lb > z_lb)
                {
                    z_lb = S_lb;
                    storeCutPoint(L, W, x1, x2, y1, y2);
                    if (z_lb == z_ub)
                    {
                        /* An optimal LBlockData.Solution was found. */
                        _solutionDepth[SharedData.IndexX[L]][SharedData.IndexY[W]] = -1;
                        _reachedLimit[SharedData.IndexX[L]][SharedData.IndexY[W]] = 0;
                        return true;
                    }
                }
            }
            return false;
        }

        /**
        * Store the points x1, x2, y1 and y2 that determine the cut in the
        * rectangle (L,W).
        */

        private void storeCutPoint(int L, int W, int x1, int x2, int y1, int y2)
        {
            var c = new CutPoint {X1 = x1, X2 = x2, Y1 = y1, Y2 = y2, Homogeneous = 0};
            SharedData.CutPoints[SharedData.IndexX[L]][SharedData.IndexY[W]] = c;
        }
    }
}