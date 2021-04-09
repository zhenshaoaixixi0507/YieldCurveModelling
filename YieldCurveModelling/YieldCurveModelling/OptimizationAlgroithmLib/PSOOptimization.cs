using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Random;

namespace YieldCurveModelling.OptimizationAlgorithmLib
{
    public class PSOOptimization
    {
        //Particle Swarm Optimization Method for Constrained Optimization Problems
        //written by Konstantinos E. Parsopoulos and Michael N. Vrahtis
        //https://www.cs.cinvestav.mx/~constraint/papers/eisci.pdf
        public double[] lowerbound { get; set; }
        public double[] upperbound { get; set; }
        public int maximumiteration { get; set; }
        public int numofswarms { get; set; }
        public double inertiaweightmax { get; set; }
        public double inertiaweightmin { get; set; }
        public double c1 { get; set; }
        public double c2 { get; set; }
        public double chi { get; set; }
        public Func<double[], double> objectfun { get; set; }
        public double Vmax { get; set; }
        public double tolerance { get; set; }
        public double[] initialguess { get; set; }

        public double[] Optimize()
        {
            // Calculate delta for interiaweight
            var detalweight = (inertiaweightmax - inertiaweightmin) / maximumiteration;
            //Generate initial guess
            var globalbest = new double[lowerbound.Length];
            var localswarm = new Dictionary<int, double[]>();
            var localbest = new Dictionary<int, double[]>();
            var Velocity = new Dictionary<int, double[]>();
          

            for (int j = 0; j < globalbest.Length; j++)
            {
                globalbest[j] = initialguess[j];
            }
            var minerror = objectfun(globalbest);

            for (int i = 0; i < numofswarms; i++)
            {
                var rnd = new MersenneTwister(i + 1, true);
                var rnd2 = new MersenneTwister(i + 2, true);
                var temp = new double[lowerbound.Length];
                var tempbest = new double[lowerbound.Length];
                var tempV = new double[lowerbound.Length];
                for (int j = 0; j < temp.Length; j++)
                {
                    temp[j] = (upperbound[j] - lowerbound[j]) * rnd.NextDouble() + lowerbound[j];
                    tempV[j] = 2 * Vmax * rnd2.NextDouble() - Vmax;
                    tempbest[j] = initialguess[j];
                }
                localswarm.Add(i, temp.Clone() as double[]);
                localbest.Add(i, tempbest.Clone() as double[]);
                Velocity.Add(i, tempV.Clone() as double[]);
               
                    var error = double.IsNaN(objectfun(temp))? 9999999999999.999: objectfun(temp);
                    if (error < minerror)
                    {
                        minerror = error;
                        globalbest = temp.Clone() as double[];
                    }

            }

            //Iteration starts
            var oldglobalerror = minerror;
            for (int i = 0; i < maximumiteration; i++)
            {
                var tempweight = inertiaweightmin + detalweight * i;
                for (int j = 0; j < numofswarms; j++)
                {
                    var tempx = localswarm[j].Clone() as double[];
                    var tempV = Velocity[j].Clone() as double[];
                    var templocalbest = localbest[j].Clone() as double[];
                    var r1 = GenerateR(j + i + 1, tempx.Length);
                    var r2 = GenerateR(j + i + 2, tempV.Length);
                    var item1 = ArrayMultiplyConstant(tempV, tempweight);
                    var item2 = ArrayMultiplyConstant(r1, c1);
                    item2 = ArrayMultiplyArray(item2, ArrayMinus(templocalbest, tempx));
                    var item3 = ArrayMultiplyConstant(r2, c2);
                    item3 = ArrayMultiplyArray(item3, ArrayMinus(globalbest, tempx));
                    item1 = ArrayPlus(item1, item2);
                    item1 = ArrayPlus(item1, item3);
                    var newV = ArrayMultiplyConstant(item1, chi);
                    newV = ConstrainV(newV, Vmax);
                    var newX = ArrayPlus(tempx, newV);
                    newX = ConstrainX(newX);
                    localswarm[j] = newX.Clone() as double[];
                    Velocity[j] = newV.Clone() as double[];
                    var newlocalbest = swaplocalbest(tempx, newX);
                    localbest[j] = newlocalbest.Clone() as double[];
                    var localerror = objectfun(localbest[j]);
                    if (localerror < minerror)
                    {
                        globalbest = localbest[j].Clone() as double[];
                        minerror = localerror;
                    }
                }
                if (Math.Abs(oldglobalerror - minerror) < tolerance && i > 50)
                {
                    break;
                }
                else
                {
                    oldglobalerror = minerror;
                    Console.WriteLine("ObjectFun: " + Convert.ToString(minerror));
                }
                
            }

            return globalbest;
        }
        public double[] ConstrainX(double[] x)
        {
            var result = new double[x.Length];
            for (int i = 0; i < result.Length; i++)
            { 
                result[i]= Math.Min(Math.Max(x[i], lowerbound[i]), upperbound[i]);
            }
            return result;
        }
        public double[] ConstrainV(double[] v, double Vmax)
        {
            var result = new double[v.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Math.Min(Math.Max(v[i], -Vmax), Vmax);
            }
            return result;
        }
        public double[] swaplocalbest(double[] oldx, double[] newx)
        {
            var result = new double[oldx.Length];
            var olderror = objectfun(oldx);
            if (double.IsNaN(olderror))
            {
                olderror = 9999999999999.999;
            }
            var newerror = objectfun(newx);
            if (double.IsNaN(newerror))
            {
                newerror = 9999999999999.999;
            }
            if (newerror < olderror)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = newx[i];
                }
            }
            else
            {
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = oldx[i];
                }
            }
            return result;
        }
        public double[] GenerateR(int seed,int length)
        { 
            var rnd= new MersenneTwister(seed + 1, true);
            var result = new double[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = rnd.NextDouble();
            }
            return result;
        }
        public double[] ArrayMinus(double[]x,double[]y)
        {
            var result = new double[x.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = x[i] - y[i];
            }
            return result;
        }
        public double[] ArrayPlus(double[] x, double[] y)
        {
            var result = new double[x.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = x[i] + y[i];
            }
            return result;
        }
        public double[] ArrayMultiplyConstant(double[] x, double c)
        {
            var result = new double[x.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = x[i] * c;
            }
            return result;
        }
        public double[] ArrayMultiplyArray(double[] x, double[] y)
        {
            var result = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                result[i] = x[i] * y[i];
            }
            return result;
        }
    }
}
