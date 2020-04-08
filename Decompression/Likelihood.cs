namespace Decompression
{
    /// <summary>
    /// Likelihood and log likelihood functions
    /// </summary>
    public static class Likelihood
    {
#if false
        public static double CalculateLogLikelihood(double[] dvVariable, DiveDataCondition<ProfileCondition<NodeCondition>, NodeCondition> d)
        {
            // d.UpdateModelVariables(m, dvVariable);

            d.RecalculateProfileTissueValues(m);

            if (DiveProfile.UsePSI && !DiveProfile.Tetranomial)
            {
                double[] dvP0 = new double[d.Profiles]; // probability of no DCS
                double[] dvPM = new double[d.Profiles]; // probability of mild DCS
                double[] dvPS = new double[d.Profiles]; // probability of serious DCS
                d.DCSTrinomialProbability(dvP0, dvPM, dvPS, m);
                int iBad = new int();
                return d.TrinomialLogLikelihood(dvP0, dvPM, dvPS, out iBad);
            }
            else if (DiveProfile.UsePSI && DiveProfile.Tetranomial)
            {
                // return d.OptimalGainTetranomialTimeOfOnsetOptimizeReturnLogLikelihood(m);
#warning change back
                double[] dvP0 = new double[d.Profiles]; // probability of no DCS
                double[] dvPN = new double[d.Profiles]; // probability of niggles
                double[] dvPM = new double[d.Profiles]; // probability of mild DCS
                double[] dvPS = new double[d.Profiles]; // probability of serious DCS
                d.DCSTetranomialProbability(dvP0, dvPN, dvPM, dvPS, m);
                return d.TetranomialLogLikelihood(dvP0, dvPN, dvPM, dvPS);

                //FileStream fs = new FileStream("tetranomial_log_likelihood.csv", FileMode.Append, FileAccess.Write, FileShare.None);
                //StreamWriter sw = new StreamWriter(fs);
                //for (int i = 0; i < dvP0.Length; i++)
                //{
                //    string s = dvP0[i].ToString() + "," + dvPN[i].ToString() + "," + dvPM[i].ToString() + "," + dvPS[i].ToString() + Environment.NewLine;

                //}
                //sw.Close();
                //fs.Close();
            }
            else if (m == MODEL.LE1_OG)
            {
                if (DCSDiveData.DiveData.FractionalMarginals && !EE1ntDiveData.UseFailureTimes)
                {
                    d.OptimalGainBinaryFractionalIncedenceOnlyOptimize(m);
                    return d.OptimalGainBinaryFractionalIncidenceOnlyLogLikelihood(m);
                }
                else if (DCSDiveData.DiveData.FractionalMarginals && EE1ntDiveData.UseFailureTimes)
                {
                    d.OptimalGainBinaryFractionalTimeOfOnsetOptimize(m);
                    return d.OptimalGainBinaryFractionalTimeOfOnsetLogLikelihood(m);
                }
                else
                {
                    return 0.0;
                }
            }
            else
            {
                double[] dvP0 = new double[d.Profiles]; // P(0) - probability of survival to the right censored time
                double[] dvPE = new double[d.Profiles]; // P(E) - probability of an event for the interval censored failure time
                d.EE1ntProbability(dvP0, dvPE, m);
                return d.DualLogLikelihood(dvP0, dvPE);
            }
        }

        public static double CalculateLogLikelihood ( MODEL m, DiveDataCondition<ProfileCondition<NodeCondition>, NodeCondition> d )
        {
            d.RecalculateProfileTissueValues(m);

            if (DiveProfile.UseMBG)
            {
                double[] dvP0 = new double[d.Profiles]; // probability of lowBG
                double[] dvPE = new double[d.Profiles]; // probability of highBG
                d.EE1ntProbability(dvP0, dvPE, m);
                return d.DualLogLikelihood(dvP0, dvPE);
            }
            else if (DiveProfile.UsePSI && !DiveProfile.Tetranomial)
            {
                double[] dvP0 = new double[d.Profiles]; // probability of no DCS
                double[] dvPM = new double[d.Profiles]; // probability of mild DCS
                double[] dvPS = new double[d.Profiles]; // probability of serious DCS
                d.DCSTrinomialProbability(dvP0, dvPM, dvPS, m);
                int iBad = new int();
                return d.TrinomialLogLikelihood(dvP0, dvPM, dvPS, out iBad);
            }
            else if (DiveProfile.UsePSI && DiveProfile.Tetranomial)
            {
                return d.OptimalGainTetranomialTimeOfOnsetOptimizeReturnLogLikelihood(m);
                //double[] dvP0 = new double[d.Profiles]; // probability of no DCS
                //double[] dvPN = new double[d.Profiles]; // probability of niggles
                //double[] dvPM = new double[d.Profiles]; // probability of mild DCS
                //double[] dvPS = new double[d.Profiles]; // probability of serious DCS
                //d.DCSTetranomialProbability(dvP0, dvPN, dvPM, dvPS, m);
                //return d.TetranomialLogLikelihood(dvP0, dvPN, dvPM, dvPS);
            }
            else
            {
                if (m == MODEL.LE1_OG)
                {
                    if (DCSDiveData.DiveData.FractionalMarginals && !EE1ntDiveData.UseFailureTimes)
                    {
                        d.OptimalGainBinaryFractionalIncedenceOnlyOptimize(m);
                        return d.OptimalGainBinaryFractionalIncidenceOnlyLogLikelihood(m);
                    }
                    else if (DCSDiveData.DiveData.FractionalMarginals && EE1ntDiveData.UseFailureTimes)
                    {
                        d.OptimalGainBinaryFractionalTimeOfOnsetOptimize(m);
                        return d.OptimalGainBinaryFractionalTimeOfOnsetLogLikelihood(m);
                    }
                    else
                    {
                        return 0.0;
                    }
                }
                else
                {
                    double[] dvP0 = new double[d.Profiles]; // P(0) - probability of survival to the right censored time
                    double[] dvPE = new double[d.Profiles]; // P(E) - probability of an event for the interval censored failure time
                    d.EE1ntProbability(dvP0, dvPE, m);
                    return d.DualLogLikelihood(dvP0, dvPE);
                }
            }
        }
#endif
    }
}