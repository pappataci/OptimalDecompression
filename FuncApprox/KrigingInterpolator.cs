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
    public class KrigingInterpolator 
    {
        public KrigingInterpolatorDouble interpolator;

        public KrigingInterpolator(Array<double> X, Array<double> Y)
        {
            interpolator = new KrigingInterpolatorDouble(Y, X);
        }

        public Array<double> Apply(Array<double> X )
        {
            Console.WriteLine("test");
            return interpolator.Apply(X);
        }

    }
}
