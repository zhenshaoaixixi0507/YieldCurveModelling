using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace YieldCurveModelling.Helpers
{
    public class USYieldDataXMLReader
    {
        public string filepath;

        public Dictionary<string, double[]> GetFullTimeSeriesData()
        {
            var result = new Dictionary<string, double[]>();
            var xmldoc = new XmlDataDocument();
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
            var datenode = xmldoc.GetElementsByTagName("d:NEW_DATE");
            
            var onemonthnode = xmldoc.GetElementsByTagName("d:BC_1MONTH");
            var twomonthnode = xmldoc.GetElementsByTagName("d:BC_2MONTH");
            var threemonthnode = xmldoc.GetElementsByTagName("d:BC_3MONTH");
            var sixmonthnode = xmldoc.GetElementsByTagName("d:BC_6MONTH");
            var oneyearnode = xmldoc.GetElementsByTagName("d:BC_1YEAR");
            var twoyearnode = xmldoc.GetElementsByTagName("d:BC_2YEAR");
            var threeyearnode = xmldoc.GetElementsByTagName("d:BC_3YEAR");
            var fiveyearnode = xmldoc.GetElementsByTagName("d:BC_5YEAR");
            var sevenyearnode = xmldoc.GetElementsByTagName("d:BC_7YEAR");
            var tenyearnode = xmldoc.GetElementsByTagName("d:BC_10YEAR");
            var twentyyearnode = xmldoc.GetElementsByTagName("d:BC_20YEAR");
            var thirtyyearnode = xmldoc.GetElementsByTagName("d:BC_30YEAR");
            var temp = new double[12];
            for (int i = 0; i <= datenode.Count - 1; i++)
            {
                var datestr = datenode[i].ChildNodes.Item(0).InnerText;
                datestr = datestr.Remove(datestr.Length - 9);
                temp[0] = Convert.ToDouble(onemonthnode[i].ChildNodes.Item(0).InnerText);
                temp[1] = Convert.ToDouble(twomonthnode[i].ChildNodes.Item(0).InnerText);
                temp[2] = Convert.ToDouble(threemonthnode[i].ChildNodes.Item(0).InnerText);
                temp[3] = Convert.ToDouble(sixmonthnode[i].ChildNodes.Item(0).InnerText);
                temp[4] = Convert.ToDouble(oneyearnode[i].ChildNodes.Item(0).InnerText);
                temp[5] = Convert.ToDouble(twoyearnode[i].ChildNodes.Item(0).InnerText);
                temp[6] = Convert.ToDouble(threeyearnode[i].ChildNodes.Item(0).InnerText);
                temp[7] = Convert.ToDouble(fiveyearnode[i].ChildNodes.Item(0).InnerText);
                temp[8] = Convert.ToDouble(sevenyearnode[i].ChildNodes.Item(0).InnerText);
                temp[9] = Convert.ToDouble(tenyearnode[i].ChildNodes.Item(0).InnerText);
                temp[10] = Convert.ToDouble(twentyyearnode[i].ChildNodes.Item(0).InnerText);
                temp[11] = Convert.ToDouble(thirtyyearnode[i].ChildNodes.Item(0).InnerText);
                result.Add(datestr, temp.Clone() as double[]);
            }
            return result;
        }
    }
}
