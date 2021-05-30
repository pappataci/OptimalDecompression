using System;
using Extreme.Mathematics.EquationSolvers;
using Extreme.Mathematics;
using Extreme.Mathematics.Algorithms;
using ILNumerics;
using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;
using ILNumerics.Toolboxes;
using Newtonsoft.Json;
using static LEModel.LEModel;
using static InputDefinition.ForPython.ModelParams;
using static OneLegStrategy;
using static ILNumerics.ILMath;
using static ILNumerics.Globals;
using System.IO;

namespace FuncApprox
{

    class Program
    {

        static void Main(string[] args)
        {
            //var dt = 0.1;
            //var timeToControl = 1;
            //var maxRisk = 5.0e-2;
            //var depth = 0.0; var numberOfPoints = 500;

            //var answer = getPressureGridForThisTissue(0, depth, dt, timeToControl, maxRisk, numberOfPoints);
            //foreach (double element in answer)
            //{
            //    Console.WriteLine(element);
            //}


            //var a = computeZeroRiskPressureForThisTissueAtDepth(0.0, threshold[0]);

            ////Console.WriteLine(response);
            //Array<double> X = linspace(0, pi, 10);
            //Array<double> Y = cos(X);

            //var interpolator = CreateInterpolator(X, Y);
            //Console.WriteLine("passed");

            //var convertedString = JsonConvert.SerializeObject(X);

            //Console.WriteLine(convertedString);
            //File.WriteAllText(@"C:\Users\glddm\Documents\Duke\test.txt", convertedString);
            //Console.WriteLine("saved");
            //var fileName = @"C:\Users\glddm\Documents\Duke\test.txt";

            Array<double> X;
            Array<double> Y;

            var matFileName = @"C:\Users\glddm\Documents\Duke\test.mat";
            //ArrayTool.ToDisk(X, fileName);
            //var serialString = File.ReadAllText(@"C:\Users\glddm\Documents\Duke\test.txt");
            //var test = JsonConvert.DeserializeObject<double[]>(serialString);
            //Console.WriteLine(test);
            //Array<double> il = vector(test);
            //using (MatFile mat = new MatFile())
            //{
            //    mat.AddArray(X, "X");
            //    mat.AddArray(Y, "Y");
            //    mat.Write(matFileName);
            //}
            //Console.WriteLine("passed");
            //Console.WriteLine("done");

            using (var back = new MatFile(matFileName))
            {
                 X = back.GetArray<double>("X");

                // ... or usign cell methods: 
                Y = back.GetArray<double>("Y");

               
            }
            Console.WriteLine(X);
            Console.WriteLine();
            Console.WriteLine(Y);

            //var interpolator = JsonConvert.DeserializeObject<KrigingInterpolator>(serialString);
            //Console.WriteLine("interpolator loaded");
            //Array<double> Yinterp = interpolator.Apply(X / 2.0, null);

            //Console.WriteLine(X);
            //Console.WriteLine();
            //Console.WriteLine(Y);
            //Console.WriteLine(Yinterp - cos(X / 2.0));
            ////Console.Rsead();


            //Console.WriteLine("Using Custom Class");
            //var interp = new KrigingInterpolator(X, Y);
            //Array<double> x_half = X / 2.0;

            //Array<double> outValues = interp.Apply(x_half) ;
            //Console.WriteLine("Comparison");
            //Console.WriteLine(outValues - Yinterp);

            Console.Read();

        }

        private static double[][] getPressuresGridAtDepth(double depth, double dt,int timeToControl, double maximumRiskBound, int  numberOfPoints)
        {
            double[][] pressureAtMaxRiskGrid = new double[threshold.Length][];
            for (int tissue = 0; tissue < threshold.Length; tissue++)
            {
                pressureAtMaxRiskGrid[tissue] = getPressureGridForThisTissue(tissue, depth, dt, timeToControl, maximumRiskBound, numberOfPoints);
            }
            return pressureAtMaxRiskGrid;
        }

        private static Func<double,double> getPressRiskForThisTissue(double depth, double maxRisk, int tissue, double dt, int timeToControl)
        {
            return maxPress => {
                var riskAndTime = getRiskAndTimeForThisTissueAtDepth(depth, maxPress, tissue, dt, timeToControl);
                var risk = riskAndTime.Item1;

                return ( risk - maxRisk ) ; };
        }

        private static double getPressAtMaxRiskForTissueAndDepth(double depth, double maxRisk, int tissue, double dt, int timeToControl)
        {
            var solver = new BisectionSolver();
            solver.LowerBound = 0.7;
            solver.UpperBound = 12.0;
            solver.TargetFunction = getPressRiskForThisTissue(depth, maxRisk, tissue, dt, timeToControl);
            var solution = solver.Solve();
            Console.WriteLine(solver.EvaluationsNeeded); 
            return solution;
        }

        private static double[] getPressureGridForThisTissue(int tissue, double depth, double dt, int timeToControl, double maxRisk, int numberOfPoints)
        {
            var pressMin = computeZeroRiskPressureForThisTissueAtDepth(depth, threshold[tissue]);
            var pressMax = getPressAtMaxRiskForTissueAndDepth(depth, maxRisk, tissue, dt, timeToControl);
            Array<double> pressureGrid = linspace(pressMin, pressMax, numberOfPoints);
            double[] outArray = null;
            pressureGrid.ExportValues(ref outArray);
            return outArray;
        }


        public static KrigingInterpolatorDouble CreateInterpolator(Array<double> X, Array<double>Y)
        {
            return new KrigingInterpolatorDouble(Y, X);
        }

    }

    
    
    
}
