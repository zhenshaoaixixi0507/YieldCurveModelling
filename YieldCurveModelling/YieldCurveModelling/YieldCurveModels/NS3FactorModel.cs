using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YieldCurveModelling.OptimizationAlgorithmLib;
using MathNet.Numerics.Statistics;
using YieldCurveModelling.Helpers;
namespace YieldCurveModelling.YieldCurveModels
{

    public class DynamicNS3FactorModelCalibration
    {
        public Dictionary<string,double[]> yields { get; set; }
        public double[] maturities { get; set; }
        private Matrix<double> initialstate;
        private Matrix<double> initialstatecovariance;
        public double[] Optimize()
        {
            Initialization();

            var lowerbound = new double[16];
            var upperbound = new double[16];
            var initialguess = new double[16];
            lowerbound[0] = 0.0000001;
            upperbound[0] = 29.99;
            initialguess[0] = 0.87;
            
            for (int i = 1; i < lowerbound.Length; i++)
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
            PSO.objectfun = DynamicNS3FactorModelObjfun;
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
            initialstate= M.Dense(3, 1);
            initialstatecovariance= M.DenseDiagonal(3, 3, 0.00);
            var tempyields = new double[maturities.Length];
            var StaticNS3factorCalibration = new StaticNS3FactorModelCalibration();
            StaticNS3factorCalibration.maturities = maturities;
            var tempbeta1 = new double[30];
            var tempbeta2 = new double[30];
            var tempbeta3 = new double[30];

            Parallel.For(0, 30, i =>
            {
                tempyields = yields[yields.ElementAt(i).Key].Clone() as double[];
                StaticNS3factorCalibration.yields = tempyields;
                var temppara = StaticNS3factorCalibration.Calibration();
                tempbeta1[i] = temppara[0];
                tempbeta2[i] = temppara[1];
                tempbeta3[i] = temppara[2];

            });
            initialstate[0, 0] = ArrayStatistics.Mean(tempbeta1);
            initialstate[1, 0] = ArrayStatistics.Mean(tempbeta2);
            initialstate[2, 0] = ArrayStatistics.Mean(tempbeta3);
          
            initialstatecovariance[0, 0] = ArrayStatistics.Covariance(tempbeta1, tempbeta1);
            initialstatecovariance[0, 1] = ArrayStatistics.Covariance(tempbeta1, tempbeta2);
            initialstatecovariance[0, 2] = ArrayStatistics.Covariance(tempbeta1, tempbeta3);
          
            initialstatecovariance[1, 0] = ArrayStatistics.Covariance(tempbeta2, tempbeta1);
            initialstatecovariance[1, 1] = ArrayStatistics.Covariance(tempbeta2, tempbeta2);
            initialstatecovariance[1, 2] = ArrayStatistics.Covariance(tempbeta2, tempbeta3);
           
            initialstatecovariance[2, 0] = ArrayStatistics.Covariance(tempbeta3, tempbeta1);
            initialstatecovariance[2, 1] = ArrayStatistics.Covariance(tempbeta3, tempbeta2);
            initialstatecovariance[2, 2] = ArrayStatistics.Covariance(tempbeta3, tempbeta3);         

        }
        public (double[], double[], double[]) GetDynamicBetas(double[] para)
        {
            var tvbeta1 = new double[yields.Count];
            var tvbeta2 = new double[yields.Count];
            var tvbeta3 = new double[yields.Count];
            var numofyields = 12;
            var lambda = para[0];
            var sigma_beta = new double[3];
            var sigma_yields = new double[numofyields];
            for (int i = 0; i < sigma_yields.Length; i++)
            {
                if (i < 3)
                {
                    sigma_beta[i] = para[i + 1];
                }
                sigma_yields[i] = para[4 + i];
            }

            var M = Matrix<double>.Build;
            // Initilize state noise covariance
            var statenoisecovariance = M.DenseOfDiagonalArray(sigma_beta);
            var observationnoisecov = M.DenseOfDiagonalArray(sigma_yields);
            var statetransition = M.DenseDiagonal(sigma_beta.Length, sigma_beta.Length, 1.00);//Random walk;

            // Kalman filter alogrithm starts
            var I = M.DenseDiagonal(initialstate.RowCount, initialstate.RowCount, 1.00);
            var observationmodel = GetObservationModel(lambda, maturities);
            for (int i = 0; i < yields.Count; i++)
            {

                var priorstate = statetransition.Multiply(initialstate);
                var priorcov = statetransition.Multiply(initialstatecovariance).Multiply(statetransition.Transpose()).Add(statenoisecovariance);
                var tempobs = GetObservationAtSingleTimePoint(yields[yields.ElementAt(i).Key]);
                var innovation = tempobs.Subtract(observationmodel.Multiply(priorstate));
                var innovationcov = observationmodel.Multiply(priorcov).Multiply(observationmodel.Transpose()).Add(observationnoisecov);
                var kalmangain = priorcov.Multiply(observationmodel.Transpose()).Multiply(innovationcov.Inverse());
                var posterioristate = priorstate.Add(kalmangain.Multiply(innovation));
                tvbeta1[i] = posterioristate[0,0];
                tvbeta2[i] = posterioristate[1, 0];
                tvbeta3[i] = posterioristate[2,0];
                initialstate = posterioristate.Clone();
                initialstatecovariance = I.Subtract(kalmangain.Multiply(observationmodel)).Multiply(priorcov);
            }
            return (tvbeta1, tvbeta2, tvbeta3);
        }
        private double DynamicNS3FactorModelObjfun(double[] para)
        {
            // Split parameters
            var numofyields = 12;
            var lambda = para[0];
            var sigma_beta= new double[3];
            var sigma_yields = new double[numofyields];
            for (int i = 0; i < sigma_yields.Length; i++)
            {
                if (i < 3)
                {
                    sigma_beta[i] = para[i+1];
                }
                sigma_yields[i] = para[4 + i];
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
            var observationmodel = GetObservationModel(lambda, maturities);
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
            if (posterioristate[0,0] + posterioristate[1,0] <= 0)
            {
                result = false;
            }
            return result;
        }
        private Matrix<double> GetObservationModel(double lambda,double[]matuirities)
        {
            var M = Matrix<double>.Build;
            var results = M.Dense(matuirities.Length,3);
            for (int i = 0; i < results.RowCount; i++)
            {
                results[i, 0] = 1;
                results[i, 1] = (1 - Math.Exp(-lambda * matuirities[i])) / (lambda * matuirities[i]);
                results[i, 2] = (1 - Math.Exp(-lambda * matuirities[i])) / (lambda * matuirities[i])-Math.Exp(-lambda*matuirities[i]);
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
    public class StaticNS3FactorModel
    {
        //Implement Nelson-Siegal 3 factor model
        public double beta1 { get; set; }
        public double beta2 { get; set; }
        public double beta3 { get; set; }
        public double lambda { get; set; }
        public double[] tau { get; set; }
        public double[] GetYield()
        {
            var result = new double[tau.Length];
            for (int i = 0; i < tau.Length; i++)
            {
                var scalefactor = tau[i] / lambda;
                var a = (1 - Math.Exp(-scalefactor)) / scalefactor;
                var b = a - Math.Exp(-scalefactor);

                result[i] = beta1 + beta2 * a + beta3 * b;
            }
            return result;
        }
    }
    public class StaticNS3FactorModelCalibration
    { 
        public double[] yields { get; set; }
        public double[] maturities { get; set; }

        public double[] Calibration()
        {
            var lowerbound = new double[4] { 0.0000001, -29.99, -29.99, 0.0000001 };
            var upperbound = new double[4] { 14.99, 29.99, 29.99, 29.99 };
            //var PSO = new PSOOptimization();
            //PSO.initialguess = new double[4] { 2.1, -1.5, -2.1, 0.87 };
            //PSO.lowerbound = lowerbound;
            //PSO.upperbound = upperbound;
            //PSO.maximumiteration = 5000;
            //PSO.numofswarms = 200;
            //PSO.inertiaweightmax = 1.2;
            //PSO.inertiaweightmin = 0.1;
            //PSO.objectfun = StaticNS3FactorModelObjfun;
            //PSO.tolerance = 0.000000001;
            //PSO.Vmax = 4;
            //PSO.c1 = 2;
            //PSO.c2 = 2;
            //PSO.chi = 0.73;
            //var optimizedp = PSO.Optimize();

            var ChaoticPSO = new ChaoticPSOOptimization();
            ChaoticPSO.lowerbound = lowerbound;
            ChaoticPSO.upperbound = upperbound;

            ChaoticPSO.inertiaweightmax = 1.2;
            ChaoticPSO.inertiaweightmin = 0.1;
            ChaoticPSO.objectfun = StaticNS3FactorModelObjfun;
            ChaoticPSO.tolerance = 0.000000001;
            ChaoticPSO.c1 = 2;
            ChaoticPSO.c2 = 2;
            var optimizedp = ChaoticPSO.Optimize();

            return optimizedp;
        }
        public double StaticNS3FactorModelObjfun(double[]para)
        {
            var sns3factor = new StaticNS3FactorModel();
            sns3factor.beta1 = para[0];
            sns3factor.beta2 = para[1];
            sns3factor.beta3 = para[2];
            sns3factor.lambda = para[3];
            sns3factor.tau = maturities;
            var modelyields = sns3factor.GetYield();
            var error = 0.0;
            if (checkpara(sns3factor) == false)
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

        public double[] CalculateModelOutput(double[]tau,double[]para)
        {
            var sns3factor = new StaticNS3FactorModel();
            sns3factor.beta1 = para[0];
            sns3factor.beta2 = para[1];
            sns3factor.beta3 = para[2];
            sns3factor.lambda = para[3];
            sns3factor.tau =tau;
            var modelyields = sns3factor.GetYield();
            return modelyields;
        }
        public bool checkpara(StaticNS3FactorModel n3factor)
        {
            var result = true;
            if (n3factor.beta1 + n3factor.beta2 <= 0)
            {
                result = false;
            }
            return result;
        }

    }
}
