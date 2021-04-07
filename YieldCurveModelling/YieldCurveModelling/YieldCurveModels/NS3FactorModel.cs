using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YieldCurveModelling.PSOAlgorithm;
namespace YieldCurveModelling.YieldCurveModels
{
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
            var PSO = new PSOOptimization();
            var lowerbound = new double[4] { 0.0000001, -9.99, -9.99, 0.0000001 };
            var upperbound = new double[4] { 9.99,9.99, 9.99, 9.99 };
            PSO.initialguess = new double[4] { (double)1/2.1,(double)-1/1.5,(double)-1/2.1,(double)1/0.87};
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
            sns3factor.beta1 = (double)1/(para[0]+0.00000000001);
            sns3factor.beta2 = (double)1 / (para[1] + 0.00000000001);
            sns3factor.beta3 = (double)1 / (para[2] + 0.00000000001);
            sns3factor.lambda = (double)1 / (para[3] + 0.00000000001);
            sns3factor.tau = maturities;
            var modelyields = sns3factor.GetYield();
            var error = 0.0;
            if (checkpara(para) == false)
            {
                error = 9999999999999999.99;
            }
            else
            {
                for (int i = 0; i < yields.Length; i++)
                {
                    var temperror = modelyields[i] - yields[i];
                    error = error + temperror * temperror;
                }
            }
            return error;
        }

        public double[] CalculateModelOutput(double[]tau,double[]para)
        {
            var sns3factor = new StaticNS3FactorModel();
            sns3factor.beta1 = (double)1 / (para[0] + 0.00000000001);
            sns3factor.beta2 = (double)1 / (para[1] + 0.00000000001);
            sns3factor.beta3 = (double)1 / (para[2] + 0.00000000001);
            sns3factor.lambda = (double)1 / (para[3] + 0.00000000001);
            sns3factor.tau =tau;
            var modelyields = sns3factor.GetYield();
            return modelyields;
        }
        public bool checkpara(double[]para)
        {
            var result = true;
            if (para[0] + para[1] <= 0)
            {
                result = false;
            }
            return result;
        }

    }
}
