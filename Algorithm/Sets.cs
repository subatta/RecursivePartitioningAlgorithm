namespace AutoNav.Library.RecursivePartitioning.Algorithm
{
    using Model;

    internal class Sets
    {
        private static Parameters _params;
        private Set _conicCombinations;

        private static readonly Sets _instance = new Sets();

        public static Sets Instance()
        {
            return _instance;
        }

        public static Sets Instance(Parameters parameters)
        {
            _params = parameters;
            return _instance;
        }

        /**
         * Insert an element into the specified set, in the case it does not
         * belong to the set yet.
         *
         * Parameters:
         * element - The element to be inserted.
         * set - The set where the element will be inserted.
         *
         * Return:
         * The size of the set after this insertion.
         */

        private int insert(int element, ref Set set)
        {
            if (set.Size == 0 || set.Points[set.Size - 1] < element)
            {
                set.Points[set.Size] = element;
                return set.Size + 1;
            }
            return set.Size;
        }

        /**
         * Construct the raster points sets X' and Y', which are defined as
         *
         * X' = {<L - x>_X | x \in X} U {0}
         * Y' = {<W - y>_Y | y \in Y} U {0}
         *
         * where X and Y are the integer conic combinations sets for L and W,
         * respectively, and <s'>_S = max {s | s \in S, s <= s'}.
         *
         * Parameters:
         * L                 - Length of the rectangle.
         * W                 - Width of the rectangle.
         * rasterPointsX     - Pointer to the raster points set X'.
         * rasterPointsY     - Pointer to the raster points set Y'.
         * conicCombinations - Set of integer conic combinations of l and w.
         *
         * Remark: it supposes that the integer conic combinations set is
         * sorted.
         */

        public void ConstructRasterPoints(ref Set rasterPointsX, ref Set rasterPointsY, int[] normalize,
            Set conicCombinations = null)
        {
            if (conicCombinations != null)
                _conicCombinations = conicCombinations;

            int x;
            int i;

            /* Maximum raster points X size. */
            var xSize = _conicCombinations.Size;
            for (i = xSize - 1; i >= 0 && _conicCombinations.Points[i] > _params.L; i--)
            {
                xSize--;
            }

            /* Maximum raster points Y size. */
            var ySize = _conicCombinations.Size;
            for (i = ySize - 1; i >= 0 && _conicCombinations.Points[i] > _params.W; i--)
            {
                ySize--;
            }

            rasterPointsX = new Set(xSize + 2);
            rasterPointsY = new Set(ySize + 2);

            /* Construct the raster points for L. */
            for (i = xSize - 1; i >= 0; i--)
            {
                x = normalize[_params.L - _conicCombinations.Points[i]];
                rasterPointsX.Size = insert(x, ref rasterPointsX);
            }

            /* Construct the raster points for W. */
            for (i = ySize - 1; i >= 0; i--)
            {
                x = normalize[_params.W - _conicCombinations.Points[i]];
                rasterPointsY.Size = insert(x, ref rasterPointsY);
            }
        }

        /**
         * Construct the set X of integer conic combinations of l and w.
         *
         * X = {x | x = rl + sw, x <= L, r,s >= 0 integers}
         *
         * Parameters:
         * L - Length of the rectangle.
         * l - Length of the boxes to be packed.
         * w - Width of the boxes to be packed.
         * X - Pointer to the set X.
         */

        public Set ConstructConicCombinations()
        {
            var X = new Set(_params.L + 2);
            var inX = new int[_params.L + 2];
            var c = new int[_params.L + 2];

            X.Points[0] = 0;
            X.Size = 1;
            inX[0] = 1;

            for (int i = 0; i <= _params.L; i++)
            {
                c[i] = 0;
            }

            for (int i = _params.l; i <= _params.L; i++)
            {
                if (c[i] < c[i - _params.l] + _params.l)
                {
                    c[i] = c[i - _params.l] + _params.l;
                }
            }

            for (int i = _params.w; i <= _params.L; i++)
            {
                if (c[i] < c[i - _params.w] + _params.w)
                {
                    c[i] = c[i - _params.w] + _params.w;
                }
            }

            for (int i = 1; i <= _params.L; i++)
            {
                if (c[i] == i && inX[i] == 0)
                {
                    X.Points[X.Size] = i;
                    X.Size++;
                    inX[i] = 1;
                }
            }

            if (X.Points[X.Size - 1] != _params.L)
            {
                /* Insert _params.L into the set X. */
                X.Points[X.Size] = _params.L;
                X.Size++;
            }

            _conicCombinations = X;
            return X;
        }
    }
}