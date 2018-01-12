using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dataNormalizer
{
    class Program
    {
        static bool Normalize()
        {
            InsuranceCompaniesNormalizer innorm = new InsuranceCompaniesNormalizer();
            return innorm.Normalize();             
        }

        static int Main(string[] args)
        {
            if (Normalize())
                return 0;
            else
                return -1;
        }
    }
}
