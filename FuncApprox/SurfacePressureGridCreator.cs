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



namespace FuncApprox
{
    public class SurfacePressureGridCreator
    {

        private static Func<double, double> getPressRiskForThisTissue( double maxRisk, int tissue, double dt, int timeToControl)
        {
            return maxPress => {
                var riskAndTime = getRiskAndTimeForThisTissueAtDepth( maxPress, tissue, dt, timeToControl);
                var risk = riskAndTime.Item1;

                return (risk - maxRisk);
            };
        }

        private static double getPressAtMaxRiskForTissueAndDepth(  double maxRisk, int tissue, double dt, int timeToControl)
        {
            var solver = new BisectionSolver();
            solver.LowerBound = 0.7;
            solver.UpperBound = 12.0;
            solver.TargetFunction = getPressRiskForThisTissue( maxRisk, tissue, dt, timeToControl);
            var solution = solver.Solve();
            return solution;
        }

        private static double[] getPressureGridForThisTissue(int tissue, double depth, double dt, int timeToControl, double maxRisk, int numberOfPoints)
        {
            var pressMin = computeZeroRiskPressureForThisTissueAtDepth(depth, threshold[tissue]);
            var pressMax = getPressAtMaxRiskForTissueAndDepth( maxRisk, tissue, dt, timeToControl);
            Array<double> pressureGrid = linspace(pressMin, pressMax, numberOfPoints);
            double[] outArray = null;
            pressureGrid.ExportValues(ref outArray);
            return outArray;
        }

        private static double[][] getPressuresGridAtDepth(double depth, double dt, int timeToControl, double maximumRiskBound, int[] numberOfPoints)
        {
            double[][] pressureAtMaxRiskGrid = new double[threshold.Length][];
            for (int tissue = 0; tissue < threshold.Length; tissue++)
            {
                Console.WriteLine("solving tissue " + tissue);
                pressureAtMaxRiskGrid[tissue] = getPressureGridForThisTissue(tissue, depth, dt, timeToControl, maximumRiskBound, numberOfPoints[tissue]);
            }
            return pressureAtMaxRiskGrid;
        }
        private static void dumpVarsToDiskWithTheseNames(double[][] pressuresAtMaxRiskGrid, string[] names, string pressureGridFileName)
        {
            Array<double> actualPressureGrid;
            using (MatFile mat = new MatFile())
            {
                for (int index = 0; index < names.Length; index++)
                {
                    actualPressureGrid = vector(pressuresAtMaxRiskGrid[index]);
                    mat.AddArray(actualPressureGrid, names[index]);
                }
                mat.Write(pressureGridFileName);
            }
        }

        public static void createPressureGrid(double dt, int timeToControl, int[] numberOfPoints, double maxRisk, string pressureGridFileName)
        {
            var pressuresAtMaxRiskGrid = getPressuresGridAtDepth(0.0, dt, timeToControl, maxRisk, numberOfPoints);
            dumpVarsToDiskWithTheseNames(pressuresAtMaxRiskGrid, new string[] { "p0", "p1", "p2" }, pressureGridFileName);
        }

        public static Array<double>[] loadGrid(string pressureGridFileName , string[] varNames)
        {
            Array<double> []  P = new Array<double>[ 3 ];
            using (var back = new MatFile(pressureGridFileName))
            {
                for (int i = 0; i < varNames.Length; i++)
                    { P[i] = back.GetArray<double>(varNames[i]); }
            }
            return P;
        }

        public static double[] setInitPressures(double pressureOfTissue, int tissueIndex, int numberOfTissues)
        {
            var pressOut = new double[numberOfTissues];
            for (int iTissue = 0; iTissue < numberOfTissues; iTissue++)
            {
                pressOut[iTissue] = 0.7; // arbitrary zero risk value
            }
            pressOut[tissueIndex] = pressureOfTissue;
            return pressOut;
        }

        public static Tuple<double[][], double[][]> createSurfaceMapsForAllComportments(double dt, int timeToControl, string pressureGridFileName )
        {
            var pressureGrid = loadGrid(pressureGridFileName, new string[] { "p0", "p1", "p2" });            
            double[][] risks = new double[pressureGrid.Length][];
            double[][] times = new double[pressureGrid.Length][];

            double[] initPressure = new double[pressureGrid.Length];

            for (int tissueIndex = 0; tissueIndex < pressureGrid.Length; tissueIndex++)
            {
                Console.WriteLine("mapping tissue " + tissueIndex);
                Array<double> pressGridForThisTissue = pressureGrid[tissueIndex];
                risks[tissueIndex] = new double[pressGridForThisTissue.Length];
                times[tissueIndex] = new double[pressGridForThisTissue.Length];

                for (int elementIndex = 0; elementIndex < pressGridForThisTissue.Length; elementIndex++)
                {
                    double actualInitPress = (double) pressGridForThisTissue[elementIndex];
                    initPressure = setInitPressures(actualInitPress, tissueIndex,  pressureGrid.Length);
                    var riskAndTime = getSurfaceRiskNTimeWithInitPress(dt, timeToControl, initPressure  );
                    var risk = riskAndTime.Item1;
                    var time = riskAndTime.Item2;
                    risks[tissueIndex][elementIndex] = risk;
                    times[tissueIndex][elementIndex] = time;   
                }
            }
            return Tuple.Create(risks, times);
        }

        public static void pressureTableToDisk( double dt, int timeToControl, string pressureGridFileName , string riskFileName, string timeFileName)
        {
            var risksAndTimeMaps = createSurfaceMapsForAllComportments(dt, timeToControl, pressureGridFileName);
            var risks = risksAndTimeMaps.Item1;
            var times = risksAndTimeMaps.Item2;
            dumpVarsToDiskWithTheseNames(risks, new string[] { "r0", "r1", "r2" }, riskFileName);
            dumpVarsToDiskWithTheseNames(times, new string[] { "t0", "t1", "t2" }, timeFileName);
        }

        public static Tuple<Array<double>[] , Array<double>[] , Array<double>[] > getSurfaceMapsFromDisk(string pressureGridFile, string riskFile, string timeFile)
        {
            Array<double>[] pressures = SurfacePressureGridCreator.loadGrid(pressureGridFile, new string[] { "p0", "p1", "p2" });
            Array<double>[] risks = SurfacePressureGridCreator.loadGrid(riskFile, new string[] { "r0", "r1", "r2" });
            Array<double>[] times = SurfacePressureGridCreator.loadGrid(timeFile, new string[] { "t0", "t1", "t2" });
            return Tuple.Create(pressures, risks, times);
        }

    }
}
