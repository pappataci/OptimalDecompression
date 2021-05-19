using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNumerics;
using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;
using ILNumerics.Toolboxes;
using static ILNumerics.ILMath;
using static ILNumerics.Globals;
namespace FuncApprox
{
    class Program
    {
        static void Main(string[] args)
        {
            Array<double> Y = 1;
            Array<double> X  = Computation.Generate1DData(10, Y);

            Array<double> Z = linspace(0.0, pi, 100);

            Array<double> values = Interpolation.kriging(Y , X  , Z);

            
            Console.WriteLine(X);
            Console.WriteLine(values);
        }

        private class Computation
        {
            public static RetArray<double> Generate1DData(int len, OutArray<double> Y)
            {
                using (Scope.Enter())
                {
                    Array<double> X = linspace(0.0, pi, len);
                    Y.a = sin(X);
                    return X;
                }
            }
        }

    }

    
    
    
}
