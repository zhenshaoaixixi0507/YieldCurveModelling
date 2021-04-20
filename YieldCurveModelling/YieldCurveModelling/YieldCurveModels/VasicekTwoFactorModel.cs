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
        //Implement Econometric Analysis of a Continuous Time Multi-Factor Generalized Vasicek Term Structure Model: International Evidence
        public double x10 { get; set; }
        public double x20 { get; set; }
        public double mu { get; set; }
        public double rho12 { get; set; }
        public double theta1 { get; set; }//market price of risk
        public double theta2 { get; set; }//market price of risk
        public double c1 { get; set; }
        public double c2 { get; set; }
        public double epsilon1 { get; set; }
        public double epsilon2 { get; set; }
        public double[] maturities { get; set; }

        public double[] GetYields()
        {
            var result = new double[maturities.Length];
            var longtermyield = GetLongTermYield();
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = longtermyield - GetOmegaValue(maturities[i])-GetHValue(epsilon1*maturities[i])*x10-GetHValue(epsilon2*maturities[i])*x20;
            }
            return result;
        }

        private double GetLongTermYield()
        {
            return mu + theta1 * (c1 / epsilon1 + c2 * rho12 / epsilon2) + theta2 * c2 * Math.Sqrt(1 - rho12 * rho12) / epsilon2 - 0.5 * (c1 * c1 / (epsilon1 * epsilon1) + 2 * c1 * c2 * rho12 / (epsilon1 * epsilon2) + c2 * c2 / (epsilon2 * epsilon2));
        }
        private double GetOmegaValue(double tau)
        {
            var item1 = GetHValue(epsilon1 * tau) * c1 / epsilon1 * (theta1 - (c1 / epsilon1 + c2 * rho12 / epsilon2));
            var item2= GetHValue(epsilon2 * tau) * c2 / epsilon2* (theta1*rho12+theta2*Math.Sqrt(1-rho12*rho12) - (c1*rho12 / epsilon1 + c2/ epsilon2));
            var item3 = 0.5 * GetHValue(2 * epsilon1 * tau) * c1 * c1 / (epsilon1 * epsilon1);
            var item4 = GetHValue((epsilon1 + epsilon2) * tau) * c1 * c2 * rho12 / (epsilon1 * epsilon2);
            var item5 = 0.5 * GetHValue(2 * epsilon2 * tau) * c2 * c2 / (epsilon2 * epsilon2);

            return item1+item2+item3+item4+item5;
        }
        private double GetHValue(double x)
        {
            return (1-Math.Exp(-x)) / x;
        }
    }
    public class StaticVasicekTwoFactorModelCalibration
    {
        public double[] maturities { get; set; }
        public double[] yields { get; set; }

        public double[] Calibration()
        {
            var lowerbound = new double[10] { -14.99, -14.99, -14.99, - 0.9999999, -4.99, -4.99, 0.0000001, 0.0000001, 0.0000001, 0.0000001 };
            var upperbound = new double[10] { 14.99, 14.99, 14.99,0.9999999, 4.99, 4.99, 4.99, 4.99, 14.99, 14.99 };

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
            var lowerbound = new double[10] { -14.99, -14.99, -14.99, -0.9999999, -4.99, -4.99, 0.0000001, 0.0000001, 0.0000001, 0.0000001 };
            var upperbound = new double[10] { 14.99, 14.99, 14.99, 0.9999999, 4.99, 4.99, 4.99, 4.99, 14.99, 14.99 };
            if (CheckStaticVasicekTwoFactorModelPara(para, lowerbound, upperbound) == false)
            {
                error = 99999999999999.99;
            }
            else
            {
                var x10 = para[0];
                var x20 = para[1];
                var mu = para[2];
                var rho12 = para[3];
                var theta1 = para[4];
                var theta2 = para[5];
                var c1 = para[6];
                var c2 = para[7];
                var epsilon1 = para[8];
                var epsilon2 = para[9];
                var SV2Factor = new StaticVasicekTwoFactorModel();
                SV2Factor.maturities = maturities;
                SV2Factor.x10 = x10;
                SV2Factor.x20 = x20;
                SV2Factor.mu = mu;
                SV2Factor.rho12 = rho12;
                SV2Factor.theta1 = theta1;
                SV2Factor.theta2 = theta2;
                SV2Factor.c1 = c1;
                SV2Factor.c2 = c2;
                SV2Factor.epsilon1 = epsilon1;
                SV2Factor.epsilon2 = epsilon2;
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
            var mu = para[2];
            var rho12 = para[3];
            var theta1 = para[4];
            var theta2 = para[5];
            var c1 = para[6];
            var c2 = para[7];
            var epsilon1 = para[8];
            var epsilon2 = para[9];
            var SV2Factor = new StaticVasicekTwoFactorModel();
            SV2Factor.maturities = maturities;
            SV2Factor.x10 = x10;
            SV2Factor.x20 = x20;
            SV2Factor.mu = mu;
            SV2Factor.rho12 = rho12;
            SV2Factor.theta1 = theta1;
            SV2Factor.theta2 = theta2;
            SV2Factor.c1 = c1;
            SV2Factor.c2 = c2;
            SV2Factor.epsilon1 = epsilon1;
            SV2Factor.epsilon2 = epsilon2;
            var modelyields = SV2Factor.GetYields();
            return modelyields;
        }
    }
    
}

