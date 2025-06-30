using Google.OrTools.Sat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Partias.Leftovers
{
    internal class CustomCpSolverSolutionCallback: CpSolverSolutionCallback
    {
        public CustomCpSolverSolutionCallback(int solution_limit)
        {
            solution_limit_ = solution_limit;
        }

        public override void OnSolutionCallback()
        {
            solution_count_++;
            if (solution_count_ >= solution_limit_)
            {
                Console.WriteLine(String.Format("Stopping search after {0} solutions", solution_limit_));
                StopSearch();
            }
        }

        public int SolutionCount()
        {
            return solution_count_;
        }

        private int solution_count_;
        private IntVar[] variables_;
        private int solution_limit_;
    }
}
