using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApproximateOptimization.Utils
{
    internal static class ParametersManagement
    {
        internal static double[][] GetDefaultSolutionRange(int dimension)
        {
            double[][] solutionRange = new double[dimension][];
            for (int i = 0; i < dimension; i++)
            {
                solutionRange[i] = new double[2];
                solutionRange[i][0] = 0;
                solutionRange[i][1] = 1;
            }
            return solutionRange;
        }

        internal static void ProcessStandarParametersForConstructor<T>(this T solutionFinderParams) where T : BaseSolutionFinderParams
        {
            if (solutionFinderParams == null)
            {
                throw new ArgumentNullException(nameof(solutionFinderParams));
            }
            if (solutionFinderParams.solutionRange == null)
            {
                solutionFinderParams.solutionRange = GetDefaultSolutionRange(solutionFinderParams.dimension);
            }
            solutionFinderParams.Validate();
        }
    }
}
