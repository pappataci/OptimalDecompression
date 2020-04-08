using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSharpWrapperToPython.Class1;

namespace Testingf
{
    class Program
    {
        static void Main(string[] args)
        {
            var n = 5;
            var aValue = 1.2;

            var outFromFSharp = createVectorFromFSharp(aValue, n);
            //Console.WriteLine(outFromFSharp[0]);
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine(outFromFSharp[i]);
            }

            Console.Read();

        }
    }
}
