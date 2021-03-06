using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YieldCurveModelling.ExcelHelpers;
using YieldCurveModelling.Helpers;
using YieldCurveModelling.YieldCurveModels;
using ScottPlot;
using System.Drawing;
using ScottPlot.Drawing;
using System.Diagnostics;
using System.Xml;

namespace YieldCurveModelling
{
    class Program
    {
        static void Main(string[] args)
        {

            //Read data
            var USDataReader = new USYieldDataXMLReader();
            USDataReader.filepath= Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Data\\USDYieldCurveDailyData2020.xml"));
            var Data = USDataReader.GetFullTimeSeriesData();

            var yields = Data["2020-02-11"];
            var tau = new double[12] { (double)1/12, (double)2/12, (double)3/12, (double)6/12,1,2,3,5,7,10,20,30};

            //// Test--------------------------------------- Static NS 3 factors model------------------------------------------------//
            //var NS3factorCalibration = new StaticNS3FactorModelCalibration();
            //NS3factorCalibration.yields = yields;
            //NS3factorCalibration.maturities = tau;
            //var optimziedpara = NS3factorCalibration.Calibration();
            //var modeloutput = NS3factorCalibration.CalculateModelOutput(tau, optimziedpara);
            //Console.WriteLine("NS 3 Factor Model Is Calibrated.");
            ////Plot
            //var plt = new ScottPlot.Plot(600, 400);
            //plt.PlotSignalXY(tau, yields, color: Color.Red, label: "Market Data");
            //plt.PlotSignalXY(tau, modeloutput, color: Color.Blue, label: "Model Output");
            //plt.Legend();
            //plt.XLabel("Time to maturity");
            //plt.YLabel("Annualized yields (%)");
            //var savepath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Pictures\\NS3FactorModelEmpiricalResult.png"));
            //plt.SaveFig(savepath);
            //Process.Start(savepath);

            //// Test--------------------------------------- Static NS 4 factors model------------------------------------------------//
            //var NS4factorCalibration = new StaticNS4FactorModelCalibration();
            //NS4factorCalibration.yields = yields;
            //NS4factorCalibration.maturities = tau;
            //var optimziedpara2 = NS4factorCalibration.Calibration();
            //var modeloutput2 = NS4factorCalibration.CalculateModelOutput(tau, optimziedpara2);
            //Console.WriteLine("NS 4 Factor Model Is Calibrated.");
            ////Plot
            //var plt2 = new ScottPlot.Plot(600, 400);
            //plt2.PlotSignalXY(tau, yields, color: Color.Red, label: "Market Data");
            //plt2.PlotSignalXY(tau, modeloutput2, color: Color.Blue, label: "Model Output");
            //plt2.Legend();
            //plt2.XLabel("Time to maturity");
            //plt2.YLabel("Annualized yields (%)");
            //var savepath2 = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Pictures\\NS4FactorModelEmpiricalResult.png"));
            //plt2.SaveFig(savepath2);
            //Process.Start(savepath2);
            //// Test--------------------------------------- Static Vasicek Two factors model------------------------------------------------//
            //var V2FactorCal = new StaticVasicekTwoFactorModelCalibration();
            //V2FactorCal.yields = yields;
            //V2FactorCal.maturities = tau;
            //var V2FactorOptPara = V2FactorCal.Calibration();
            //var V2Factor = new StaticVasicekTwoFactorModel();
            //V2Factor.maturities = tau;
            //var modeloutputV2 = V2FactorCal.CalcualteModelOutput(V2FactorOptPara);
            //Console.WriteLine("Vasicek 2 Factor Model Is Calibrated.");
            ////Plot
            //var pltv2 = new ScottPlot.Plot(600, 400);
            //pltv2.PlotSignalXY(tau, yields, color: Color.Red, label: "Market Data");
            //pltv2.PlotSignalXY(tau, modeloutputV2, color: Color.Blue, label: "Model Output");
            //pltv2.Legend();
            //pltv2.XLabel("Time to maturity");
            //pltv2.YLabel("Annualized yields (%)");
            //var savepathV2 = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Pictures\\VasicekTwoFactorModelEmpiricalResult.png"));
            //pltv2.SaveFig(savepathV2);
            //Process.Start(savepathV2);
            // Test--------------------------------------- Static Longstaff Schwartz Two factors model------------------------------------------------//
            var LS2FactorCal = new StaticTwoFactorLongstaffSchwartzModelCalibration();
            LS2FactorCal.yields = yields;
            LS2FactorCal.maturities = tau;
            var LS2FactorOptPara = LS2FactorCal.Calibration();
            var LS2Factor = new StaticTwoFactorLongstaffSchwartzModel();
            LS2Factor.maturities = tau;
            var modeloutputLS2 = LS2FactorCal.CalcualteModelOutput(LS2FactorOptPara);
            Console.WriteLine("Longstaff Schwartz 2 Factor Model Is Calibrated.");
            //Plot
            var pltls2 = new ScottPlot.Plot(600, 400);
            pltls2.PlotSignalXY(tau, yields, color: Color.Red, label: "Market Data");
            pltls2.PlotSignalXY(tau, modeloutputLS2, color: Color.Blue, label: "Model Output");
            pltls2.Legend();
            pltls2.XLabel("Time to maturity");
            pltls2.YLabel("Annualized yields (%)");
            var savepathls2 = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Pictures\\LongstaffSchwartzTwoFactorModelEmpiricalResult.png"));
            pltls2.SaveFig(savepathls2);
            Process.Start(savepathls2);

            ////Test dynamic NS3 factor model
            //var DynaimcNS3factor = new DynamicNS3FactorModelCalibration();
            //DynaimcNS3factor.yields = Data;
            //DynaimcNS3factor.maturities = tau;
            //var optimziedpara=DynaimcNS3factor.Optimize();
            //(var tvbeta1, var tvbeta2, var tvbeta3) = DynaimcNS3factor.GetDynamicBetas(optimziedpara);
            ////Plot
            //// Beta1
            //var pltbeta1 = new ScottPlot.Plot(600, 400);
            //pltbeta1.PlotSignal(tvbeta1, color: Color.Red, label: "Time-varying Beta1");
            //pltbeta1.Legend();
            //pltbeta1.XLabel("History Time");
            //pltbeta1.YLabel("Beta1");
            //var savepathbeta1 = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Pictures\\DynamicNS3FactorModelBeta1.png"));
            //pltbeta1.SaveFig(savepathbeta1);
            //Process.Start(savepathbeta1);
            //// Beta2
            //var pltbeta2 = new ScottPlot.Plot(600, 400);
            //pltbeta2.PlotSignal(tvbeta2, color: Color.Red, label: "Time-varying Beta2");
            //pltbeta2.Legend();
            //pltbeta2.XLabel("History Time");
            //pltbeta2.YLabel("Beta2");
            //var savepathbeta2 = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Pictures\\DynamicNS3FactorModelBeta2.png"));
            //pltbeta2.SaveFig(savepathbeta2);
            //Process.Start(savepathbeta2);
            //// Beta3
            //var pltbeta3 = new ScottPlot.Plot(600, 400);
            //pltbeta3.PlotSignal(tvbeta3, color: Color.Red, label: "Time-varying Beta3");
            //pltbeta3.Legend();
            //pltbeta3.XLabel("History Time");
            //pltbeta3.YLabel("Beta3");
            //var savepathbeta3 = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Pictures\\DynamicNS3FactorModelBeta3.png"));
            //pltbeta3.SaveFig(savepathbeta3);
            //Process.Start(savepathbeta3);

            ////Test dynamic NS4 factor model
            //var DynaimcNS4factor = new DynamicNS4FactorModelCalibration();
            //DynaimcNS4factor.yields = Data;
            //DynaimcNS4factor.maturities = tau;
            //var optimziedpara = DynaimcNS4factor.Optimize();
            //(var tvbeta1, var tvbeta2, var tvbeta3, var tvbeta4) = DynaimcNS4factor.GetDynamicBetas(optimziedpara);
            ////Plot
            //// Beta1
            //var pltbeta1 = new ScottPlot.Plot(600, 400);
            //pltbeta1.PlotSignal(tvbeta1, color: Color.Red, label: "Time-varying Beta1");
            //pltbeta1.Legend();
            //pltbeta1.XLabel("History Time");
            //pltbeta1.YLabel("Beta1");
            //var savepathbeta1 = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Pictures\\DynamicNS3FactorModelBeta1.png"));
            //pltbeta1.SaveFig(savepathbeta1);
            //Process.Start(savepathbeta1);
            //// Beta2
            //var pltbeta2 = new ScottPlot.Plot(600, 400);
            //pltbeta2.PlotSignal(tvbeta2, color: Color.Red, label: "Time-varying Beta2");
            //pltbeta2.Legend();
            //pltbeta2.XLabel("History Time");
            //pltbeta2.YLabel("Beta2");
            //var savepathbeta2 = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Pictures\\DynamicNS3FactorModelBeta2.png"));
            //pltbeta2.SaveFig(savepathbeta2);
            //Process.Start(savepathbeta2);
            //// Beta3
            //var pltbeta3 = new ScottPlot.Plot(600, 400);
            //pltbeta3.PlotSignal(tvbeta3, color: Color.Red, label: "Time-varying Beta3");
            //pltbeta3.Legend();
            //pltbeta3.XLabel("History Time");
            //pltbeta3.YLabel("Beta3");
            //var savepathbeta3 = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Pictures\\DynamicNS3FactorModelBeta3.png"));
            //pltbeta3.SaveFig(savepathbeta3);
            //Process.Start(savepathbeta3);
            //// Beta3
            //var pltbeta4 = new ScottPlot.Plot(600, 400);
            //pltbeta4.PlotSignal(tvbeta4, color: Color.Red, label: "Time-varying Beta4");
            //pltbeta4.Legend();
            //pltbeta4.XLabel("History Time");
            //pltbeta4.YLabel("Beta4");
            //var savepathbeta4 = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Pictures\\DynamicNS3FactorModelBeta4.png"));
            //pltbeta4.SaveFig(savepathbeta4);
            //Process.Start(savepathbeta4);


            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
