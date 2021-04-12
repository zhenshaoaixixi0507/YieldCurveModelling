using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YieldCurveModelling.OptimizationAlgorithmLib;
namespace YieldCurveModelling.YieldCurveModels
{

    public class DynamicNS3FactorModel
    { 
        public double[] beta1 { get; set; }
        public double[] beta2 { get; set; }
        public double[] beta3 { get; set; }
        public double[] lambda { get; set; }
        public double[] tau { get; set; }
        public Dictionary<int, double[]> GetYields()
        {
            var result = new Dictionary<int, double[]>();
            for (int t = 0; t < beta1.Length; t++)
            {
                var temp = new double[tau.Length];
                for (int i = 0; i < tau.Length; i++)
                {
                    var scalefactor = tau[i] / lambda[t];
                    var a = (1 - Math.Exp(-scalefactor)) / scalefactor;
                    var b = a - Math.Exp(-scalefactor);

                    temp[i] = beta1[t] + beta2[t] * a + beta3[t] * b;
                }
                result.Add(t, temp.Clone() as double[]);
            }
            return result;
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
            var PSO = new PSOOptimization();
            PSO.initialguess = new double[4] { 2.1, -1.5, -2.1, 0.87 };
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
        public double Objfun(double[]para)
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
