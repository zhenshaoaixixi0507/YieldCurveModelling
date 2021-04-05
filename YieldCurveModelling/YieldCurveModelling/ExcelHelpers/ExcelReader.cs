using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Excel=Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace YieldCurveModelling.ExcelHelpers
{
    public enum ExcelOneDType{Row,Column }
    public class ExcelReader
    {
        public string filepath { get; set; }
        public string tabname { get; set; }
        public string rangename { get; set; }
        public ExcelOneDType type{get;set;}

        public double[] Get1DData()
        {
            Excel.Application oExcel = new Excel.Application();
            oExcel.Visible = false;
            oExcel.DisplayAlerts = false;
            Excel.Workbook WB = oExcel.Workbooks.Open(filepath);
            Excel.Worksheet wks = (Excel.Worksheet)WB.Worksheets[tabname];
            Excel.Range rng = wks.Range[rangename];
            int num = 0;
            num = type == ExcelOneDType.Row ? rng.Rows.Count : rng.Columns.Count;

            var result = new double[num];
            if (type == ExcelOneDType.Row)
            {
                for (int i = 0; i < num; i++)
                {
                    result[i] = (double)rng[i+1, 1].Value;
                }
            }
            if (type == ExcelOneDType.Column)
            {
                for (int i = 0; i < num; i++)
                {
                    result[i] = (double)rng[1, i+1].Value;
                }
            }

            WB.Close();
            oExcel.Quit();

            Marshal.ReleaseComObject(rng);
            Marshal.ReleaseComObject(wks);
            Marshal.ReleaseComObject(WB);
            Marshal.ReleaseComObject(oExcel);

            return result;
        }
        public double[,] Get2DData()
        {
            Excel.Application oExcel = new Excel.Application();
            oExcel.Visible = false;
            oExcel.DisplayAlerts = false;
            Excel.Workbook WB = oExcel.Workbooks.Open(filepath);
            Excel.Worksheet wks = (Excel.Worksheet)WB.Worksheets[tabname];
            Excel.Range rng = wks.Range[rangename];
            

            var result = new double[rng.Rows.Count,rng.Columns.Count];
            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    result[i, j] = (double)rng[i + 1, j + 1].Value;
                }
            }

            WB.Close();
            oExcel.Quit();

            Marshal.ReleaseComObject(rng);
            Marshal.ReleaseComObject(wks);
            Marshal.ReleaseComObject(WB);
            Marshal.ReleaseComObject(oExcel);

            return result;
        }
    }
}
