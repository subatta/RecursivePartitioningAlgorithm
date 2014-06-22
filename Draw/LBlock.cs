using System;

namespace AutoNav.Library.RecursivePartitioning.Draw
{
    using Model;
    using Model.Shared;

    internal class LBlock
    {
        private int retVal;
        public LBlockShared LBlockData { get; set; }
        public int[][] Rectangles { get; set; }

        public FiveBlock FiveBlock { get; set; }

        public void Draw(int L, int[] q)
        {
            int i;
            int start;
            int end;
            int divisionType;

            if (LBlockData.MemoryType == Constants.MEM_TYPE_4)
            {
                divisionType = (LBlockData.Solution[L] & Constants.SOLUTION) >> Constants.descSol;
            }
            else
            {
                int h = LBlockData.GetKey(q);
                divisionType = (LBlockData.SolutionMap[L][h] & Constants.SOLUTION) >> Constants.descSol;
            }

            switch (divisionType)
            {
                case Constants.HOMOGENEOUS:

                    /* Non-degenerated L. */
                    if (q[0] != q[2])
                    {
                        int cut = LBlockData.LCut(q);

                        if (cut == Constants.VERTICAL_CUT)
                        {
                            retVal = FiveBlock.Draw(q[2], q[1], retVal);
                            start = retVal;
                            retVal = FiveBlock.Draw(LBlockData.FiveBlockData.Normalized[q[0] - q[2]], q[3], retVal);
                            end = retVal;
                            for (i = start; i < end; i++)
                            {
                                shiftX(i, q[2]);
                            }
                        }
                        else
                        {
                            start = retVal;
                            retVal = FiveBlock.Draw(q[2], LBlockData.FiveBlockData.Normalized[q[1] - q[3]], retVal);
                            end = retVal;
                            for (i = start; i < end; i++)
                            {
                                shiftY(i, q[3]);
                            }
                            retVal = FiveBlock.Draw(q[0], q[3], retVal);
                        }
                    }
                        /* Degenerated L (rectangle). */
                    else
                    {
                        retVal = FiveBlock.Draw(q[0], q[1], retVal);
                    }
                    break;

                case Constants.B1:
                    drawB1(L, q);
                    break;
                case Constants.B2:
                    drawB2(L, q);
                    break;
                case Constants.B3:
                    drawB3(L, q);
                    break;
                case Constants.B4:
                    drawB4(L, q);
                    break;
                case Constants.B5:
                    drawB5(L, q);
                    break;
                case Constants.B6:
                    drawB6(L, q);
                    break;
                case Constants.B7:
                    drawB7(L, q);
                    break;
                case Constants.B8:
                    drawB8(L, q);
                    break;
                case Constants.B9:
                    drawB9(L, q);
                    break;
                default:
                    Console.Write("Invalid division: {0:D}\n", divisionType);
                    Environment.Exit(0);
                    break;
            }
        }

        /**
         * Shift the rectangle in the x-axis.
         *
         * Parameters:
         * id     - Identifier of the rectangle.
         *
         * deltaX - Amount to be shifted.
         */

        private void shiftX(int id, int deltaX)
        {
            Rectangles[id][0] += deltaX;
            Rectangles[id][2] += deltaX;
        }

        /**
         * Shift the rectangle in the y-axis.
         *
         * Parameters:
         * id     - Identifier of the rectangle.
         *
         * deltaY - Amount to be shifted.
         */

        private void shiftY(int id, int deltaY)
        {
            Rectangles[id][1] += deltaY;
            Rectangles[id][3] += deltaY;
        }

        /**
         * Normalize degenerated L-pieces.
         *
         * Parameter:
         * q - The degenerated L-piece to be normalized.
         */

        private void normalizeDegeneratedL(int[] q)
        {
            if (q[2] == 0)
            {
                q[2] = q[0];
                q[1] = q[3];
            }
            else if (q[3] == 0)
            {
                q[3] = q[1];
                q[0] = q[2];
            }
            else if (q[2] == q[0] || q[3] == q[1])
            {
                q[2] = q[0];
                q[3] = q[1];
            }
        }

        /**
         * Draw the boxes according to the B1 subdivision.
         */

        private void drawB1(int L, int[] q)
        {
            int L1;
            int L2;
            var q1 = new int[4];
            var q2 = new int[4];
            var tmp = new int[4];
            int width;
            int height;
            int deltaX;
            int deltaY;
            int start;
            int end;
            var div = new int[2];

            if (LBlockData.MemoryType == Constants.MEM_TYPE_4)
            {
                div[0] = LBlockData.DivisionPoint[L] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPoint[L] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }
            else
            {
                int h = LBlockData.GetKey(q);
                div[0] = LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }

            LBlockData.StandardPositionB1(ref div, ref q, ref q1, ref q2);

            /* Draw L1. */
            tmp[0] = q1[0];
            tmp[1] = q1[1];
            tmp[2] = q1[2];
            tmp[3] = q1[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q1);
            L1 = LBlockData.LIndex(q1);

            start = retVal;
            Draw(L1, q1);
            end = retVal;

            deltaX = 0;
            deltaY = div[1];
            if (div[0] == 0)
            {
                deltaY = q[3];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (tmp[0] == tmp[2])
            {
                if (width >= height)
                {
                    P4(start, end, L1, deltaX, deltaY);
                }
                else
                {
                    P8(start, end, L1, deltaX, deltaY);
                }
            }
            else
            {
                if (width >= height)
                {
                    P1(start, end, L1, q1, deltaX, deltaY);
                }
                else
                {
                    P5(start, end, L1, q1, deltaX, deltaY);
                }
            }

            /* Draw L2. */
            tmp[0] = q2[0];
            tmp[1] = q2[1];
            tmp[2] = q2[2];
            tmp[3] = q2[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q2);
            L2 = LBlockData.LIndex(q2);

            start = retVal;
            Draw(L2, q2);
            end = retVal;

            deltaX = 0;
            deltaY = 0;
            if (div[1] == 0)
            {
                deltaX = div[0];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (tmp[0] == tmp[2])
            {
                if (width >= height)
                {
                    P4(start, end, L2, deltaX, deltaY);
                }
                else
                {
                    P8(start, end, L2, deltaX, deltaY);
                }
            }
            else
            {
                if (width >= height)
                {
                    P2(start, end, L2, q2, deltaX, deltaY);
                }
                else
                {
                    P6(start, end, L2, q2, deltaX, deltaY);
                }
            }
        }

        /**
         * Draw the boxes according to the B2 subdivision.
         */

        private void drawB2(int L, int[] q)
        {
            int L1;
            int L2;
            var q1 = new int[4];
            var q2 = new int[4];
            var tmp = new int[4];
            int width;
            int height;
            int deltaX;
            int deltaY;
            int start;
            int end;
            var div = new int[3];

            if (LBlockData.MemoryType == Constants.MEM_TYPE_4)
            {
                div[0] = LBlockData.DivisionPoint[L] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPoint[L] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }
            else
            {
                int h = LBlockData.GetKey(q);
                div[0] = LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }

            LBlockData.StandardPositionB2(ref div, ref q, ref q1, ref q2);

            /* Draw L1. */
            tmp[0] = q1[0];
            tmp[1] = q1[1];
            tmp[2] = q1[2];
            tmp[3] = q1[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q1);
            L1 = LBlockData.LIndex(q1);

            start = retVal;
            Draw(L1, q1);
            end = retVal;

            deltaX = 0;
            deltaY = q[3];
            if (div[1] == q[1])
            {
                deltaX = div[0];
            }
            else if (tmp[0] == tmp[2])
            {
                deltaY = div[1];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (tmp[0] == tmp[2])
            {
                if (width >= height)
                {
                    P4(start, end, L1, deltaX, deltaY);
                }
                else
                {
                    P8(start, end, L1, deltaX, deltaY);
                }
            }
            else
            {
                if (width >= height)
                {
                    P3(start, end, L1, q1, deltaX, deltaY);
                }
                else
                {
                    P7(start, end, L1, q1, deltaX, deltaY);
                }
            }

            /* Draw L2. */
            tmp[0] = q2[0];
            tmp[1] = q2[1];
            tmp[2] = q2[2];
            tmp[3] = q2[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q2);
            L2 = LBlockData.LIndex(q2);

            start = retVal;
            Draw(L2, q2);
            end = retVal;

            deltaX = 0;
            deltaY = 0;

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (width >= height)
            {
                P4(start, end, L2, deltaX, deltaY);
            }
            else
            {
                P8(start, end, L2, deltaX, deltaY);
            }
        }

        /**
         * Draw the boxes according to the B3 subdivision.
         */

        private void drawB3(int L, int[] q)
        {
            int L1;
            int L2;
            var q1 = new int[4];
            var q2 = new int[4];
            var tmp = new int[4];
            int width;
            int height;
            int deltaX;
            int deltaY;
            int start;
            int end;
            var div = new int[2];

            if (LBlockData.MemoryType == Constants.MEM_TYPE_4)
            {
                div[0] = LBlockData.DivisionPoint[L] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPoint[L] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }
            else
            {
                int h = LBlockData.GetKey(q);
                div[0] = LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }

            LBlockData.StandardPositionB3(ref div, ref q, ref q1, ref q2);

            /* Draw L1. */
            tmp[0] = q1[0];
            tmp[1] = q1[1];
            tmp[2] = q1[2];
            tmp[3] = q1[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q1);
            L1 = LBlockData.LIndex(q1);

            start = retVal;
            Draw(L1, q1);
            end = retVal;

            deltaX = 0;
            deltaY = 0;

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (width >= height)
            {
                P4(start, end, L1, deltaX, deltaY);
            }
            else
            {
                P8(start, end, L1, deltaX, deltaY);
            }

            /* Draw L2. */
            tmp[0] = q2[0];
            tmp[1] = q2[1];
            tmp[2] = q2[2];
            tmp[3] = q2[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q2);
            L2 = LBlockData.LIndex(q2);

            start = retVal;
            Draw(L2, q2);
            end = retVal;

            deltaX = div[0];
            deltaY = div[1];

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (width >= height)
            {
                P4(start, end, L2, deltaX, deltaY);
            }
            else
            {
                P8(start, end, L2, deltaX, deltaY);
            }
        }

        /**
         * Draw the boxes according to the B4 subdivision.
         */

        private void drawB4(int L, int[] q)
        {
            int L1;
            int L2;
            var q1 = new int[4];
            var q2 = new int[4];
            var tmp = new int[4];
            int width;
            int height;
            int deltaX;
            int deltaY;
            int start;
            int end;
            var div = new int[2];

            if (LBlockData.MemoryType == Constants.MEM_TYPE_4)
            {
                div[0] = LBlockData.DivisionPoint[L] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPoint[L] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }
            else
            {
                int h = LBlockData.GetKey(q);
                div[0] = LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }

            LBlockData.StandardPositionB4(ref div, ref q, ref q1, ref q2);

            /* Draw L1. */
            tmp[0] = q1[0];
            tmp[1] = q1[1];
            tmp[2] = q1[2];
            tmp[3] = q1[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q1);
            L1 = LBlockData.LIndex(q1);

            start = retVal;
            Draw(L1, q1);
            end = retVal;

            deltaX = 0;
            deltaY = 0;

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (width >= height)
            {
                P4(start, end, L1, deltaX, deltaY);
            }
            else
            {
                P8(start, end, L1, deltaX, deltaY);
            }

            /* Draw L2. */
            tmp[0] = q2[0];
            tmp[1] = q2[1];
            tmp[2] = q2[2];
            tmp[3] = q2[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q2);
            L2 = LBlockData.LIndex(q2);

            start = retVal;
            Draw(L2, q2);
            end = retVal;

            deltaX = q[3];
            deltaY = 0;
            if (div[0] == q[0])
            {
                deltaY = div[1];
            }
            else if (tmp[0] == tmp[2])
            {
                deltaX = div[0];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (tmp[0] == tmp[2])
            {
                if (width >= height)
                {
                    P4(start, end, L2, deltaX, deltaY);
                }
                else
                {
                    P8(start, end, L2, deltaX, deltaY);
                }
            }
            else
            {
                if (width > height)
                {
                    P3(start, end, L2, q2, deltaX, deltaY);
                }
                else
                {
                    P7(start, end, L2, q2, deltaX, deltaY);
                }
            }
        }

        /**
         * Draw the boxes according to the B5 subdivision.
         */

        private void drawB5(int L, int[] q)
        {
            int L1;
            int L2;
            var q1 = new int[4];
            var q2 = new int[4];
            var tmp = new int[4];
            int width;
            int height;
            int deltaX;
            int deltaY;
            int start;
            int end;
            var div = new int[2];

            if (LBlockData.MemoryType == Constants.MEM_TYPE_4)
            {
                div[0] = LBlockData.DivisionPoint[L] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPoint[L] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }
            else
            {
                int h = LBlockData.GetKey(q);
                div[0] = LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }

            LBlockData.StandardPositionB5(ref div, ref q, ref q1, ref q2);

            /* Draw L1. */
            tmp[0] = q1[0];
            tmp[1] = q1[1];
            tmp[2] = q1[2];
            tmp[3] = q1[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q1);
            L1 = LBlockData.LIndex(q1);

            start = retVal;
            Draw(L1, q1);
            end = retVal;

            deltaX = 0;
            deltaY = 0;
            if (div[0] == 0)
            {
                deltaY = div[1];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (tmp[0] == tmp[2])
            {
                if (width >= height)
                {
                    P4(start, end, L1, deltaX, deltaY);
                }
                else
                {
                    P8(start, end, L1, deltaX, deltaY);
                }
            }
            else
            {
                if (width >= height)
                {
                    P1(start, end, L1, q1, deltaX, deltaY);
                }
                else
                {
                    P5(start, end, L1, q1, deltaX, deltaY);
                }
            }

            /* Draw L2. */
            tmp[0] = q2[0];
            tmp[1] = q2[1];
            tmp[2] = q2[2];
            tmp[3] = q2[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q2);
            L2 = LBlockData.LIndex(q2);

            start = retVal;
            Draw(L2, q2);
            end = retVal;

            deltaX = div[0];
            deltaY = 0;
            if (div[1] == 0)
            {
                deltaX = q[2];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (tmp[0] == tmp[2])
            {
                if (width >= height)
                {
                    P4(start, end, L2, deltaX, deltaY);
                }
                else
                {
                    P8(start, end, L2, deltaX, deltaY);
                }
            }
            else
            {
                if (width >= height)
                {
                    P2(start, end, L2, q2, deltaX, deltaY);
                }
                else
                {
                    P6(start, end, L2, q2, deltaX, deltaY);
                }
            }
        }

        /**
         * Draw the boxes according to the B6 subdivision.
         */

        private void drawB6(int L, int[] q)
        {
            int L1;
            int L2;
            var q1 = new int[4];
            var q2 = new int[4];
            var tmp = new int[4];
            int width;
            int height;
            int deltaX;
            int deltaY;
            int start;
            int end;
            var div = new int[3];

            if (LBlockData.MemoryType == Constants.MEM_TYPE_4)
            {
                div[0] = LBlockData.DivisionPoint[L] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPoint[L] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
                div[2] = (int) (LBlockData.DivisionPoint[L] & Constants.ptoDiv3) >> Constants.descPtoDiv3;
            }
            else
            {
                int h = LBlockData.GetKey(q);
                div[0] = LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
                div[2] = (int) (LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv3) >> Constants.descPtoDiv3;
            }

            LBlockData.StandardPositionB6(ref div, ref q, ref q1, ref q2);

            /* Draw L1. */
            tmp[0] = q1[0];
            tmp[1] = q1[1];
            tmp[2] = q1[2];
            tmp[3] = q1[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q1);
            L1 = LBlockData.LIndex(q1);

            start = retVal;
            Draw(L1, q1);
            end = retVal;

            deltaX = 0;
            deltaY = 0;
            if (div[0] == 0)
            {
                deltaY = div[1];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (tmp[0] == tmp[2])
            {
                if (width >= height)
                {
                    P4(start, end, L1, deltaX, deltaY);
                }
                else
                {
                    P8(start, end, L1, deltaX, deltaY);
                }
            }
            else
            {
                if (width >= height)
                {
                    P1(start, end, L1, q1, deltaX, deltaY);
                }
                else
                {
                    P5(start, end, L1, q1, deltaX, deltaY);
                }
            }

            /*Draw L2. */
            tmp[0] = q2[0];
            tmp[1] = q2[1];
            tmp[2] = q2[2];
            tmp[3] = q2[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q2);
            L2 = LBlockData.LIndex(q2);

            start = retVal;
            Draw(L2, q2);
            end = retVal;

            deltaX = div[0];
            deltaY = 0;
            if (div[1] == 0)
            {
                deltaX = div[2];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (tmp[0] == tmp[2])
            {
                if (width >= height)
                {
                    P4(start, end, L2, deltaX, deltaY);
                }
                else
                {
                    P8(start, end, L2, deltaX, deltaY);
                }
            }
            else
            {
                if (width >= height)
                {
                    P2(start, end, L2, q2, deltaX, deltaY);
                }
                else
                {
                    P6(start, end, L2, q2, deltaX, deltaY);
                }
            }
        }

        /**
         * Draw the boxes according to the B7 subdivision.
         */

        private void drawB7(int L, int[] q)
        {
            int L1;
            int L2;
            var q1 = new int[4];
            var q2 = new int[4];
            var tmp = new int[4];
            int width;
            int height;
            int deltaX;
            int deltaY;
            int start;
            int end;
            var div = new int[3];


            if (LBlockData.MemoryType == Constants.MEM_TYPE_4)
            {
                div[0] = LBlockData.DivisionPoint[L] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPoint[L] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
                div[2] = (int) (LBlockData.DivisionPoint[L] & Constants.ptoDiv3) >> Constants.descPtoDiv3;
            }
            else
            {
                int h = LBlockData.GetKey(q);
                div[0] = LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
                div[2] = (int) (LBlockData.DivisionPointMap[L][h] & Constants.ptoDiv3) >> Constants.descPtoDiv3;
            }

            LBlockData.StandardPositionB7(ref div, ref q, ref q1, ref q2);

            /* Draw L1. */
            tmp[0] = q1[0];
            tmp[1] = q1[1];
            tmp[2] = q1[2];
            tmp[3] = q1[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q1);
            L1 = LBlockData.LIndex(q1);

            start = retVal;
            Draw(L1, q1);
            end = retVal;

            deltaX = 0;
            deltaY = div[1];
            if (div[0] == 0)
            {
                deltaY = div[2];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (tmp[0] == tmp[2])
            {
                if (width >= height)
                {
                    P4(start, end, L1, deltaX, deltaY);
                }
                else
                {
                    P8(start, end, L1, deltaX, deltaY);
                }
            }
            else
            {
                if (width >= height)
                {
                    P1(start, end, L1, q1, deltaX, deltaY);
                }
                else
                {
                    P5(start, end, L1, q1, deltaX, deltaY);
                }
            }

            /* Draw L2. */
            tmp[0] = q2[0];
            tmp[1] = q2[1];
            tmp[2] = q2[2];
            tmp[3] = q2[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q2);
            L2 = LBlockData.LIndex(q2);

            start = retVal;
            Draw(L2, q2);
            end = retVal;

            deltaX = 0;
            deltaY = 0;
            if (div[1] == 0)
            {
                deltaX = div[0];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (tmp[0] == tmp[2])
            {
                if (width >= height)
                {
                    P4(start, end, L2, deltaX, deltaY);
                }
                else
                {
                    P8(start, end, L2, deltaX, deltaY);
                }
            }
            else
            {
                if (width >= height)
                {
                    P2(start, end, L2, q2, deltaX, deltaY);
                }
                else
                {
                    P6(start, end, L2, q2, deltaX, deltaY);
                }
            }
        }

        /**
         * Draw the boxes according to the B8 subdivision.
         */

        private void drawB8(int L_index, int[] q)
        {
            int L1;
            int L2;
            var q1 = new int[4];
            var q2 = new int[4];
            var tmp = new int[4];
            int width;
            int height;
            int deltaX;
            int deltaY;
            int start;
            int end;
            var div = new int[2];

            if (LBlockData.MemoryType == Constants.MEM_TYPE_4)
            {
                div[0] = LBlockData.DivisionPoint[L_index] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPoint[L_index] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }
            else
            {
                int h = LBlockData.GetKey(q);
                div[0] = LBlockData.DivisionPointMap[L_index][h] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPointMap[L_index][h] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }

            LBlockData.StandardPositionB8(ref div, ref q, ref q1, ref q2);

            /* Draw L1. */
            tmp[0] = q1[0];
            tmp[1] = q1[1];
            tmp[2] = q1[2];
            tmp[3] = q1[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q1);
            L1 = LBlockData.LIndex(q1);

            start = retVal;
            Draw(L1, q1);
            end = retVal;

            deltaX = 0;
            deltaY = 0;
            if (div[0] == 0)
            {
                deltaY = div[1];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (tmp[0] == tmp[2])
            {
                if (width >= height)
                {
                    P4(start, end, L1, deltaX, deltaY);
                }
                else
                {
                    P8(start, end, L1, deltaX, deltaY);
                }
            }
            else
            {
                if (width >= height)
                {
                    P1(start, end, L1, q1, deltaX, deltaY);
                }
                else
                {
                    P5(start, end, L1, q1, deltaX, deltaY);
                }
            }

            /* Draw L2. */
            tmp[0] = q2[0];
            tmp[1] = q2[1];
            tmp[2] = q2[2];
            tmp[3] = q2[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q2);
            L2 = LBlockData.LIndex(q2);

            start = retVal;
            Draw(L2, q2);
            end = retVal;

            deltaX = div[0];
            deltaY = 0;
            if (div[1] == 0)
            {
                deltaX = div[0];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (width >= height)
            {
                P4(start, end, L2, deltaX, deltaY);
            }
            else
            {
                P8(start, end, L2, deltaX, deltaY);
            }
        }

        /**
         * Draw the boxes according to the B9 subdivision.
         */

        private void drawB9(int L_index, int[] q)
        {
            int L1;
            int L2;
            var q1 = new int[4];
            var q2 = new int[4];
            var tmp = new int[4];
            int width;
            int height;
            int deltaX;
            int deltaY;
            int start;
            int end;
            var div = new int[2];

            if (LBlockData.MemoryType == Constants.MEM_TYPE_4)
            {
                div[0] = LBlockData.DivisionPoint[L_index] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPoint[L_index] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }
            else
            {
                int h = LBlockData.GetKey(q);
                div[0] = LBlockData.DivisionPointMap[L_index][h] & Constants.ptoDiv1;
                div[1] = (LBlockData.DivisionPointMap[L_index][h] & Constants.ptoDiv2) >> Constants.descPtoDiv2;
            }

            LBlockData.StandardPositionB9(ref div, ref q, ref q1, ref q2);

            /* Draw L1. */
            tmp[0] = q1[0];
            tmp[1] = q1[1];
            tmp[2] = q1[2];
            tmp[3] = q1[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q1);
            L1 = LBlockData.LIndex(q1);

            start = retVal;
            Draw(L1, q1);
            end = retVal;

            deltaX = 0;
            deltaY = div[1];
            if (div[0] == 0)
            {
                deltaY = q[3];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (width >= height)
            {
                P4(start, end, L1, deltaX, deltaY);
            }
            else
            {
                P8(start, end, L1, deltaX, deltaY);
            }

            /* Draw L2. */
            tmp[0] = q2[0];
            tmp[1] = q2[1];
            tmp[2] = q2[2];
            tmp[3] = q2[3];
            normalizeDegeneratedL(tmp);

            LBlockData.NormalizePiece(q2);
            L2 = LBlockData.LIndex(q2);

            start = retVal;
            Draw(L2, q2);
            end = retVal;

            deltaX = 0;
            deltaY = 0;
            if (div[1] == 0)
            {
                deltaX = div[0];
            }

            if (tmp[0] != tmp[1])
            {
                width = tmp[0];
                height = tmp[1];
            }
            else
            {
                width = tmp[2];
                height = tmp[3];
            }

            if (tmp[0] == tmp[2])
            {
                if (width >= height)
                {
                    P4(start, end, L2, deltaX, deltaY);
                }
                else
                {
                    P8(start, end, L2, deltaX, deltaY);
                }
            }
            else
            {
                if (width >= height)
                {
                    P2(start, end, L2, q2, deltaX, deltaY);
                }
                else
                {
                    P6(start, end, L2, q2, deltaX, deltaY);
                }
            }
        }

        /**
         * Fix the coordinates of the rectangle with the specified identifier.
         *
         * Parameter:
         * id - Identifier of the rectangle.
         */

        private void fixCoordinates(int id)
        {
            if (Rectangles[id][0] > Rectangles[id][2])
            {
                Util.Swap(ref Rectangles[id][0], ref Rectangles[id][2]);
            }
            if (Rectangles[id][1] > Rectangles[id][3])
            {
                Util.Swap(ref Rectangles[id][1], ref Rectangles[id][3]);
            }
        }

        private void P1(int start, int end, int L, int[] q, int deltaX, int deltaY)
        {
            for (int i = start; i < end; i++)
            {
                Rectangles[i][1] = q[1] - Rectangles[i][1];
                Rectangles[i][3] = q[1] - Rectangles[i][3];

                fixCoordinates(i);

                shiftX(i, deltaX);
                shiftY(i, deltaY);
            }
        }

        private void P2(int start, int end, int L, int[] q, int deltaX, int deltaY)
        {
            for (int i = start; i < end; i++)
            {
                Rectangles[i][0] = q[0] - Rectangles[i][0];
                Rectangles[i][2] = q[0] - Rectangles[i][2];

                fixCoordinates(i);

                shiftX(i, deltaX);
                shiftY(i, deltaY);
            }
        }

        private void P3(int start, int end, int L, int[] q, int deltaX, int deltaY)
        {
            for (int i = start; i < end; i++)
            {
                Rectangles[i][0] = q[0] - Rectangles[i][0];
                Rectangles[i][2] = q[0] - Rectangles[i][2];

                Rectangles[i][1] = q[1] - Rectangles[i][1];
                Rectangles[i][3] = q[1] - Rectangles[i][3];

                fixCoordinates(i);

                shiftX(i, deltaX);
                shiftY(i, deltaY);
            }
        }

        private void P4(int start, int end, int L, int deltaX, int deltaY)
        {
            for (int i = start; i < end; i++)
            {
                shiftX(i, deltaX);
                shiftY(i, deltaY);
            }
        }

        private void P5(int start, int end, int L, int[] q, int deltaX, int deltaY)
        {
            for (int i = start; i < end; i++)
            {
                int tmp1 = Rectangles[i][1];
                int tmp2 = Rectangles[i][3];

                Rectangles[i][1] = q[0] - Rectangles[i][0];
                Rectangles[i][3] = q[0] - Rectangles[i][2];

                Rectangles[i][0] = tmp1;
                Rectangles[i][2] = tmp2;

                fixCoordinates(i);

                shiftX(i, deltaX);
                shiftY(i, deltaY);
            }
        }

        private void P6(int start, int end, int L, int[] q, int deltaX, int deltaY)
        {
            for (int i = start; i < end; i++)
            {
                int tmp1 = Rectangles[i][0];
                int tmp2 = Rectangles[i][2];

                Rectangles[i][0] = q[1] - Rectangles[i][1];
                Rectangles[i][2] = q[1] - Rectangles[i][3];

                Rectangles[i][1] = tmp1;
                Rectangles[i][3] = tmp2;

                fixCoordinates(i);

                shiftX(i, deltaX);
                shiftY(i, deltaY);
            }
        }

        private void P7(int start, int end, int L, int[] q, int deltaX, int deltaY)
        {
            for (int i = start; i < end; i++)
            {
                int tmp1 = q[0] - Rectangles[i][0];
                int tmp2 = q[0] - Rectangles[i][2];

                Rectangles[i][0] = q[1] - Rectangles[i][1];
                Rectangles[i][2] = q[1] - Rectangles[i][3];

                Rectangles[i][1] = tmp1;
                Rectangles[i][3] = tmp2;

                fixCoordinates(i);

                shiftX(i, deltaX);
                shiftY(i, deltaY);
            }
        }

        private void P8(int start, int end, int L, int deltaX, int deltaY)
        {
            for (int i = start; i < end; i++)
            {
                int tmp1 = Rectangles[i][0];
                int tmp2 = Rectangles[i][2];

                Rectangles[i][0] = Rectangles[i][1];
                Rectangles[i][2] = Rectangles[i][3];

                Rectangles[i][1] = tmp1;
                Rectangles[i][3] = tmp2;

                shiftX(i, deltaX);
                shiftY(i, deltaY);
            }
        }
    }
}