using System;
using Extreme.Mathematics.EquationSolvers;
using Extreme.Mathematics;
using Extreme.Mathematics.Algorithms;
using ILNumerics;
using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;
using ILNumerics.Toolboxes;
using static LEModel.LEModel;
using static InputDefinition.ForPython.ModelParams;
using static OneLegStrategy;
using static ILNumerics.ILMath;
using static ILNumerics.Globals;
using System.IO;
using System.Diagnostics;

namespace FuncApprox
{

    class Program
    {

        static void Main(string[] args)
        {
            var dt = 0.1;
            var timeToControl = 1;
            //var maxRisk = 5.0e-2;
            //var numberOfPoints = new int[] { 50, 25, 15 } ;

            var pressureGridFileName = @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\maps\pressuresGridLast.mat";
            //SurfacePressureGridCreator.createPressureGrid(dt, timeToControl, numberOfPoints, maxRisk, pressureGridFileName);

            var riskFileName = @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\maps\risks.mat";
            var timeFileName = @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\maps\times.mat";
            //SurfacePressureGridCreator.pressureTableToDisk(dt, timeToControl, pressureGridFileName, riskFileName, timeFileName);
            //Console.WriteLine("Done");
            //Array<double>[] risks = SurfacePressureGridCreator.loadGrid(riskFileName, new string[] { "r0", "r1", "r2" });


            var surfaceMapValues = SurfacePressureGridCreator.getSurfaceMapsFromDisk(pressureGridFileName, riskFileName, timeFileName);

            //var pressures = surfaceMapValues.Item1;
            //var risks = surfaceMapValues.Item2;

            Stopwatch s = new Stopwatch();

            //var adimMapper = new Kriging1DAdimMapper(pressures[0], risks[0]);
            s.Start();
            var surfaceApproximator = new SurfaceMapper(surfaceMapValues);
            var initPressures = new double[] { 5.2, 1.535, 1.2 } ;
            double approxRisk;

            for (int i =0; i< 10; i++)
                approxRisk = surfaceApproximator.EstimateRisk(initPressures);

            Console.WriteLine(s.ElapsedMilliseconds);
            s.Restart();

            Tuple<double, double> exactSolution;

            for(int i = 0; i<10; i++)
                exactSolution = getSurfaceRiskNTimeWithInitPress(dt, timeToControl, initPressures);
            
            Console.WriteLine(s.ElapsedMilliseconds);
            Console.WriteLine("results");

            //var approxTime = surfaceApproximator.EstimateTime(initPressures);
            //var exactTime = exactSolution.Item2;
            //Console.WriteLine(approxTime - exactTime);
        
            //Console.WriteLine(adimMapper.EstimateMapValue(5.513));

            // third tissue
            //Array<double> pressure0 = reshape<double>(pressures[0], 1, 50);
            //Array<double> risk0 = reshape<double>(risks[0], 1, 50);

            //var initPress = (double) pressure0[0];
            //Console.WriteLine(pressure0);
            //Console.WriteLine();
            //Console.WriteLine(risk0);


            //Console.WriteLine(pressure0.S);


            //KrigingInterpolator test = new KrigingInterpolator(risk0, pressure0);
            //var interpolator0 = CreateInterpolator(pressure0, risk0  );


            ////var riskEstimate = interpolator0.Apply(3.8);
            //Console.WriteLine(riskEstimate);

            //Console.WriteLine("one d mapper example");

            //var adimMapper = new AdimMapper(pressure0);

            //Array<double> adimVec = adimMapper.GetAdim(pressure0);

            //new KrigingInterpolatorDouble(Y, X);

            //////Console.WriteLine(response);
            //X[0] = linspace(0, pi, 10);
            //X[1] = cos(X[0]);

            //var interpolator = CreateInterpolator(X, Y);

            //Console.WriteLine(convertedString);
            //File.WriteAllText(@"C:\Users\glddm\Documents\Duke\test.txt", convertedString);
            //Console.WriteLine("saved");
            //var fileName = @"C:\Users\glddm\Documents\Duke\test.txt";

            //var interpolator = JsonConvert.DeserializeObject<KrigingInterpolator>(serialString);
            //Console.WriteLine("interpolator loaded");
            //Array<double> Yinterp = interpolator.Apply(X / 2.0, null);

            //Console.WriteLine(X);
            //Console.WriteLine();
            //Console.WriteLine(Y);
            //Console.WriteLine(Yinterp - cos(X / 2.0));
            ////Console.Rsead();


            //Array<double> outValues = interp.Apply(x_half) ;
            //Console.WriteLine("Comparison");
            //Console.WriteLine(outValues - Yinterp);

            Console.Read();

        }

        







        public static KrigingInterpolatorDouble CreateInterpolator(Array<double> X, Array<double>Y)
        {
            return new KrigingInterpolatorDouble(Y, X);
        }

    }

    
    
    
}
