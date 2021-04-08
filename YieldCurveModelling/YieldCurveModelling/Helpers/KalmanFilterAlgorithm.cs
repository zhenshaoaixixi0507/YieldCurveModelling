using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace YieldCurveModelling.Helpers
{
    public class KalmanFilterAlgorithm
    {
        public Matrix<double> initialstate { get; set; }
        public Matrix<double> initialstatecovariance { get; set; }
        public Matrix<double> statetransition { get; set; }
        public Matrix<double> statenoisecovariance { get; set; }
        public Matrix<double> observationmodel { get; set; }
        public Matrix<double> observationnoisecov { get; set; }
        public Matrix<double> observation { get; set; }
        public bool calculateloglikelihood { get; set; }
        
        public (Dictionary<int, Matrix<double>>,double )FilterStates()
        {
            var results = new Dictionary<int, Matrix<double>>();
            var M = Matrix<double>.Build;
            var I = M.DenseDiagonal(initialstate.RowCount, initialstate.RowCount, 1.00);
            double loglikelihood = 0;
            for (int i = 0; i < observation.RowCount; i++)
            {
                var priorstate = statetransition.Multiply(initialstate);
                var priorcov = statetransition.Multiply(initialstatecovariance).Multiply(statetransition.Transpose()).Add(statenoisecovariance);
                var tempobs = GetObservationAtSingleTimePoint(i);
                var innovation = tempobs.Subtract(observationmodel.Multiply(priorstate));
                var innovationcov = observationmodel.Multiply(priorcov).Multiply(observationmodel.Transpose()).Add(observationnoisecov);
                var kalmangain = priorcov.Multiply(observationmodel.Transpose()).Multiply(innovationcov.Inverse());
                var posterioristate = priorstate.Add(kalmangain.Multiply(innovation));
                results.Add(i, posterioristate.Clone());
                initialstate = posterioristate.Clone();
                initialstatecovariance = I.Subtract(kalmangain.Multiply(observationmodel)).Multiply(priorcov);
                if (calculateloglikelihood == true)
                {
                    loglikelihood = loglikelihood + GetLogLiklihoodValueAtSingleTimePoint(innovationcov, innovation);
                }
            }

            return (results,loglikelihood);
        }

        private Matrix<double> GetObservationAtSingleTimePoint(int index)
        {
            var M = Matrix<double>.Build;
            var result = M.Dense(observation.ColumnCount, 1);
            for (int i = 0; i < result.RowCount; i++)
            {
                result[i,0] = observation[index, i];
            }

            return result;
        }
        private double GetLogLiklihoodValueAtSingleTimePoint(Matrix<double> innovationcov,Matrix<double>innovation)
        {
            return -0.5 * Math.Log(innovationcov.Determinant())-0.5*innovation.Transpose().Multiply(innovationcov.Inverse()).Multiply(innovation)[0,0];
        }
    }
}
