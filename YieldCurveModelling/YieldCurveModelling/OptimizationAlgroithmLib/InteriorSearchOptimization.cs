using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

namespace YieldCurveModelling.OptimizationAlgorithmLib
{
    public class InteriorSearchOptimization
    {
        //Interior search algorithm (ISA): A novel approach for global optimization
        //written by Amir H. Gandomi
        //https://www.sciencedirect.com/science/article/abs/pii/S0019057814000597
        public double[] lowerbound { get; set; }
        public double[] upperbound { get; set; }
        public int maximumiteration { get; set; }
        public int locationsize { get; set; }
        public double tolerance { get; set; }
        public Func<double[], double> objectfun { get; set; }
        public int sizeofinitialguess { get; set; }
        public double alpha { get; set; }
        public double[] Optimize()
        {

            //Initialize 

            var best = new double();
            var globalbest = new double[lowerbound.Length];
            best = 9999999999999999.99;
            for (int i = 0; i < sizeofinitialguess; i++)
            {
                var rnd = new MersenneTwister(i + 3, true);
                var tempx = new double[lowerbound.Length];
                for (int j = 0; j < lowerbound.Length; j++)
                {
                    tempx[j] = rnd.NextDouble() * (upperbound[j] - lowerbound[j]) + lowerbound[j];
                }
                var newerror = objectfun(tempx);

                if (newerror < best)
                {
                    best = newerror;
                    globalbest = tempx.Clone() as double[];

                }
            }
           
            var component=globalbest.Clone() as double[];
            var mirror=globalbest.Clone() as double[];
            var oldbest = best;
            //Iteration starts
            for(int i=0;i<maximumiteration;i++)
            {
                var rnd1= new MersenneTwister(i+1,true);
                var rnd2= new MersenneTwister(i+2,true);
                var rnd3=new MersenneTwister(i + 3, true);
                for(int j=0;j<locationsize;j++)
                {
                   globalbest=Generatenewglobal(i+j,globalbest).Clone() as double[];
                   var r1=rnd1.NextDouble();
                   if(r1<=alpha)
                   {
                        var r3 = rnd3.NextDouble();
                        (var tempmirror, var tempcomponent) = GeneratenMirrorAndComponent(r3, component, globalbest);
                        mirror = tempmirror.Clone() as double[];
                        var newcomponent = tempcomponent.Clone() as double[];
                        var errorofnewcompo = objectfun(newcomponent);
                        var errorofoldcompo = objectfun(component);
                        if (errorofnewcompo < errorofoldcompo)
                        {
                            component = newcomponent.Clone() as double[];
                        }
                        if (errorofnewcompo < best)
                        {
                            best = errorofnewcompo;
                        }
                   }
                    if (r1 > alpha)
                    {
                        var r2 = rnd2.NextDouble();
                        var newcomponent = GenerateComponent(r2,component).Clone() as double[];
                        var errorofnewcompo = objectfun(newcomponent);
                        var errorofoldcompo = objectfun(component);
                        if (errorofnewcompo < errorofoldcompo)
                        {
                            component = newcomponent.Clone() as double[];
                        }
                        if (errorofnewcompo < best)
                        {
                            best = errorofnewcompo;
                        }
                    }
                }

                //if (Math.Abs(oldbest - best) < tolerance && i > Math.Floor((double)maximumiteration / 2))
                if (Math.Abs(oldbest - best) < tolerance && i > 500)
                {
                    break;
                }
                if (best < oldbest)
                {
                    oldbest = best;
                }

                Console.WriteLine("Error: " + Convert.ToString(best));
            }

            return component;
        }


        public double[] GenerateComponent(double r2,double[]oldcomponent)
        {
            var result = new double[oldcomponent.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (upperbound[i] - lowerbound[i]) * r2 + lowerbound[i];
            }

            return result;
        }
        public (double[],double[]) GeneratenMirrorAndComponent(double r3,double[]oldcomponent, double[]newglobal)
        {
            var newmirror=new double[oldcomponent.Length];
            var newcomponent= new double[oldcomponent.Length];
            for(int i=0;i<oldcomponent.Length;i++)
            {
                newmirror[i]=r3*oldcomponent[i]+(1-r3)*newglobal[i];
                newcomponent[i]=2*newmirror[i]-oldcomponent[i];
            }
            return (newmirror,newcomponent);
        }
        public double[] Generatenewglobal(int seed, double[]oldglobal)
        {
            var result= new double[oldglobal.Length];
           for(int i=0;i<oldglobal.Length;i++)
           {
               var rn=Normal.Sample(new MersenneTwister(seed+10,true), 0.0, 1.0); 
               result[i]=rn*0.01*(upperbound[i]-lowerbound[i]);
           }
           return result;
            
        }
        public double[] GenerateComposition(double r2)
        {
            var result= new double[lowerbound.Length];
            for(int i=0;i<lowerbound.Length;i++)
            {
                result[i]=lowerbound[i]+(upperbound[i]-lowerbound[i])*r2;
            }
            return result;
        }
      
       
    }
}
