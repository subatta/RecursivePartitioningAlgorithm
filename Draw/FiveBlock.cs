
namespace AutoNav.Library.RecursivePartitioning.Draw
{
    using Model;
    using Model.Shared;

    internal class FiveBlock
    {
        private int boxesDrawn = 0;

        public LBlockShared LBlockData { get; set; }
        public int[][] Rectangles { get; set; }

        private void draw(int L, int W, int dx, int dy)
        {
            if (L >= W)
            {
                drawNormal(L, W, dx, dy);
            }
            else
            {
                drawRotation(L, W, dx, dy);
            }
        }

        private void drawNormal(int L, int W, int dx, int dy)
        {
            int i;
            var L_ = new int[6];
            var W_ = new int[6];

            int iX = LBlockData.FiveBlockData.IndexX[L];
            int iY = LBlockData.FiveBlockData.IndexY[W];

            if (LBlockData.FiveBlockData.CutPoints[iX][iY].Homogeneous > 0)
            {
                drawHomogeneous(L, W, dx, dy);
                return;
            }

            getSubproblems(LBlockData.FiveBlockData.CutPoints[iX][iY], L_, W_, L, W);

            for (i = 1; i <= 5; i++)
            {
                if (L_[i] != 0 && W_[i] != 0 && !((L_[i] == L && W_[i] == W) || (L_[i] == W && W_[i] == L)))
                {
                    switch (i)
                    {
                        case 1:
                            draw(L_[1], W_[1], dx, dy + W_[4]);
                            break;
                        case 2:
                            draw(L_[2], W_[2], dx + L_[1], dy + W_[5]);
                            break;
                        case 3:
                            draw(L_[3], W_[3], dx + L_[1], dy + W_[4]);
                            break;
                        case 4:
                            draw(L_[4], W_[4], dx, dy);
                            break;
                        case 5:
                            draw(L_[5], W_[5], dx + L_[4], dy);
                            break;
                    }
                }
            }
        }

        private void drawRotation(int L, int W, int dx, int dy)
        {
            int[] L_ = new int[6];
            int[] W_ = new int[6];
            int i;
            int iX;
            int iY;

            Util.Swap(ref L, ref W);

            iX = LBlockData.FiveBlockData.IndexX[L];
            iY = LBlockData.FiveBlockData.IndexY[W];

            if (LBlockData.FiveBlockData.CutPoints[iX][iY].Homogeneous > 0)
            {
                Util.Swap(ref L, ref W);
                drawHomogeneous(L, W, dx, dy);
                return;
            }

            getSubproblems(LBlockData.FiveBlockData.CutPoints[iX][iY], L_, W_, L, W);

            for (i = 1; i <= 5; i++)
            {
                Util.Swap(ref L_[i], ref W_[i]);
                if (L_[i] != 0 && W_[i] != 0 && !((L_[i] == L && W_[i] == W) || (L_[i] == W && W_[i] == L)))
                {
                    switch (i)
                    {
                        case 1:
                            draw(L_[1], W_[1], dx + W_[4], dy + L_[2]);
                            break;
                        case 2:
                            draw(L_[2], W_[2], dx + W_[5], dy);
                            break;
                        case 3:
                            draw(L_[3], W_[3], dx + W_[4], dy + L_[5]);
                            break;
                        case 4:
                            draw(L_[4], W_[4], dx, dy + L_[5]);
                            break;
                        case 5:
                            draw(L_[5], W_[5], dx, dy);
                            break;
                    }
                }
                Util.Swap(ref L_[i], ref W_[i]);
            }
        }

        public int Draw(int L, int W, int ret)
        {
            boxesDrawn = ret;
            draw(L, W, 0, 0);
            return boxesDrawn;
        }

        private void drawHomogeneous(int x, int y, int dx, int dy)
        {
            int i, j;
            int corte = boxOrientation(x, y);

            if (corte == Constants.HORIZONTAL)
            {
                for (i = 0; i + LBlockData.FiveBlockData.Parameters.l <= x; i += LBlockData.FiveBlockData.Parameters.l)
                {
                    for (j = 0;
                        j + LBlockData.FiveBlockData.Parameters.w <= y;
                        j += LBlockData.FiveBlockData.Parameters.w)
                    {
                        Rectangles[boxesDrawn][0] = i + dx;
                        Rectangles[boxesDrawn][1] = j + dy;
                        Rectangles[boxesDrawn][2] = i + LBlockData.FiveBlockData.Parameters.l + dx;
                        Rectangles[boxesDrawn][3] = j + LBlockData.FiveBlockData.Parameters.w + dy;
                        boxesDrawn++;
                    }
                }
            }

            else
            {
                for (i = 0; i + LBlockData.FiveBlockData.Parameters.w <= x; i += LBlockData.FiveBlockData.Parameters.w)
                {
                    for (j = 0;
                        j + LBlockData.FiveBlockData.Parameters.l <= y;
                        j += LBlockData.FiveBlockData.Parameters.l)
                    {
                        Rectangles[boxesDrawn][0] = i + dx;
                        Rectangles[boxesDrawn][1] = j + dy;
                        Rectangles[boxesDrawn][2] = i + LBlockData.FiveBlockData.Parameters.w + dx;
                        Rectangles[boxesDrawn][3] = j + LBlockData.FiveBlockData.Parameters.l + dy;
                        boxesDrawn++;
                    }
                }
            }
        }

        /**
         * Determine the orientation of the boxes (l,w) that maximizes the
         * homogeneous packing in (x,y).
         *
         * Parameters:
         * x - Length of the rectangle.
         *
         * y - Width of the rectangle.
         */

        private int boxOrientation(int x, int y)
        {
            int a = (x/LBlockData.FiveBlockData.Parameters.l)*(y/LBlockData.FiveBlockData.Parameters.w);
            int b = (x/LBlockData.FiveBlockData.Parameters.w)*(y/LBlockData.FiveBlockData.Parameters.l);
            return (a > b) ? Constants.HORIZONTAL : Constants.VERTICAL;
        }

        private void getSubproblems(CutPoint cutPoint, int[] L_, int[] W_, int L, int W)
        {
            int x1 = cutPoint.X1;
            int x2 = cutPoint.X2;
            int y1 = cutPoint.Y1;
            int y2 = cutPoint.Y2;

            L_[1] = x1;
            W_[1] = LBlockData.FiveBlockData.Normalized[W - y1];
            L_[2] = LBlockData.FiveBlockData.Normalized[L - x1];
            W_[2] = LBlockData.FiveBlockData.Normalized[W - y2];
            L_[3] = LBlockData.FiveBlockData.Normalized[x2 - x1];
            W_[3] = LBlockData.FiveBlockData.Normalized[y2 - y1];
            L_[4] = x2;
            W_[4] = y1;
            L_[5] = LBlockData.FiveBlockData.Normalized[L - x2];
            W_[5] = y2;
        }
    }
}