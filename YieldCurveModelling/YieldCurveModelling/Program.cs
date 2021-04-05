using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YieldCurveModelling.ExcelHelpers;
using YieldCurveModelling.YieldCurveModels;

namespace YieldCurveModelling
{
    class Program
    {
        static void Main(string[] args)
        {
            //var excelreader = new ExcelReader();
            //excelreader.filepath = @"C:\Users\jingz\source\repos\YieldCurveModelling\YieldCurveModelling\YieldCurveModelling\Data\USD Yield Curve.xlsx";
            //excelreader.tabname = "2021";
            //excelreader.rangename = "B64:M64";
            //excelreader.type = ExcelOneDType.Row;
            //var yields = excelreader.Get1DData();
            var tau = new double[12] { (double)1/12, (double)2/12, (double)3/12, (double)6/12,1,2,3,5,7,10,20,30};
            var yields = new double[12] {0.02,0.02,0.02,0.04, 0.07, 0.19,0.39,0.97,1.42,1.72,2.27,2.35};
            var NS3factorCalibration = new StaticNS3FactorModelCalibration();
            NS3factorCalibration.yields = yields;
            NS3factorCalibration.maturities = tau;
            var optimziedpara = NS3factorCalibration.Calibration();
            var modeloutput = NS3factorCalibration.CalculateModelOutput(tau, optimziedpara);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
