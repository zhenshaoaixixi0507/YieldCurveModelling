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

            var yields = Data["2020-04-17"];
            var tau = new double[12] { (double)1/12, (double)2/12, (double)3/12, (double)6/12,1,2,3,5,7,10,20,30};

            // Test--------------------------------------- Static NS 3 factors model------------------------------------------------//
            var NS3factorCalibration = new StaticNS3FactorModelCalibration();
            NS3factorCalibration.yields = yields;
            NS3factorCalibration.maturities = tau;
            var optimziedpara = NS3factorCalibration.Calibration();
            var modeloutput = NS3factorCalibration.CalculateModelOutput(tau, optimziedpara);
            Console.WriteLine("NS 3 Factor Model Is Calibrated.");
            //Plot
            var plt = new ScottPlot.Plot(600, 400);
            plt.PlotSignalXY(tau, yields, color: Color.Red, label: "Market Data");
            plt.PlotSignalXY(tau, modeloutput, color: Color.Blue, label: "Model Output");
            plt.Legend();
            plt.XLabel("Time to maturity");
            plt.YLabel("Annualized yields (%)");
            var savepath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Pictures\\NS3FactorModelEmpiricalResult.png"));
            plt.SaveFig(savepath);
            Process.Start(savepath);

            // Test--------------------------------------- Static NS 4 factors model------------------------------------------------//
            var NS4factorCalibration = new StaticNS4FactorModelCalibration();
            NS4factorCalibration.yields = yields;
            NS4factorCalibration.maturities = tau;
            var optimziedpara2 = NS4factorCalibration.Calibration();
            var modeloutput2 = NS4factorCalibration.CalculateModelOutput(tau, optimziedpara2);
            Console.WriteLine("NS 4 Factor Model Is Calibrated.");
            //Plot
            var plt2 = new ScottPlot.Plot(600, 400);
            plt2.PlotSignalXY(tau, yields, color: Color.Red, label: "Market Data");
            plt2.PlotSignalXY(tau, modeloutput2, color: Color.Blue, label: "Model Output");
            plt2.Legend();
            plt2.XLabel("Time to maturity");
            plt2.YLabel("Annualized yields (%)");
            var savepath2 = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "Pictures\\NS4FactorModelEmpiricalResult.png"));
            plt2.SaveFig(savepath2);
            Process.Start(savepath2);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
