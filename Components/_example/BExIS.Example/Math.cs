using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Example
{
    public class Math
    {
        private NumberHelper _numberHelper;

        public Math()
        { 
            _numberHelper = new NumberHelper();
        }

        public Math(NumberHelper numberHelper)
        {
            _numberHelper = new NumberHelper();
        }

        public int Add(int a, int b)
        {
            if(a < 0) return -1;
            if(b < 0) return -1;

            return a + b;
        }

        public int Sub(int a, int b)
        {
            return a - b;
        }

        public int Multiply(int a, int b)
        {
            return a * b;
        }

        public double Divide(int a, int b)
        {
            return a/b;
        }

        public int Pow_MH (int a, int b)
        {
            if (b < 0) 
                return b;

            if(b == 0)
                return 1;
           
            int tmp = 0;
            for(int i = 0; i < b ;i++ )
            {
                if(i==0)
                {
                    tmp = a;
                }
                else
                {
                    tmp = tmp * a;
                }
            }

            return tmp;
        }
        
        public int AddWithCheck(int a, int b)
        {
            if (_numberHelper.IsNumber(a) &&
                _numberHelper.IsNumber(b))
            {

                if (a < 0) return -1;
                if (b < 0) return -1;


                return a + b;
            }

            return -1;
        }

        public int AddWithEvenCheck(int a, int b) {
            int aEven = IsEven(a);
            int bEven = IsEven(b);

            if (aEven == 0) return -1;
            if (bEven == 0) return -1;

            return a + b;
        }

        public int IsEven(int x) {
            return 1 - (x % 2);
        }
    }
}
