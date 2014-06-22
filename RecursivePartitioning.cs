using AutoNav.Library.RecursivePartitioning.Model.Shared;

namespace AutoNav.Library.RecursivePartitioning
{
    using Model;
    using Draw;
    
    public class RecursivePartitioning
    {

        public int[][] Solve(int L, int W, int l, int w, out int numRects)
        {
            var p = new RecursivePartitioning();

            var parameters = new Parameters
            {
                L = L,
                W = W,
                l = l,
                w = w
            };
            var fiveBlockSolver = new Algorithm.FiveBlock();
            var fiveBlockSolverResult = fiveBlockSolver.Solve(parameters);

            var lblockSolver = new Algorithm.LBlock();

            var q = new int[4];
            q[0] = q[2] = fiveBlockSolver.SharedData.NormalizedL;
            q[1] = q[3] = fiveBlockSolver.SharedData.NormalizedW;

            var draw = new Driver();

            if (!fiveBlockSolver.IsSolutionOptimal())
            {
                /* The solution obtained by Five Block Algorithm is not known to be
                    * optimal. Then it will try to solve the problem with
                    * L-Algorithm. */

                lblockSolver.SharedData = new LBlockShared {FiveBlockData = fiveBlockSolver.SharedData};

                var idx = lblockSolver.SharedData.LIndex(q);
                lblockSolver.Solve(idx, q);
                var lblockSolverResult = lblockSolver.GetSolution(idx, q);

                draw.LBlockData = lblockSolver.SharedData;

                if (fiveBlockSolverResult < lblockSolverResult)
                {
                    draw.ProcessOutput(L, W, idx, q, lblockSolverResult & Constants.nRet, true);
                    numRects = lblockSolverResult;
                }
                else
                {
                    draw.ProcessOutput(L, W, 0, q, fiveBlockSolverResult, false);
                    numRects = fiveBlockSolverResult;
                }
            }
            else
            {
                draw.LBlockData = new LBlockShared
                {
                    FiveBlockData = fiveBlockSolver.SharedData
                };
                draw.ProcessOutput(L, W, 0, q, fiveBlockSolverResult, false);
                numRects = fiveBlockSolverResult;
            }

            return draw.Rectangles;
        }
    }
}