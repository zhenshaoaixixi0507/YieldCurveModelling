using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YieldCurveModelling.OptimizationAlgorithmLib;

namespace YieldCurveModelling.YieldCurveModels
{

    public class DynamicNS4FactorModelCalibration
    {
        public Dictionary<string, double[]> yields { get; set; }
        public double[] maturities { get; set; }
        private Matrix<double> initialstate;
        private Matrix<double> initialstatecovariance;
        public double[] Optimize()
        {
            Initialization();

            var lowerbound = new double[18];
            var upperbound = new double[18];
            var initialguess = new double[18];
            //Lambda 1
            lowerbound[0] = 0.0000001;
            upperbound[0] = 29.99;
            initialguess[0] = 0.5;
            //Lambda 2
            lowerbound[1] = 0.0000001;
            upperbound[1] = 29.99;
            initialguess[1] = 11.5;

            for (int i = 2; i < lowerbound.Length; i++)
            {
                lowerbound[i] = 0.0000001;
                upperbound[i] = 2.99;
                initialguess[i] = 0.01;
            }
            var PSO = new PSOOptimization();
            PSO.initialguess = initialguess;
            PSO.lowerbound = lowerbound;
            PSO.upperbound = upperbound;
            PSO.maximumiteration = 5000;
            PSO.numofswarms = 200;
            PSO.inertiaweightmax = 1.2;
            PSO.inertiaweightmin = 0.1;
            PSO.objectfun = DynamicNS4FactorModelObjfun;
            PSO.tolerance = 0.000000001;
            PSO.Vmax = 4;
            PSO.c1 = 2;
            PSO.c2 = 2;
            PSO.chi = 0.73;
            var optimizedp = PSO.Optimize();
            return optimizedp;
        }
        private void Initialization()
        {
            //Calculate initialstate and initialstatecovariance
            var M = Matrix<double>.Build;
            initialstate = M.Dense(4, 1);
            initialstatecovariance = M.DenseDiagonal(4, 4, 0.00);
            var tempyields = new double[maturities.Length];
            var StaticNS4factorCalibration = new StaticNS4FactorModelCalibration();
            StaticNS4factorCalibration.maturities = maturities;
            var tempbeta1 = new double[30];
            var tempbeta2 = new double[30];
            var tempbeta3 = new double[30];
            var tempbeta4 = new double[30];

            Parallel.For(0, 30, i =>
            {
                tempyields = yields[yields.ElementAt(i).Key].Clone() as double[];
                StaticNS4factorCalibration.yields = tempyields;
                var temppara = StaticNS4factorCalibration.Calibration();
                tempbeta1[i] = temppara[0];
                tempbeta2[i] = temppara[1];
                tempbeta3[i] = temppara[2];
                tempbeta4[i] = temppara[3];

            });
            initialstate[0, 0] = ArrayStatistics.Mean(tempbeta1);
            initialstate[1, 0] = ArrayStatistics.Mean(tempbeta2);
            initialstate[2, 0] = ArrayStatistics.Mean(tempbeta3);
            initialstate[3, 0] = ArrayStatistics.Mean(tempbeta4);

            initialstatecovariance[0, 0] = ArrayStatistics.Covariance(tempbeta1, tempbeta1);
            initialstatecovariance[0, 1] = ArrayStatistics.Covariance(tempbeta1, tempbeta2);
            initialstatecovariance[0, 2] = ArrayStatistics.Covariance(tempbeta1, tempbeta3);
            initialstatecovariance[0, 3] = ArrayStatistics.Covariance(tempbeta1, tempbeta4);

            initialstatecovariance[1, 0] = ArrayStatistics.Covariance(tempbeta2, tempbeta1);
            initialstatecovariance[1, 1] = ArrayStatistics.Covariance(tempbeta2, tempbeta2);
            initialstatecovariance[1, 2] = ArrayStatistics.Covariance(tempbeta2, tempbeta3);
            initialstatecovariance[1, 3] = ArrayStatistics.Covariance(tempbeta2, tempbeta4);

            initialstatecovariance[2, 0] = ArrayStatistics.Covariance(tempbeta3, tempbeta1);
            initialstatecovariance[2, 1] = ArrayStatistics.Covariance(tempbeta3, tempbeta2);
            initialstatecovariance[2, 2] = ArrayStatistics.Covariance(tempbeta3, tempbeta3);
            initialstatecovariance[2, 3] = ArrayStatistics.Covariance(tempbeta3, tempbeta4);

            initialstatecovariance[3, 0] = ArrayStatistics.Covariance(tempbeta4, tempbeta1);
            initialstatecovariance[3, 1] = ArrayStatistics.Covariance(tempbeta4, tempbeta2);
            initialstatecovariance[3, 2] = ArrayStatistics.Covariance(tempbeta4, tempbeta3);
            initialstatecovariance[3, 3] = ArrayStatistics.Covariance(tempbeta4, tempbeta4);

        }
        public (double[], double[], double[], double[]) GetDynamicBetas(double[] para)
        {
            var tvbeta1 = new double[yields.Count];
            var tvbeta2 = new double[yields.Count];
            var tvbeta3 = new double[yields.Count];
            var tvbeta4 = new double[yields.Count];
            var numofyields = 12;
            var lambda1 = para[0];
            var lambda2 = para[1];
            var sigma_beta = new double[4];
            var sigma_yields = new double[numofyields];
            for (int i = 0; i < sigma_yields.Length; i++)
            {
                if (i < 4)
                {
                    sigma_beta[i] = para[i + 2];
                }
                sigma_yields[i] = para[6 + i];
            }

            var M = Matrix<double>.Build;
            // Initilize state noise covariance
            var statenoisecovariance = M.DenseOfDiagonalArray(sigma_beta);
            var observationnoisecov = M.DenseOfDiagonalArray(sigma_yields);
            var statetransition = M.DenseDiagonal(sigma_beta.Length, sigma_beta.Length, 1.00);//Random walk;

            // Kalman filter alogrithm starts
            var I = M.DenseDiagonal(initialstate.RowCount, initialstate.RowCount, 1.00);
            var observationmodel = GetObservationModel(lambda1, lambda2, maturities);
            for (int i = 0; i < yields.Count; i++)
            {

                var priorstate = statetransition.Multiply(initialstate);
                var priorcov = statetransition.Multiply(initialstatecovariance).Multiply(statetransition.Transpose()).Add(statenoisecovariance);
                var tempobs = GetObservationAtSingleTimePoint(yields[yields.ElementAt(i).Key]);
                var innovation = tempobs.Subtract(observationmodel.Multiply(priorstate));
                var innovationcov = observationmodel.Multiply(priorcov).Multiply(observationmodel.Transpose()).Add(observationnoisecov);
                var kalmangain = priorcov.Multiply(observationmodel.Transpose()).Multiply(innovationcov.Inverse());
                var posterioristate = priorstate.Add(kalmangain.Multiply(innovation));
                tvbeta1[i] = posterioristate[0, 0];
                tvbeta2[i] = posterioristate[1, 0];
                tvbeta3[i] = posterioristate[2, 0];
                tvbeta4[i] = posterioristate[3, 0];
                initialstate = posterioristate.Clone();
                initialstatecovariance = I.Subtract(kalmangain.Multiply(observationmodel)).Multiply(priorcov);
            }
            return (tvbeta1, tvbeta2, tvbeta3, tvbeta4);
        }
        private double DynamicNS4FactorModelObjfun(double[] para)
        {
            // Split parameters
            var numofyields = 12;
            var lambda1 = para[0];
            var lambda2 = para[1];
            var sigma_beta = new double[4];
            var sigma_yields = new double[numofyields];
            for (int i = 0; i < sigma_yields.Length; i++)
            {
                if (i < 4)
                {
                    sigma_beta[i] = para[i + 2];
                }
                sigma_yields[i] = para[6 + i];
            }

            var M = Matrix<double>.Build;
            // Initilize state noise covariance
            var statenoisecovariance = M.DenseOfDiagonalArray(sigma_beta);
            var observationnoisecov = M.DenseOfDiagonalArray(sigma_yields);
            var statetransition = M.DenseDiagonal(sigma_beta.Length, sigma_beta.Length, 1.00);//Random walk;

            // Kalman filter alogrithm starts
            var I = M.DenseDiagonal(initialstate.RowCount, initialstate.RowCount, 1.00);
            double loglikelihood = 0;
            var timevaryingpara = new Dictionary<int, Matrix<double>>();
            var observationmodel = GetObservationModel(lambda1, lambda2, maturities);
            for (int i = 0; i < yields.Count; i++)
            {

                var priorstate = statetransition.Multiply(initialstate);
                var priorcov = statetransition.Multiply(initialstatecovariance).Multiply(statetransition.Transpose()).Add(statenoisecovariance);
                var tempobs = GetObservationAtSingleTimePoint(yields[yields.ElementAt(i).Key]);
                var innovation = tempobs.Subtract(observationmodel.Multiply(priorstate));
                var innovationcov = observationmodel.Multiply(priorcov).Multiply(observationmodel.Transpose()).Add(observationnoisecov);
                var kalmangain = priorcov.Multiply(observationmodel.Transpose()).Multiply(innovationcov.Inverse());
                var posterioristate = priorstate.Add(kalmangain.Multiply(innovation));
                if (checkdynamicpara(posterioristate) == false)
                {
                    loglikelihood = 999999999999999999.99;
                    break;
                }
                timevaryingpara.Add(i, posterioristate.Clone());
                initialstate = posterioristate.Clone();
                initialstatecovariance = I.Subtract(kalmangain.Multiply(observationmodel)).Multiply(priorcov);
                loglikelihood = loglikelihood + GetLogLiklihoodValueAtSingleTimePoint(innovationcov, innovation);
            }

            return loglikelihood;
        }
        private bool checkdynamicpara(Matrix<double> posterioristate)
        {
            var result = true;
            if (posterioristate[0, 0] + posterioristate[1, 0] <= 0)
            {
                result = false;
            }
            return result;
        }
        private Matrix<double> GetObservationModel(double lambda1, double lambda2, double[] matuirities)
        {
            var M = Matrix<double>.Build;
            var results = M.Dense(matuirities.Length, 4);
            for (int i = 0; i < results.RowCount; i++)
            {
                results[i, 0] = 1;
                results[i, 1] = (1 - Math.Exp(-lambda1 * matuirities[i])) / (lambda1 * matuirities[i]);
                results[i, 2] = (1 - Math.Exp(-lambda1 * matuirities[i])) / (lambda1 * matuirities[i]) - Math.Exp(-lambda1 * matuirities[i]);
                results[i, 3] = (1 - Math.Exp(-lambda2 * matuirities[i])) / (lambda2 * matuirities[i]) - Math.Exp(-lambda2 * matuirities[i]);
            }
            return results;
        }
        private Matrix<double> GetObservationAtSingleTimePoint(double[] x)
        {
            var M = Matrix<double>.Build;
            var result = M.Dense(x.Length, 1);
            for (int i = 0; i < result.RowCount; i++)
            {
                result[i, 0] = x[i];
            }

            return result;
        }
        private double GetLogLiklihoodValueAtSingleTimePoint(Matrix<double> innovationcov, Matrix<double> innovation)
        {
            return 0.5 * Math.Log(innovationcov.Determinant()) + 0.5 * innovation.Transpose().Multiply(innovationcov.Inverse()).Multiply(innovation)[0, 0];
        }
    }
        public class StaticNS4FactorModel
        {
            //Implement Nelson-Siegal-Svensson 4 factor model
            public double beta1 { get; set; }
            public double beta2 { get; set; }
            public double beta3 { get; set; }
            public double beta4 { get; set; }
            public double lambda1 { get; set; }
            public double lambda2 { get; set; }
            public double[] tau { get; set; }
            public double[] GetYield()
            {
                var result = new double[tau.Length];
                for (int i = 0; i < tau.Length; i++)
                {
                    var scalefactor1 = tau[i] / lambda1;
                    var scalefactor2 = tau[i] / lambda2;
                    var a = (1 - Math.Exp(-scalefactor1)) / scalefactor1;
                    var b = a - Math.Exp(-scalefactor1);
                    var c = (1 - Math.Exp(-scalefactor2)) / scalefactor2 - Math.Exp(-scalefactor2);
                    result[i] = beta1 + beta2 * a + beta3 * b + beta4 * c;
                }
                return result;
            }
        }
        public class StaticNS4FactorModelCalibration
        {
            public double[] yields { get; set; }
            public double[] maturities { get; set; }

            public double[] Calibration()
            {
                var PSO = new PSOOptimization();
                var lowerbound = new double[6] { 0.0000001, -29.99, -29.99, -29.99, 0.00000001, 0.00000001 };
                var upperbound = new double[6] { 14.99, 29.99, 29.99, 29.99, 29.99, 29.99 };
                PSO.initialguess = new double[6] { 2.1, -1.8, -2.1, 8.2, 0.5, 11.2 };
                PSO.lowerbound = lowerbound;
                PSO.upperbound = upperbound;
                PSO.maximumiteration = 5000;
                PSO.numofswarms = 200;
                PSO.inertiaweightmax = 1.2;
                PSO.inertiaweightmin = 0.1;
                PSO.objectfun = Objfun;
                PSO.tolerance = 0.000000001;
                PSO.Vmax = 4;
                PSO.c1 = 2;
                PSO.c2 = 2;
                PSO.chi = 0.73;
                var optimizedp = PSO.Optimize();
                return optimizedp;
            }
            public double Objfun(double[] para)
            {
                var sns4factor = new StaticNS4FactorModel();
                sns4factor.beta1 = para[0];
                sns4factor.beta2 = para[1];
                sns4factor.beta3 = para[2];
                sns4factor.beta4 = para[3];
                sns4factor.lambda1 = para[4];
                sns4factor.lambda2 = para[5];
                sns4factor.tau = maturities;
                var modelyields = sns4factor.GetYield();
                var error = 0.0;
                if (checkpara(sns4factor) == false)
                {
                    error = 9999999999999999.99;
                }
                else
                {
                    for (int i = 0; i < yields.Length; i++)
                    {
                        var temperror = (modelyields[i] - yields[i]);
                        error = error + temperror * temperror;
                    }
                }
                return error;
            }

            public double[] CalculateModelOutput(double[] tau, double[] para)
            {
                var sns4factor = new StaticNS4FactorModel();
                sns4factor.beta1 = para[0];
                sns4factor.beta2 = para[1];
                sns4factor.beta3 = para[2];
                sns4factor.beta4 = para[3];
                sns4factor.lambda1 = para[4];
                sns4factor.lambda2 = para[5];
                sns4factor.tau = tau;
                var modelyields = sns4factor.GetYield();
                return modelyields;
            }
            public bool checkpara(StaticNS4FactorModel n4factor)
            {
                var result = true;
                if (n4factor.beta1 + n4factor.beta2 <= 0)
                {
                    result = false;
                }
                return result;
            }
        }
    
}
