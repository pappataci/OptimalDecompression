using System;
using ILNumerics;
using ILNumerics.Toolboxes;
using static ILNumerics.ILMath;

namespace FuncApprox
{
    public class SurfaceMapper
    {

        public SurfaceMapper(Tuple<Array<double>[], Array<double>[], Array<double>[]> rawData)
        {
            Array<double>[] pressures = rawData.Item1;
            Array<double>[] risks = rawData.Item2;
            Array<double>[] times = rawData.Item3;

            var riskMapper = new Kringing3DAdimMapper(pressures, risks);
            var timeMapper = new Kringing3DAdimMapper(pressures, times);
        }
    }

    public class Kringing3DAdimMapper
    {
        private Array<double>[] pressures;
        private Array<double>[] times;

        public Kringing3DAdimMapper(Array<double>[] pressures, Array<double>[] times)
        {

            this.pressures = pressures;
            this.times = times;
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
            var adimX = xFieldMapper.dimToAdim(dimensionalX);
            var adimOutput = (double) interpolator.Apply(adimX);
            return yFieldMapper.adimToDim(adimOutput);
        }

    }

    public class AdimMapper
    {
        private double range, minValue;

        public AdimMapper(Array<double> xField)
        {
            range = getRange(xField);
            minValue = (double)xField[0];
        }

        public static double getRange(Array<double> vec)
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

        public double adimToDim(double adimValue)
        {
            return adimValue * range + minValue;
        }

        public double dimToAdim(double dimValue)
        {
            return (dimValue - minValue) / range;
        }

    }
}
