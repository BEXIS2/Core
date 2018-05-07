using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.IO.Transform.Input
{
    public class ExcelHelper
    {

        public static double FromExcelSerialDate(double SerialDate)
        {
            if (SerialDate > 59) SerialDate -= 1; //Excel/Lotus 2/29/1900 bug   
            return new DateTime(1899, 12, 31).AddDays(SerialDate).ToOADate();
        }
    }
}
