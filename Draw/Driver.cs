using System.IO;
using System.Text;

namespace AutoNav.Library.RecursivePartitioning.Draw
{
    using Model.Shared;

    public class Driver
    {
        public LBlockShared LBlockData { get; set; }

        private void makeFile(int[] q, int n)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0} {1} {2}", n + 1, q[0], q[1]));
            sb.AppendLine(string.Format("0 0 {0} {1}", q[0], q[1]));

            for (int i = 0; i < n; i++)
            {
                sb.AppendLine(string.Format("{0} {1} {2} {3}", Rectangles[i][0], Rectangles[i][1], Rectangles[i][2],
                    Rectangles[i][3]));
            }

            File.WriteAllText("Solution.txt", sb.ToString());
        }

        public int[][] Rectangles { get; set; }

        private int _retVal;

        public void ProcessOutput(int Lo, int Wo, int L, int[] q, int n, bool solvedWithL)
        {
            Rectangles = new int[n][];
            for (int i = 0; i < n; i++)
            {
                Rectangles[i] = new int[4];
            }

            _retVal = 0;

            var drawFive = new FiveBlock
            {
                LBlockData = LBlockData,
                Rectangles = Rectangles
            };

            if (solvedWithL)
            {
                var drawL = new LBlock
                {
                    LBlockData = LBlockData,
                    Rectangles = Rectangles,
                    FiveBlock = drawFive
                };
                drawL.LBlockData = LBlockData;
                drawL.Draw(L, q);
                Rectangles = drawL.Rectangles;
            }
            else
            {
                _retVal = drawFive.Draw(q[0], q[1], _retVal);
                Rectangles = drawFive.Rectangles;
            }

            makeFile(q, n);
        }
    }
}