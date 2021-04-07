using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YieldCurveModelling.ExcelHelpers;
using YieldCurveModelling.YieldCurveModels;
using ScottPlot;
using System.Drawing;
using ScottPlot.Drawing;
using System.Diagnostics;

namespace YieldCurveModelling
{
    class Program
    {
        static void Main(string[] args)
        {
            var excelreader = new ExcelReader();
            string path = Directory.GetCurrentDirectory();
         
            excelreader.filepath = Path.GetFullPath(Path.Combine(path, @"..\..\", "Data\\USD Yield Curve.xlsx"));//@"C:\Users\jingz\source\repos\YieldCurveModelling\YieldCurveModelling\YieldCurveModelling\Data\USD Yield Curve.xlsx";
            excelreader.tabname = "2021";
            excelreader.rangename = "B64:M64";
            excelreader.type = ExcelOneDType.Column;
            var yields = excelreader.Get1DData();
            var tau = new double[12] { (double)1/12, (double)2/12, (double)3/12, (double)6/12,1,2,3,5,7,10,20,30};
            //var yields = new double[12] {0.02,0.02,0.02,0.04, 0.07, 0.19,0.39,0.97,1.42,1.72,2.27,2.35};
            // Test--------------------------------------- NS 3 factors model------------------------------------------------//
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
            var savepath = Path.GetFullPath(Path.Combine(path, @"..\..\", "Pictures\\NS3FactorModelEmpiricalResult.png"));
            plt.SaveFig(savepath);
            Process.Start(savepath);

            // Test--------------------------------------- NS 4 factors model------------------------------------------------//
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
            var savepath2 = Path.GetFullPath(Path.Combine(path, @"..\..\", "Pictures\\NS4FactorModelEmpiricalResult.png"));
            plt2.SaveFig(savepath2);
            Process.Start(savepath2);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
