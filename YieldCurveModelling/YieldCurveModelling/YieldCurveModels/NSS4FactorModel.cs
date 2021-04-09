using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YieldCurveModelling.OptimizationAlgorithmLib;

namespace YieldCurveModelling.YieldCurveModels
{
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
                var c = (1 - Math.Exp(-scalefactor2)) / scalefactor2-Math.Exp(-scalefactor2);
                result[i] = beta1 + beta2 * a + beta3 * b+beta4*c;
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
            var upperbound = new double[6] {14.99, 29.99, 29.99, 29.99, 29.99,29.99};
            PSO.initialguess = new double[6] {2.1, -1.8,-2.1,8.2,0.5,11.2 };
            PSO.lowerbound = lowerbound;
            PSO.upperbound = upperbound;
            PSO.maximumiteration = 5000;
            PSO.numofswarms =200;
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
            sns4factor.beta1 =para[0] ;
            sns4factor.beta2 = para[1];
            sns4factor.beta3 = para[2];
            sns4factor.beta4 = para[3] ;
            sns4factor.lambda1 =para[4];
            sns4factor.lambda2 =para[5];
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
