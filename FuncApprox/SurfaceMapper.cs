using System;
using ILNumerics;
using ILNumerics.Toolboxes;
using static ILNumerics.ILMath;
using System.Linq;

namespace FuncApprox
{
    public class SurfaceMapper
    {
        private Kringing3DAdimMapper riskMapper;
        private Kringing3DAdimMapper timeMapper;

        public SurfaceMapper(Tuple<Array<double>[], Array<double>[], Array<double>[]> rawData)
        {
            Array<double>[] pressures = rawData.Item1;
            Array<double>[] risks = rawData.Item2;
            Array<double>[] times = rawData.Item3;

            Func<double[], double> sumComponents = x => x.Sum();
            Func<double[], double> getMax = x => x.Max();

            riskMapper = new Kringing3DAdimMapper(pressures, risks, sumComponents);
            timeMapper = new Kringing3DAdimMapper(pressures, times, getMax);
        }

        public double EstimateRisk(double[] pressures)
        {
            return riskMapper.SurfaceApproximateOutput(pressures);
        }

        public double EstimateTime(double[] pressures)
        {
            return timeMapper.SurfaceApproximateOutput(pressures);
        }

    }

    public class Kringing3DAdimMapper
    {
        private Kriging1DAdimMapper[] adimMappers;
        private Func<double[], double> computationAggregator;

        public Kringing3DAdimMapper(Array<double>[] pressures, Array<double>[] dependentVar, Func<double[], double> aggregator)
        {
            adimMappers = new Kriging1DAdimMapper[pressures.Length];
            for (int iTissue = 0; iTissue < pressures.Length; iTissue++)
            {
                adimMappers[iTissue] = new Kriging1DAdimMapper(pressures[iTissue], dependentVar[iTissue]);
            }
            computationAggregator = aggregator;
        }

        public double SurfaceApproximateOutput(double[] independentVars)
        {
            var tissueValues = new double[independentVars.Length];
            for (int iTissue = 0; iTissue < independentVars.Length; iTissue++)
            {
                tissueValues[iTissue] = Math.Max( adimMappers[iTissue].EstimateMapValue(independentVars[iTissue]) , 0.0) ;
            }
            return computationAggregator(tissueValues)  ;
        }
    }

    public class Kriging1DAdimMapper
    {
        private AdimMapper xFieldMapper;
        private AdimMapper yFieldMapper;
        private KrigingInterpolatorDouble interpolator;
        public Kriging1DAdimMapper(Array<double> xField, Array<double> yField)
        {
            xFieldMapper = new AdimMapper(xField);
            yFieldMapper = new AdimMapper(yField);

            Array<double> xAdimField = xFieldMapper.GetAdim(xField);
            Array<double> yAdimField = yFieldMapper.GetAdim(yField);
            interpolator = new KrigingInterpolatorDouble(yAdimField, xAdimField);

        }

        public double EstimateMapValue(double dimensionalX)
        {
            var adimX = xFieldMapper.DimToAdim(dimensionalX);
            var adimOutput = (double) interpolator.Apply(adimX);
            return yFieldMapper.AdimToDim(adimOutput);
        }

    }

    public class AdimMapper
    {
        private double range, minValue;

        public AdimMapper(Array<double> xField)
        {
            range = GetRange(xField);
            minValue = (double)xField[0];
        }

        public static double GetRange(Array<double> vec)
        {
            var endValue = (double)vec["end"];
            double initValue = (double)vec[0];
            return endValue - initValue;
        }

        public  RetArray<double> GetAdim(InArray<double> dimVector)
        {
            using(Scope.Enter(dimVector))
            {
                Array<double> actualVector = reshape<double>(dimVector, 1, dimVector.Length);
                return ((actualVector - minValue) / range);
            }
        }

        public double AdimToDim(double adimValue)
        {
            return adimValue * range + minValue;
        }

        public double DimToAdim(double dimValue)
        {
            return (dimValue - minValue) / range;
        }

    }
}
