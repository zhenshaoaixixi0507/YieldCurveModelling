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
        public Matrix<double> statetransition { get; set; }
        public Matrix<double> statenoisecovariance { get; set; }
        public Matrix<double> observationmodel { get; set; }
        public Matrix<double> observationnoisecov { get; set; }
        public Matrix<double> observation { get; set; }
        public Dictionary<int, Matrix<double>> FilterStates()
        {
            var results = new Dictionary<int, Matrix<double>>();

            return results;
        }
    }
}
