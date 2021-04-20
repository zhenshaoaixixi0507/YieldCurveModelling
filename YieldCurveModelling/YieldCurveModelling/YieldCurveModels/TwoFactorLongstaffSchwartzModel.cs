using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YieldCurveModelling.OptimizationAlgorithmLib;

namespace YieldCurveModelling.YieldCurveModels
{
    public class StaticTwoFactorLongstaffSchwartzModel
    {
        // Implement Interest rate volatility and and the term structure two factor general equilibrium model
        public double x10 { get; set; }
        public double x20 { get; set; }
        public double alpha { get; set; }
        public double beta { get; set; }
        public double gamma { get; set; }
        public double epsilon { get; set; }
        public double eita { get; set; }
        public double vega { get; set; }
        public double c { get; set; }
        public double[] maturities { get; set; }

        public double[] GetYields()
        {
            var result = new double[maturities.Length];
            var phi = Math.Sqrt(2*alpha+epsilon*epsilon);
            var psi = Math.Sqrt(2*beta+vega*vega);
            var k = gamma * (epsilon + phi) + eita * (vega + psi);
            var r = alpha * x10 + beta * x20;
            var V = alpha * alpha * x10 * x10 + beta * beta * x20 * x20;
            for (int i = 0; i < result.Length; i++)
            {
                var tau = maturities[i];
                var A = 2 * phi / ((epsilon + phi) * (Math.Exp(phi * tau) - 1) + 2 * phi);
                var B=2*psi/ ((vega + psi) * (Math.Exp(psi * tau) - 1) + 2 * psi);
                var C = (alpha * phi * (Math.Exp(psi * tau) - 1) * B - beta * psi * (Math.Sqrt(phi * tau) - 1) * A) / (phi*psi*(beta-alpha));
                var D = (psi * (Math.Exp(phi * tau) - 1) * A - phi* (Math.Sqrt(psi * tau) - 1) * B) / (phi * psi * (beta - alpha));
                result[i] = -(k * tau + 2 * gamma * Math.Log(A) + 2 * eita * Math.Log(B) + C * r + D * V) / tau+c;
            }
            return result;
        }

       
    }
    public class StaticTwoFactorLongstaffSchwartzModelCalibration
    {
        public double[] maturities { get; set; }
        public double[] yields { get; set; }

        public double[] Calibration()
        {
            var lowerbound = new double[9] { 0.0000001, 0.0000001, 0.0000001, 0.0000001, 0.0000001, 0.0000001, 0.0000001, 0.0000001, -29.99 };
            var upperbound = new double[9] { 29.99, 29.99, 29.99, 29.99, 29.99, 29.99, 29.99, 29.99, 29.99 };

            var ChaoticPSO = new ChaoticPSOOptimization();
            ChaoticPSO.lowerbound = lowerbound;
            ChaoticPSO.upperbound = upperbound;
            ChaoticPSO.inertiaweightmax = 1.2;
            ChaoticPSO.inertiaweightmin = 0.1;
            ChaoticPSO.objectfun = StaticTwoFactorLongstaffSchwartzModelObj;
            ChaoticPSO.tolerance = 0.000000001;
            ChaoticPSO.c1 = 2;
            ChaoticPSO.c2 = 2;
            var optimizedp = ChaoticPSO.Optimize();

            return optimizedp;
        }
        private double StaticTwoFactorLongstaffSchwartzModelObj(double[] para)
        {
            var error = 0.0;
            var lowerbound = new double[9] { 0.0000001, 0.0000001, 0.0000001, 0.0000001, 0.0000001, 0.0000001, 0.0000001, 0.0000001, -29.99 };
            var upperbound = new double[9] { 29.99, 29.99, 29.99, 29.99, 29.99, 29.99, 29.99, 29.99, 29.99 };
            if (CheckStaticTwoFactorLongstaffSchwartzModelPara(para, lowerbound, upperbound) == false)
            {
                error = 99999999999999.99;
            }
            else
            {
                var x10 = para[0];
                var x20 = para[1];
                var alpha = para[2];
                var beta = para[3];
                var gamma = para[4];
                var epsilon = para[5];
                var eita = para[6];
                var vega = para[7];
                var c = para[8];
               
                var LS2Factor = new StaticTwoFactorLongstaffSchwartzModel();
                LS2Factor.maturities = maturities;
                LS2Factor.x10 = x10;
                LS2Factor.x20 = x20;
                LS2Factor.alpha = alpha;
                LS2Factor.beta = beta;
                LS2Factor.gamma = gamma;
                LS2Factor.epsilon = epsilon;
                LS2Factor.eita = eita;
                LS2Factor.vega = vega;
                //LS2Factor.c = c;
       
                var modelyields = LS2Factor.GetYields();

                for (int i = 0; i < modelyields.Length; i++)
                {
                    error = error + (modelyields[i] - yields[i]) * (modelyields[i] - yields[i]);
                }
                if (Double.IsNaN(error))
                { 
                    error = 99999999999999.99;
                }
            }

            return error;
        }

        private bool CheckStaticTwoFactorLongstaffSchwartzModelPara(double[] para, double[] lowerbound, double[] upperbound)
        {
            var result = true;
            var alpha = para[2];
            var beta = para[3];
            var gamma = para[4];
            var epsilon = para[5];
            var eita = para[6];
            var vega = para[7];
            var phi = Math.Sqrt(2 * alpha + epsilon * epsilon);
            var psi = Math.Sqrt(2 * beta + vega * vega);
            if (gamma * (phi - epsilon) + eita * (psi - vega) < 0)
            {
                result = false;
            }
            else
            {

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
            }
            return result;

        }
        public double[] CalcualteModelOutput(double[] para)
        {
            var x10 = para[0];
            var x20 = para[1];
            var alpha = para[2];
            var beta = para[3];
            var gamma = para[4];
            var epsilon = para[5];
            var eita = para[6];
            var vega = para[7];
            var c = para[8];

            var LS2Factor = new StaticTwoFactorLongstaffSchwartzModel();
            LS2Factor.maturities = maturities;
            LS2Factor.x10 = x10;
            LS2Factor.x20 = x20;
            LS2Factor.alpha = alpha;
            LS2Factor.beta = beta;
            LS2Factor.gamma = gamma;
            LS2Factor.epsilon = epsilon;
            LS2Factor.eita = eita;
            LS2Factor.vega = vega;
            //LS2Factor.c = c;
            var modelyields = LS2Factor.GetYields();
            return modelyields;
        }
    }
}
