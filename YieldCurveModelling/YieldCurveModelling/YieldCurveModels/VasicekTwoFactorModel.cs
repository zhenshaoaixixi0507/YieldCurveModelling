using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YieldCurveModelling.OptimizationAlgorithmLib;

namespace YieldCurveModelling.YieldCurveModels
{
    public class StaticVasicekTwoFactorModel
    {
        public double x10 { get; set; }
        public double x20 { get; set; }
        public double rho12 { get; set; }
        public double sigma1 { get; set; }
        public double sigma2 { get; set; }
        public double k1 { get; set; }
        public double k2 { get; set; }
        public double mu1 { get; set; }
        public double mu2 { get; set; }
        public double[] maturities { get; set; }

        public double[] GetYields()
        {
            var result = new double[maturities.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = GetYield(maturities[i]);
            }
            return result;
        }
        private double GetYield(double tau)
        {
            var B1 = GetBValue(k1, tau);
            B1 = B1 * x10;
            var B2 = GetBValue(k2, tau);
            B2 = B2 * x20;
            var Pi = GetPiValue(tau);
            return -(Pi + B1 + B2) / tau;
        }
        private double GetPiValue(double tau)
        {
            var item1 = rho12 * sigma1 * sigma2 / (2 * k1 * k2);
            var item2 = (1 - Math.Exp(-(k1 + k2) * tau)) / (k1 + k2);
            var item3 = (1 - Math.Exp(-k2 * tau)) / k2;
            var item4 = (1 - Math.Exp(-k1 * tau)) / k1;
            item1 = item1 * (item2 + item3 + item4 + tau);

            var B1 = GetBValue(k1, tau);
            var B2 = GetBValue(k2, tau);
            B1 = (B1 + tau) * (sigma1 * sigma1 / (2 * k1 * k1) - mu1) - sigma1 * sigma1 / (4 * k1 * k1) * B1 * B1;
            B2 = (B2 + tau) * (sigma2 * sigma2 / (2 * k2 * k2) - mu2) - sigma2 * sigma2 / (4 * k2 * k2) * B2 * B2;
            item1 = item1 + B1 + B2;
            return item1;
        }
        private double GetBValue(double k, double tau)
        {
            return (Math.Exp(-k * tau) - 1) / k;
        }
    }
    public class StaticVasicekTwoFactorModelCalibration
    {
        public double[] maturities { get; set; }
        public double[] yields { get; set; }

        public double[] Calibration()
        {
            var lowerbound = new double[9] { -14.99, -14.99, 0.0000001, 0.0000001, 0.0000001, 0.0000001, 0.0000001, -14.99, -14.99 };
            var upperbound = new double[9] { 14.99, 14.99, 0.9999999, 4.99, 4.99, 4.99, 4.99, 14.99, 14.99 };

            var ChaoticPSO = new ChaoticPSOOptimization();
            ChaoticPSO.lowerbound = lowerbound;
            ChaoticPSO.upperbound = upperbound;
            ChaoticPSO.inertiaweightmax = 1.2;
            ChaoticPSO.inertiaweightmin = 0.1;
            ChaoticPSO.objectfun = StaticVasicekTwoFactorModelObj;
            ChaoticPSO.tolerance = 0.000000001;
            ChaoticPSO.c1 = 2;
            ChaoticPSO.c2 = 2;
            var optimizedp = ChaoticPSO.Optimize();

        return optimizedp;
        }
        private double StaticVasicekTwoFactorModelObj(double[] para)
        {
            var error = 0.0;
            var lowerbound = new double[9] { -14.99, -14.99, 0.0000001, 0.0000001, 0.0000001, 0.0000001, 0.0000001, -14.99, -14.99 };
            var upperbound = new double[9] { 14.99, 14.99, 0.9999999, 4.99, 4.99, 4.99, 4.99, 14.99, 14.99 };
            if (CheckStaticVasicekTwoFactorModelPara(para, lowerbound, upperbound) == false)
            {
                error = 99999999999999.99;
            }
            else
            {
                var x10 = para[0];
                var x20 = para[1];
                var rho12 = para[2];
                var sigma1 = para[3];
                var sigma2 = para[4];
                var k1 = para[5];
                var k2 = para[6];
                var mu1 = para[7];
                var mu2 = para[8];
                var SV2Factor = new StaticVasicekTwoFactorModel();
                SV2Factor.maturities = maturities;
                SV2Factor.x10 = x10;
                SV2Factor.x20 = x20;
                SV2Factor.rho12 = rho12;
                SV2Factor.sigma1 = sigma1;
                SV2Factor.sigma2 = sigma2;
                SV2Factor.k1 = k2;
                SV2Factor.k2 = k2;
                var modelyields = SV2Factor.GetYields();

                for (int i = 0; i < modelyields.Length; i++)
                {
                    error = error + (modelyields[i] - yields[i]) * (modelyields[i] - yields[i]);
                }
            }

            return error;
        }

        private bool CheckStaticVasicekTwoFactorModelPara(double[] para,double[]lowerbound,double[]upperbound)
        {
            var result = true;
            for (int i = 0; i < para.Length; i++)
            {
                if (para[i] < lowerbound[i]) 
                {
                    result = false;
                    break;
                }
                if (para[i] > upperbound[i]) 
                {
                    result = false;
                    break;
                }
            }
            return result;
             
        }
        public double[] CalcualteModelOutput(double[] para)
        {
            var x10 = para[0];
            var x20 = para[1];
            var rho12 = para[2];
            var sigma1 = para[3];
            var sigma2 = para[4];
            var k1 = para[5];
            var k2 = para[6];
            var mu1 = para[7];
            var mu2 = para[8];
            var SV2Factor = new StaticVasicekTwoFactorModel();
            SV2Factor.maturities = maturities;
            SV2Factor.x10 = x10;
            SV2Factor.x20 = x20;
            SV2Factor.rho12 = rho12;
            SV2Factor.sigma1 = sigma1;
            SV2Factor.sigma2 = sigma2;
            SV2Factor.k1 = k2;
            SV2Factor.k2 = k2;
            var modelyields = SV2Factor.GetYields();
            return modelyields;
        }
    }
    
}

