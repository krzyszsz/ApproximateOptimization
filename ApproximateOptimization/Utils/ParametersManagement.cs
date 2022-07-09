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

        internal static void ProcessStandardParametersForConstructor<T>(this T optimizerParams) where T : CoreOptimizerParams
        {
            if (optimizerParams == null)
            {
                throw new ArgumentNullException(nameof(optimizerParams));
            }
            if (optimizerParams.SolutionRange == null)
            {
                optimizerParams.SolutionRange = GetDefaultSolutionRange(optimizerParams.Dimension);
            }
            optimizerParams.Validate();
        }
    }
}
