using System;
using DCSUtilities;

namespace Decompression
{
    public static class ExactGainTetranomial
    {
        public static OPTIMIZATIONTYPE OptimizationType = OPTIMIZATIONTYPE.TIMEOFONSETFRACTIONALMARGINAL;

        private static double [ ] m_dvGain;
        private static DiveDataCondition<ProfileCondition<NodeCondition>, NodeCondition> m_Data;
        private static int m_iNumberOfRestarts = 32;
        private static double [ ] dvScale = new double [ NodeTissue.NumberOfTissues ];
        private static double m_dTrinomialScaleFactor;
        private static double m_dTetranomialScaleFactor;

        public static double [ ] Gain { set { m_dvGain = value; } get { return m_dvGain; } }

        public static int Restarts { set { m_iNumberOfRestarts = value; } get { return m_iNumberOfRestarts; } }

        public static double TrinomialScaleFactor { set { m_dTrinomialScaleFactor = value; } get { return m_dTrinomialScaleFactor; } }

        public static double TetranomialScaleFactor { set { m_dTetranomialScaleFactor = value; } get { return m_dTetranomialScaleFactor; } }

        public static DiveDataCondition<ProfileCondition<NodeCondition>, NodeCondition> Data { set { m_Data = value; } get { return m_Data; } }

        public static double [ ] Optimize ( )
        {
            throw new NotImplementedException ( "Decompression.ExactGainTetranomial.Optimize ( ) not yet implemented" );
            //return m_dvGain;
        }

#if false

        public double EOTetranomialOptimize()
        {
            MInternalM = MODEL.LE1NT_TETRA;

            m_dvR01 = new double[m_dvGain.Length, this.Profiles];
            m_dvR12 = new double[m_dvGain.Length, this.Profiles];
            m_dvR03 = new double[m_dvGain.Length, this.Profiles];

            OptimalGainTetranomialTimeOfOnsetHazard(m_dvR01, m_dvR12, m_dvR03);

            MInternalM = MODEL.LE1NT_TETRA;

            MultivariateRealFunction f = new MultivariateRealFunction(fOptimalGainTetranomialTimeOfOnsetLogLikelihood);
            FastMultivariateVectorFunction g = new FastMultivariateVectorFunction(gOptimalGainTetranomialTimeOfOnsetLogLikelihood);
            GeneralVector initialGuess = new GeneralVector(m_dvGain[0], m_dvGain[1], m_dvGain[2], m_dACoefficient, m_dBCoefficient);
            QuasiNewtonOptimizer bfgs = new QuasiNewtonOptimizer(QuasiNewtonMethod.Bfgs);
            bfgs.InitialGuess = initialGuess;
            bfgs.ExtremumType = ExtremumType.Maximum;
            bfgs.ObjectiveFunction = f;
            bfgs.FastGradientFunction = g;

            bfgs.FindExtremum();

            GeneralVector sol = bfgs.Extremum;
            double dError = bfgs.EstimatedError;
            int its = bfgs.IterationsNeeded;
            int fevals = bfgs.EvaluationsNeeded;
            int gevals = bfgs.GradientEvaluationsNeeded;
            return 1.0;
        }

        public void OptimalGainTetranomialTimeOfOnsetHazard(double[,] dvR01, double[,] dvR12, double[,] dvR03, MODEL m)
        { // verified 20090222
            double[] dvTemp = new double[m_dvGain.Length];

            this.RecalculateProfileTissueValues(m);

            for (int i = 0; i < this.Profiles; i++)
            {
                EE1ntDiveProfile p = this.Entry(i);

                dvTemp = p.CalculateArbitraryTimeIntegratedRisk(this.IntegrationLimit(i), this.m_dvLESwitchOverPressure, this.m_dvThreshold, m);

                for (int n = 0; n < dvTemp.Length; n++)
                {
                    dvR01[n, i] = dvR12[n, i] = 0.0;
                    dvR03[n, i] = dvTemp[n];
                }

                if (bUseFailureTime)
                {
                    if (p.IsAnyDCS)
                    {
                        if (p.GoodTimes)
                        {
                            dvTemp = p.CalculateArbitraryTimeIntegratedRisk(p.Time1, this.m_dvLESwitchOverPressure, this.m_dvThreshold, m);

                            for (int n = 0; n < dvTemp.Length; n++)
                                dvR01[n, i] = dvTemp[n];

                            dvTemp = p.CalculateArbitraryTimeIntegratedRisk(p.Time2, this.m_dvLESwitchOverPressure, this.m_dvThreshold, m);

                            for (int n = 0; n < dvTemp.Length; n++)
                                dvR12[n, i] = dvTemp[n] - dvR01[n, i];
                        }
                        else
                        {
                            dvTemp = p.CalculateArbitraryTimeIntegratedRisk(this.FirstDecompressionStartTime(i), this.m_dvLESwitchOverPressure, this.m_dvThreshold, m);

                            for (int n = 0; n < dvTemp.Length; n++)
                            {
                                dvR01[n, i] = dvTemp[n];
                                dvR12[n, i] = dvR03[n, i] - dvR01[n, i];
                            }
                        }
                    } // if p.GoodTimes
                } // if bUseFailureTimes
            } // for i
        } // verified

        public void OptimalGainTetranomialTimeOfOnsetHazard(double[,] dvR01, double[,] dvR12, double[,] dvR03)
        { // verified 20090222
            double[] dvTemp = new double[m_dvGain.Length];

            this.RecalculateProfileTissueValues(MInternalM);

            for (int i = 0; i < this.Profiles; i++)
            {
                EE1ntDiveProfile p = this.Entry(i);

                dvTemp = p.CalculateArbitraryTimeIntegratedRisk(this.IntegrationLimit(i), this.m_dvLESwitchOverPressure, this.m_dvThreshold, MInternalM);

                for (int n = 0; n < dvTemp.Length; n++)
                {
                    dvR01[n, i] = dvR12[n, i] = 0.0;
                    dvR03[n, i] = dvTemp[n];
                }

                if (bUseFailureTime)
                {
                    if (p.IsAnyDCS)
                    {
                        if (p.GoodTimes)
                        {
                            dvTemp = p.CalculateArbitraryTimeIntegratedRisk(p.Time1, this.m_dvLESwitchOverPressure, this.m_dvThreshold, MInternalM);

                            for (int n = 0; n < dvTemp.Length; n++)
                                dvR01[n, i] = dvTemp[n];

                            dvTemp = p.CalculateArbitraryTimeIntegratedRisk(p.Time2, this.m_dvLESwitchOverPressure, this.m_dvThreshold, MInternalM);

                            for (int n = 0; n < dvTemp.Length; n++)
                                dvR12[n, i] = dvTemp[n] - dvR01[n, i];
                        }
                        else
                        {
                            dvTemp = p.CalculateArbitraryTimeIntegratedRisk(this.FirstDecompressionStartTime(i), this.m_dvLESwitchOverPressure, this.m_dvThreshold, MInternalM);

                            for (int n = 0; n < dvTemp.Length; n++)
                            {
                                dvR01[n, i] = dvTemp[n];
                                dvR12[n, i] = dvR03[n, i] - dvR01[n, i];
                            }
                        }
                    } // if p.GoodTimes
                } // if bUseFailureTimes
            } // for i
        } // verified

        public double OptimalGainTetranomialTimeOfOnsetLogLikelihood(MODEL m)
        { // verified 20090222
            // see equation ?? in Howle's decompression sickness 1 lab notebook

            double[,] dvR01 = new double[m_dvGain.Length, this.Profiles];
            double[,] dvR12 = new double[m_dvGain.Length, this.Profiles];
            double[,] dvR03 = new double[m_dvGain.Length, this.Profiles];
            double dLL = new double();

            OptimalGainTetranomialTimeOfOnsetHazard(dvR01, dvR12, dvR03, m);

            for (int i = 0; i < this.Profiles; i++)
            {
                double dTerm01 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();

                for (int j = 0; j < m_dvGain.Length; j++)
                {
                    dTerm01 -= m_dvGain[j] * dvR01[j, i];
                    dTerm12 -= m_dvGain[j] * dvR12[j, i];
                    dTerm03 -= m_dvGain[j] * dvR03[j, i];
                }

                double dXi01 = new double();
                double dXiA01 = new double();
                double dXiB01 = new double();
                double dXi12 = new double();
                double dXiA12 = new double();
                double dXiB12 = new double();
                double dXi03 = new double();
                double dXiA03 = new double();
                double dXiB03 = new double();

                dXi01 = Math.Exp(dTerm01);
                dXiA01 = Math.Exp(m_dACoefficient * dTerm01);
                dXiB01 = Math.Exp(m_dBCoefficient * dTerm01);
                dXi12 = Math.Exp(dTerm12);
                dXiA12 = Math.Exp(m_dACoefficient * dTerm12);
                dXiB12 = Math.Exp(m_dBCoefficient * dTerm12);
                dXi03 = Math.Exp(dTerm03);
                dXiA03 = Math.Exp(m_dACoefficient * dTerm03);
                dXiB03 = Math.Exp(m_dBCoefficient * dTerm03);

                EE1ntDiveProfile p = this.Entry(i);

                if (p.PSISeriousDCS)
                {
                    dLL += p.Divers * Math.Log(((1 - dXiA12) * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)));
                }
                else if (p.PSIMildDCS)
                {
                    dLL += p.Divers * Math.Log(((1 - dXi12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)));
                }
                else if (p.IsMarginalDCS)
                {
                    dLL += p.Divers * Math.Log(((1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)));
                }
                else
                {
                    dLL += p.Divers * Math.Log((dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03));
                }
            } // for i

            return dLL;
        } // verified

        public double fOptimalGainTetranomialTimeOfOnsetLogLikelihood(Vector VParams)
        {
            double dLL = new double();

            // load the upload the parameter vector components
            for (int i = 0; i < m_dvGain.Length; i++)
                m_dvGain[i] = VParams[i];

            m_dACoefficient = VParams[m_dvGain.Length];
            m_dBCoefficient = VParams[m_dvGain.Length + 1];

            for (int i = 0; i < this.Profiles; i++)
            {
                double dTerm01 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();

                for (int j = 0; j < m_dvGain.Length; j++)
                {
                    dTerm01 -= m_dvGain[j] * m_dvR01[j, i];
                    dTerm12 -= m_dvGain[j] * m_dvR12[j, i];
                    dTerm03 -= m_dvGain[j] * m_dvR03[j, i];
                }

                double dXi01 = new double();
                double dXiA01 = new double();
                double dXiB01 = new double();
                double dXi12 = new double();
                double dXiA12 = new double();
                double dXiB12 = new double();
                double dXi03 = new double();
                double dXiA03 = new double();
                double dXiB03 = new double();

                dXi01 = Math.Exp(dTerm01);
                dXiA01 = Math.Exp(m_dACoefficient * dTerm01);
                dXiB01 = Math.Exp(m_dBCoefficient * dTerm01);
                dXi12 = Math.Exp(dTerm12);
                dXiA12 = Math.Exp(m_dACoefficient * dTerm12);
                dXiB12 = Math.Exp(m_dBCoefficient * dTerm12);
                dXi03 = Math.Exp(dTerm03);
                dXiA03 = Math.Exp(m_dACoefficient * dTerm03);
                dXiB03 = Math.Exp(m_dBCoefficient * dTerm03);

                EE1ntDiveProfile p = this.Entry(i);

                if (p.PSISeriousDCS)
                {
                    dLL += p.Divers * Math.Log(((1 - dXiA12) * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)));
                }
                else if (p.PSIMildDCS)
                {
                    dLL += p.Divers * Math.Log(((1 - dXi12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)));
                }
                else if (p.IsMarginalDCS)
                {
                    dLL += p.Divers * Math.Log(((1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)));
                }
                else
                {
                    dLL += p.Divers * Math.Log((dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03));
                }
            } // for i

            return dLL;
        } // verified

        public Vector gOptimalGainTetranomialTimeOfOnsetLogLikelihood(Vector VParams, Vector f)
        {
            if (f == null)
                f = new GeneralVector(5);

            // upload the parameters
            for (int i = 0; i < m_dvGain.Length; i++)
                m_dvGain[i] = VParams[i];

            m_dACoefficient = VParams[m_dvGain.Length];
            m_dBCoefficient = VParams[m_dvGain.Length + 1];

            double[] dvG = OptimalGainTetranomialTimeOfOnsetGainSlope(m_dvR01, m_dvR12, m_dvR03);

            for (int i = 0; i < m_dvGain.Length; i++)
                f[i] = dvG[i];

            f[m_dvGain.Length] = OptimalGainTetranomialTimeOfOnsetASlope(m_dvR01, m_dvR12, m_dvR03);
            f[m_dvGain.Length + 1] = OptimalGainTetranomialTimeOfOnsetBSlope(m_dvR01, m_dvR12, m_dvR03);

            return f;
        }

        public double OptimalGainTetranomialTimeOfOnsetLogLikelihood(MODEL m, double[,] dvR01, double[,] dvR12, double[,] dvR03)
        { // verified 20090222
            double dLL = new double();

            for (int i = 0; i < this.Profiles; i++)
            {
                double dTerm01 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();

                for (int j = 0; j < m_dvGain.Length; j++)
                {
                    dTerm01 -= m_dvGain[j] * dvR01[j, i];
                    dTerm12 -= m_dvGain[j] * dvR12[j, i];
                    dTerm03 -= m_dvGain[j] * dvR03[j, i];
                }

                double dXi01 = new double();
                double dXiA01 = new double();
                double dXiB01 = new double();
                double dXi12 = new double();
                double dXiA12 = new double();
                double dXiB12 = new double();
                double dXi03 = new double();
                double dXiA03 = new double();
                double dXiB03 = new double();

                dXi01 = Math.Exp(dTerm01);
                dXiA01 = Math.Exp(m_dACoefficient * dTerm01);
                dXiB01 = Math.Exp(m_dBCoefficient * dTerm01);
                dXi12 = Math.Exp(dTerm12);
                dXiA12 = Math.Exp(m_dACoefficient * dTerm12);
                dXiB12 = Math.Exp(m_dBCoefficient * dTerm12);
                dXi03 = Math.Exp(dTerm03);
                dXiA03 = Math.Exp(m_dACoefficient * dTerm03);
                dXiB03 = Math.Exp(m_dBCoefficient * dTerm03);

                EE1ntDiveProfile p = this.Entry(i);

                if (p.PSISeriousDCS)
                {
                    dLL += p.Divers * Math.Log(((1 - dXiA12) * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)));
                }
                else if (p.PSIMildDCS)
                {
                    dLL += p.Divers * Math.Log(((1 - dXi12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)));
                }
                else if (p.IsMarginalDCS)
                {
                    dLL += p.Divers * Math.Log(((1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)));
                }
                else
                {
                    dLL += p.Divers * Math.Log((dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03));
                }
            } // for i

            return dLL;
        } // verified

        public double OptimalGainTetranomialTimeOfOnsetASlope(double[,] dvR01, double[,] dvR12, double[,] dvR03)
        { // verified 20090224
            double dSlope = new double();

            for (int i = 0; i < this.Profiles; i++)
            {
                double dTerm01 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();

                for (int j = 0; j < m_dvGain.Length; j++)
                {
                    dTerm01 -= m_dvGain[j] * dvR01[j, i];
                    dTerm12 -= m_dvGain[j] * dvR12[j, i];
                    dTerm03 -= m_dvGain[j] * dvR03[j, i];
                }

                double dXi01 = new double();
                double dXiA01 = new double();
                double dXiB01 = new double();
                double dXi12 = new double();
                double dXiA12 = new double();
                double dXiB12 = new double();
                double dXi03 = new double();
                double dXiA03 = new double();
                double dXiB03 = new double();

                dXi01 = Math.Exp(dTerm01);
                dXiA01 = Math.Exp(m_dACoefficient * dTerm01);
                dXiB01 = Math.Exp(m_dBCoefficient * dTerm01);
                dXi12 = Math.Exp(dTerm12);
                dXiA12 = Math.Exp(m_dACoefficient * dTerm12);
                dXiB12 = Math.Exp(m_dBCoefficient * dTerm12);
                dXi03 = Math.Exp(dTerm03);
                dXiA03 = Math.Exp(m_dACoefficient * dTerm03);
                dXiB03 = Math.Exp(m_dBCoefficient * dTerm03);

                EE1ntDiveProfile p = this.Entry(i);

                if (p.PSISeriousDCS)
                {
                    dSlope += (double)p.Divers * (-dTerm12 * dXiA12 * (dXiA01 - (1.0 - dXi01) * dXiA01 - (1.0 - dXiB01) * (1.0 - (1.0 - dXi01) * dXiA01) * dXiA01) + (1.0 - dXiA12) * (dTerm01 * dXiA01 - (1.0 - dXi01)
                        * dTerm01 * dXiA01 + (1.0 - dXiB01) * (1.0 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1.0 - dXiB01) * (1.0 - (1.0 - dXi01) * dXiA01) * dTerm01 * dXiA01)) / (1.0 - dXiA12) / (dXiA01 - (1.0 - dXi01)
                        * dXiA01 - (1.0 - dXiB01) * (1.0 - (1.0 - dXi01) * dXiA01) * dXiA01);
                }
                else if (p.PSIMildDCS)
                {
                    dSlope += (double)p.Divers * (((1.0 - dXi12) * dTerm12 * dXiA12 * (dXiA01 - (1.0 - dXi01) * dXiA01 - (1.0 - dXiB01) * (1.0 - (1.0 - dXi01) * dXiA01) * dXiA01) + (1.0 - dXi12) * dXiA12 * (dTerm01
                        * dXiA01 - (1.0 - dXi01) * dTerm01 * dXiA01 + (1.0 - dXiB01) * (1.0 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1.0 - dXiB01) * (1.0 - (1.0 - dXi01) * dXiA01) * dTerm01 * dXiA01)) / (1.0 - dXi12)
                        / dXiA12 / (dXiA01 - (1.0 - dXi01) * dXiA01 - (1.0 - dXiB01) * (1.0 - (1.0 - dXi01) * dXiA01) * dXiA01));
                }
                else if (p.IsMarginalDCS)
                {
                    dSlope += (double)p.Divers * ((-(1.0 - dXiB12) * (1.0 - dXi12) * dTerm12 * dXiA12 * dXiA12 * (dXiA01 - (1.0 - dXi01) * dXiA01 - (1.0 - dXiB01) * (1.0 - (1.0 - dXi01) * dXiA01) * dXiA01) + (1.0 - dXiB12)
                        * (1.0 - (1.0 - dXi12) * dXiA12) * dTerm12 * dXiA12 * (dXiA01 - (1.0 - dXi01) * dXiA01 - (1.0 - dXiB01) * (1.0 - (1.0 - dXi01) * dXiA01) * dXiA01) + (1.0 - dXiB12) * (1.0 - (1.0 - dXi12) * dXiA12)
                        * dXiA12 * (dTerm01 * dXiA01 - (1.0 - dXi01) * dTerm01 * dXiA01 + (1.0 - dXiB01) * (1.0 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1.0 - dXiB01) * (1.0 - (1.0 - dXi01) * dXiA01) * dTerm01 * dXiA01))
                        / (1.0 - dXiB12) / (1.0 - (1.0 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1.0 - dXi01) * dXiA01 - (1.0 - dXiB01) * (1.0 - (1.0 - dXi01) * dXiA01) * dXiA01));
                }
                else
                {
                    dSlope += (double)p.Divers * ((dTerm03 * dXiA03 - (1.0 - dXi03) * dTerm03 * dXiA03 + (1.0 - dXiB03) * (1.0 - dXi03) * dTerm03 * dXiA03 * dXiA03 - (1.0 - dXiB03) * (1.0 - (1.0 - dXi03) * dXiA03)
                        * dTerm03 * dXiA03) / (dXiA03 - (1.0 - dXi03) * dXiA03 - (1.0 - dXiB03) * (1.0 - (1.0 - dXi03) * dXiA03) * dXiA03));
                }
            } // for i

            return dSlope;
        } // verified

        public double OptimalGainTetranomialTimeOfOnsetACurvature(double[,] dvR01, double[,] dvR12, double[,] dvR03)
        { // unverified
            double dCurv = new double();

            for (int i = 0; i < this.Profiles; i++)
            {
                double dTerm01 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();

                for (int j = 0; j < m_dvGain.Length; j++)
                {
                    dTerm01 -= m_dvGain[j] * dvR01[j, i];
                    dTerm12 -= m_dvGain[j] * dvR12[j, i];
                    dTerm03 -= m_dvGain[j] * dvR03[j, i];
                }

                double dXi01 = new double();
                double dXiA01 = new double();
                double dXiB01 = new double();
                double dXi12 = new double();
                double dXiA12 = new double();
                double dXiB12 = new double();
                double dXi03 = new double();
                double dXiA03 = new double();
                double dXiB03 = new double();

                dXi01 = Math.Exp(dTerm01);
                dXiA01 = Math.Exp(m_dACoefficient * dTerm01);
                dXiB01 = Math.Exp(m_dBCoefficient * dTerm01);
                dXi12 = Math.Exp(dTerm12);
                dXiA12 = Math.Exp(m_dACoefficient * dTerm12);
                dXiB12 = Math.Exp(m_dBCoefficient * dTerm12);
                dXi03 = Math.Exp(dTerm03);
                dXiA03 = Math.Exp(m_dACoefficient * dTerm03);
                dXiB03 = Math.Exp(m_dBCoefficient * dTerm03);

                EE1ntDiveProfile p = this.Entry(i);

                if (p.PSISeriousDCS)
                    dCurv += p.Divers * ((-dTerm12 * dTerm12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - 2 * dTerm12 * dXiA12 * (dTerm01 * dXiA01 - (1 - dXi01)
                        * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01) + (1 - dXiA12) * (dTerm01 * dTerm01 * dXiA01 - (1 - dXi01)
                        * dTerm01 * dTerm01 * dXiA01 + 3 * (1 - dXiB01) * (1 - dXi01) * dTerm01 * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dTerm01 * dXiA01)) / (1 - dXiA12)
                        / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (-dTerm12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                        + (1 - dXiA12) * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01))
                        * (double)Math.Pow((double)(1 - dXiA12), (double)(-2)) / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * dTerm12 * dXiA12 - (-dTerm12 * dXiA12 * (dXiA01
                        - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiA12) * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01
                        * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01)) / (1 - dXiA12) * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01)
                        * dXiA01), (double)(-2)) * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01));
                else if (p.PSIMildDCS)
                    dCurv += p.Divers * (((1 - dXi12) * dTerm12 * dTerm12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + 2 * (1 - dXi12) * dTerm12 * dXiA12 * (dTerm01
                        * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01) + (1 - dXi12) * dXiA12 * (dTerm01
                        * dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dTerm01 * dXiA01 + 3 * (1 - dXiB01) * (1 - dXi01) * dTerm01 * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dTerm01
                        * dXiA01)) / (1 - dXi12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - ((1 - dXi12) * dTerm12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01
                        - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXi12) * dXiA12 * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01)) / (1 - dXi12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * dTerm12 - ((1 - dXi12) * dTerm12
                        * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXi12) * dXiA12 * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01)
                        * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01)) / (1 - dXi12) / dXiA12 * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * dTerm01 * dXiA01));
                else if (p.IsMarginalDCS)
                    dCurv += p.Divers * ((-3 * (1 - dXiB12) * (1 - dXi12) * dTerm12 * dTerm12 * dXiA12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - 2 * (1 - dXiB12)
                        * (1 - dXi12) * dTerm12 * dXiA12 * dXiA12 * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01)
                        * dTerm01 * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dTerm12 * dTerm12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + 2 * (1 - dXiB12)
                        * (1 - (1 - dXi12) * dXiA12) * dTerm12 * dXiA12 * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * dTerm01 * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dTerm01 * dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dTerm01 * dXiA01 + 3 * (1 - dXiB01) * (1 - dXi01) * dTerm01
                        * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dTerm01 * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01
                        - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (-(1 - dXiB12) * (1 - dXi12) * dTerm12 * dXiA12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                        + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dTerm12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12)
                        * dXiA12 * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01)) / (1 - dXiB12)
                        * (double)Math.Pow((double)(1 - (1 - dXi12) * dXiA12), (double)(-2)) / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * (1 - dXi12) * dTerm12 - (-(1 - dXiB12)
                        * (1 - dXi12) * dTerm12 * dXiA12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dTerm12 * dXiA12
                        * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01
                        + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01)
                        * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * dTerm12 - (-(1 - dXiB12) * (1 - dXi12) * dTerm12 * dXiA12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dTerm12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1
                        - (1 - dXi12) * dXiA12) * dXiA12 * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01
                        * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2))
                        * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01));
                else
                    dCurv += p.Divers * ((dTerm03 * dTerm03 * dXiA03 - (1 - dXi03) * dTerm03 * dTerm03 * dXiA03 + 3 * (1 - dXiB03) * (1 - dXi03) * dTerm03 * dTerm03 * dXiA03 * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03)
                        * dXiA03) * dTerm03 * dTerm03 * dXiA03) / (dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03) - (double)Math.Pow((double)(dTerm03 * dXiA03 - (1 - dXi03) * dTerm03
                        * dXiA03 + (1 - dXiB03) * (1 - dXi03) * dTerm03 * dXiA03 * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dTerm03 * dXiA03), (double)2) * (double)Math.Pow((double)(dXiA03 - (1 - dXi03)
                        * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03), (double)(-2)));
            } // for i

            return dCurv;
        } // verified

        public double OptimalGainTetranomialTimeOfOnsetBSlope(double[,] dvR01, double[,] dvR12, double[,] dvR03)
        { // verified 20090224
            double dSlope = new double();

            for (int i = 0; i < this.Profiles; i++)
            {
                double dTerm01 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();

                for (int j = 0; j < m_dvGain.Length; j++)
                {
                    dTerm01 -= m_dvGain[j] * dvR01[j, i];
                    dTerm12 -= m_dvGain[j] * dvR12[j, i];
                    dTerm03 -= m_dvGain[j] * dvR03[j, i];
                }

                double dXi01 = new double();
                double dXiA01 = new double();
                double dXiB01 = new double();
                double dXi12 = new double();
                double dXiA12 = new double();
                double dXiB12 = new double();
                double dXi03 = new double();
                double dXiA03 = new double();
                double dXiB03 = new double();

                dXi01 = Math.Exp(dTerm01);
                dXiA01 = Math.Exp(m_dACoefficient * dTerm01);
                dXiB01 = Math.Exp(m_dBCoefficient * dTerm01);
                dXi12 = Math.Exp(dTerm12);
                dXiA12 = Math.Exp(m_dACoefficient * dTerm12);
                dXiB12 = Math.Exp(m_dBCoefficient * dTerm12);
                dXi03 = Math.Exp(dTerm03);
                dXiA03 = Math.Exp(m_dACoefficient * dTerm03);
                dXiB03 = Math.Exp(m_dBCoefficient * dTerm03);

                EE1ntDiveProfile p = this.Entry(i);

                if (p.PSISeriousDCS)
                {
                    double temp = (dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01));
                    dSlope += (double)p.Divers * (dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01));
                }
                else if (p.PSIMildDCS)
                    dSlope += (double)p.Divers * (dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01));
                else if (p.IsMarginalDCS)
                    dSlope += (double)p.Divers * ((-dTerm12 * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12)
                        * (1 - (1 - dXi12) * dXiA12) * dXiA12 * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01
                        - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01));
                else
                    dSlope += (double)p.Divers * (dTerm03 * dXiB03 * (1 - (1 - dXi03) * dXiA03) * dXiA03 / (dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03));
            } // for i

            return dSlope;
        } // verified

        public double OptimalGainTetranomialTimeOfOnsetBCurvature(double[,] dvR01, double[,] dvR12, double[,] dvR03)
        {
            double dCurv = new double();

            for (int i = 0; i < this.Profiles; i++)
            {
                double dTerm01 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();

                for (int j = 0; j < m_dvGain.Length; j++)
                {
                    dTerm01 -= m_dvGain[j] * dvR01[j, i];
                    dTerm12 -= m_dvGain[j] * dvR12[j, i];
                    dTerm03 -= m_dvGain[j] * dvR03[j, i];
                }

                double dXi01 = new double();
                double dXiA01 = new double();
                double dXiB01 = new double();
                double dXi12 = new double();
                double dXiA12 = new double();
                double dXiB12 = new double();
                double dXi03 = new double();
                double dXiA03 = new double();
                double dXiB03 = new double();

                dXi01 = Math.Exp(dTerm01);
                dXiA01 = Math.Exp(m_dACoefficient * dTerm01);
                dXiB01 = Math.Exp(m_dBCoefficient * dTerm01);
                dXi12 = Math.Exp(dTerm12);
                dXiA12 = Math.Exp(m_dACoefficient * dTerm12);
                dXiB12 = Math.Exp(m_dBCoefficient * dTerm12);
                dXi03 = Math.Exp(dTerm03);
                dXiA03 = Math.Exp(m_dACoefficient * dTerm03);
                dXiB03 = Math.Exp(m_dBCoefficient * dTerm03);

                EE1ntDiveProfile p = this.Entry(i);

                if (p.PSISeriousDCS)
                    dCurv += p.Divers * (dTerm01 * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - dTerm01 * dTerm01
                        * dXiB01 * dXiB01 * (double)Math.Pow((double)(1 - (1 - dXi01) * dXiA01), (double)2) * dXiA01 * dXiA01 * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1
                        - dXi01) * dXiA01) * dXiA01), (double)(-2)));
                else if (p.PSIMildDCS)
                    dCurv += p.Divers * (dTerm01 * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - dTerm01 * dTerm01
                        * dXiB01 * dXiB01 * (double)Math.Pow((double)(1 - (1 - dXi01) * dXiA01), (double)2) * dXiA01 * dXiA01 * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1
                        - dXi01) * dXiA01) * dXiA01), (double)(-2)));
                else if (p.IsMarginalDCS)
                    dCurv += p.Divers * ((-dTerm12 * dTerm12 * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - 2 * dTerm12 * dXiB12
                        * (1 - (1 - dXi12) * dXiA12) * dXiA12 * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * dTerm01 * dTerm01 * dXiB01 * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (-dTerm12 * dXiB12 * (1 - (1
                        - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * dTerm01 * dXiB01 * (1
                        - (1 - dXi01) * dXiA01) * dXiA01) * (double)Math.Pow((double)(1 - dXiB12), (double)(-2)) / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01) * dTerm12 * dXiB12 - (-dTerm12 * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12)
                        * (1 - (1 - dXi12) * dXiA12) * dXiA12 * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 * (double)Math.Pow((double)(dXiA01 - (1 - dXi01)
                        * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01);
                else
                    dCurv += p.Divers * (dTerm03 * dTerm03 * dXiB03 * (1 - (1 - dXi03) * dXiA03) * dXiA03 / (dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03) - dTerm03 * dTerm03 * dXiB03
                        * dXiB03 * (double)Math.Pow((double)(1 - (1 - dXi03) * dXiA03), (double)2) * dXiA03 * dXiA03 * (double)Math.Pow((double)(dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03)
                        * dXiA03), (double)(-2)));
            } // for i

            return dCurv;
        } // verified

        public double OptimalGainTetranomialTimeOfOnsetABCrossDerivative(double[,] dvR01, double[,] dvR12, double[,] dvR03)
        {
            double dHess = new double();

            for (int i = 0; i < this.Profiles; i++)
            {
                double dTerm01 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();

                for (int j = 0; j < m_dvGain.Length; j++)
                {
                    dTerm01 -= m_dvGain[j] * dvR01[j, i];
                    dTerm12 -= m_dvGain[j] * dvR12[j, i];
                    dTerm03 -= m_dvGain[j] * dvR03[j, i];
                }

                double dXi01 = new double();
                double dXiA01 = new double();
                double dXiB01 = new double();
                double dXi12 = new double();
                double dXiA12 = new double();
                double dXiB12 = new double();
                double dXi03 = new double();
                double dXiA03 = new double();
                double dXiB03 = new double();

                dXi01 = Math.Exp(dTerm01);
                dXiA01 = Math.Exp(m_dACoefficient * dTerm01);
                dXiB01 = Math.Exp(m_dBCoefficient * dTerm01);
                dXi12 = Math.Exp(dTerm12);
                dXiA12 = Math.Exp(m_dACoefficient * dTerm12);
                dXiB12 = Math.Exp(m_dBCoefficient * dTerm12);
                dXi03 = Math.Exp(dTerm03);
                dXiA03 = Math.Exp(m_dACoefficient * dTerm03);
                dXiB03 = Math.Exp(m_dBCoefficient * dTerm03);

                EE1ntDiveProfile p = this.Entry(i);

                if (p.PSISeriousDCS)
                    dHess += p.Divers * ((-dTerm12 * dXiA12 * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 + (1 - dXiA12) * (-dTerm01 * dTerm01 * dXiB01 * (1 - dXi01) * dXiA01 * dXiA01 + dTerm01 * dTerm01
                        * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01)) / (1 - dXiA12) / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (-dTerm12 * dXiA12 * (dXiA01 - (1 - dXi01)
                        * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiA12) * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01
                        - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01)) / (1 - dXiA12) * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01),
                        (double)(-2)) * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01);
                else if (p.PSIMildDCS)
                    dHess += p.Divers * (((1 - dXi12) * dTerm12 * dXiA12 * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 + (1 - dXi12) * dXiA12 * (-dTerm01 * dTerm01 * dXiB01 * (1 - dXi01) * dXiA01 * dXiA01
                        + dTerm01 * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01)) / (1 - dXi12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - ((1 - dXi12)
                        * dTerm12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXi12) * dXiA12 * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01)
                        * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01)) / (1 - dXi12) / dXiA12 * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01
                        - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01);
                else if (p.IsMarginalDCS)
                    dHess += p.Divers * ((dTerm12 * dTerm12 * dXiB12 * (1 - dXi12) * dXiA12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12) * (1 - dXi12)
                        * dTerm12 * dXiA12 * dXiA12 * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - dTerm12 * dTerm12 * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dTerm12 * dXiA12 * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - dTerm12 * dXiB12 * (1 - (1 - dXi12)
                        * dXiA12) * dXiA12 * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01)
                        + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-dTerm01 * dTerm01 * dXiB01 * (1 - dXi01) * dXiA01 * dXiA01 + dTerm01 * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01)) / (1 - dXiB12)
                        / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (-(1 - dXiB12) * (1 - dXi12) * dTerm12 * dXiA12 * dXiA12 * (dXiA01
                        - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dTerm12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1
                        - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01
                        - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01 * dXiA01)) * (double)Math.Pow((double)(1 - dXiB12), (double)(-2)) / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01
                        - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * dTerm12 * dXiB12 - (-(1 - dXiB12) * (1 - dXi12) * dTerm12 * dXiA12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dTerm12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1
                        - (1 - dXi12) * dXiA12) * dXiA12 * (dTerm01 * dXiA01 - (1 - dXi01) * dTerm01 * dXiA01 + (1 - dXiB01) * (1 - dXi01) * dTerm01 * dXiA01 * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dTerm01
                        * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2))
                        * dTerm01 * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01);
                else
                    dHess += p.Divers * ((-dTerm03 * dTerm03 * dXiB03 * (1 - dXi03) * dXiA03 * dXiA03 + dTerm03 * dTerm03 * dXiB03 * (1 - (1 - dXi03) * dXiA03) * dXiA03) / (dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03)
                        * (1 - (1 - dXi03) * dXiA03) * dXiA03) - (dTerm03 * dXiA03 - (1 - dXi03) * dTerm03 * dXiA03 + (1 - dXiB03) * (1 - dXi03) * dTerm03 * dXiA03 * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03)
                        * dTerm03 * dXiA03) * (double)Math.Pow((double)(dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03), (double)(-2)) * dTerm03 * dXiB03 * (1 - (1 - dXi03) * dXiA03)
                        * dXiA03);
            } // for i

            return dHess;
        } // verified

        public double[] OptimalGainTetranomialTimeOfOnsetGainSlope(double[,] dvR01, double[,] dvR12, double[,] dvR03)
        {
            double[] dvSlope = new double[m_dvGain.Length];

            double a = m_dACoefficient;
            double b = m_dBCoefficient;

            for (int i = 0; i < this.Profiles; i++)
            {
                double dTerm01 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();

                for (int n = 0; n < m_dvGain.Length; n++)
                {
                    dTerm01 -= m_dvGain[n] * dvR01[n, i];
                    dTerm12 -= m_dvGain[n] * dvR12[n, i];
                    dTerm03 -= m_dvGain[n] * dvR03[n, i];
                }

                double dXi01 = new double();
                double dXiA01 = new double();
                double dXiB01 = new double();
                double dXi12 = new double();
                double dXiA12 = new double();
                double dXiB12 = new double();
                double dXi03 = new double();
                double dXiA03 = new double();
                double dXiB03 = new double();

                dXi01 = Math.Exp(dTerm01);
                dXiA01 = Math.Exp(m_dACoefficient * dTerm01);
                dXiB01 = Math.Exp(m_dBCoefficient * dTerm01);
                dXi12 = Math.Exp(dTerm12);
                dXiA12 = Math.Exp(m_dACoefficient * dTerm12);
                dXiB12 = Math.Exp(m_dBCoefficient * dTerm12);
                dXi03 = Math.Exp(dTerm03);
                dXiA03 = Math.Exp(m_dACoefficient * dTerm03);
                dXiB03 = Math.Exp(m_dBCoefficient * dTerm03);

                EE1ntDiveProfile p = this.Entry(i);

                if (p.PSISeriousDCS)
                {
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dvSlope[n] += (double)p.Divers * ((a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiA12) * (-a * dvR01[n, i] * dXiA01
                            - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01
                            + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) / (1 - dXiA12) / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                            * (1 - (1 - dXi01) * dXiA01) * dXiA01));
                }
                else if (p.PSIMildDCS)
                {
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dvSlope[n] += (double)p.Divers * ((dvR12[n, i] * dXi12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXi12) * a * dvR12[n, i] * dXiA12
                            * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXi12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a
                            * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01
                            + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) / (1 - dXi12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01));
                }
                else if (p.IsMarginalDCS)
                {
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dvSlope[n] += (double)p.Divers * ((b * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12)
                            * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12) * (1
                            - (1 - dXi12) * dXiA12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12
                            * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i]
                            * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12
                            / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01));
                }
                else
                {
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dvSlope[n] += (double)p.Divers * ((-a * dvR03[n, i] * dXiA03 - dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[n, i] * dXiA03 - b * dvR03[n, i] * dXiB03 * (1 - (1 - dXi03) * dXiA03) * dXiA03
                            - (1 - dXiB03) * (-dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[n, i] * dXiA03) * dXiA03 + (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * a * dvR03[n, i] * dXiA03) / (dXiA03 - (1 - dXi03)
                            * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03));
                }
            } // for i

            return dvSlope;
        } // verified

        public double[] OptimalGainTetranomialTimeOfOnsetGainCurvature(double[,] dvR01, double[,] dvR12, double[,] dvR03)
        {
            double[] dvCurv = new double[m_dvGain.Length];

            double a = m_dACoefficient;
            double b = m_dBCoefficient;

            for (int i = 0; i < this.Profiles; i++)
            {
                double dTerm01 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();

                for (int n = 0; n < m_dvGain.Length; n++)
                {
                    dTerm01 -= m_dvGain[n] * dvR01[n, i];
                    dTerm12 -= m_dvGain[n] * dvR12[n, i];
                    dTerm03 -= m_dvGain[n] * dvR03[n, i];
                }

                double dXi01 = new double();
                double dXiA01 = new double();
                double dXiB01 = new double();
                double dXi12 = new double();
                double dXiA12 = new double();
                double dXiB12 = new double();
                double dXi03 = new double();
                double dXiA03 = new double();
                double dXiB03 = new double();

                dXi01 = Math.Exp(dTerm01);
                dXiA01 = Math.Exp(m_dACoefficient * dTerm01);
                dXiB01 = Math.Exp(m_dBCoefficient * dTerm01);
                dXi12 = Math.Exp(dTerm12);
                dXiA12 = Math.Exp(m_dACoefficient * dTerm12);
                dXiB12 = Math.Exp(m_dBCoefficient * dTerm12);
                dXi03 = Math.Exp(dTerm03);
                dXiA03 = Math.Exp(m_dACoefficient * dTerm03);
                dXiB03 = Math.Exp(m_dBCoefficient * dTerm03);

                EE1ntDiveProfile p = this.Entry(i);

                if (p.PSISeriousDCS)
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dvCurv[n] += p.Divers * ((-a * a * dvR12[n, i] * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + 2 * a * dvR12[n, i] * dXiA12
                            * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01)
                            * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01) + (1 - dXiA12) * (a * a * dvR01[n, i]
                            * dvR01[n, i] * dXiA01 + dvR01[n, i] * dvR01[n, i] * dXi01 * dXiA01 + 2 * dvR01[n, i] * dvR01[n, i] * dXi01 * a * dXiA01 - (1 - dXi01) * a * a * dvR01[n, i] * dvR01[n, i] * dXiA01 + b
                            * b * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - 2 * b * dvR01[n, i] * dXiB01 * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01)
                            * dXiA01 + 2 * b * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * a * dXiA01 - (1 - dXiB01) * (dvR01[n, i] * dvR01[n, i] * dXi01 * dXiA01 + 2 * dvR01[n, i] * dvR01[n, i]
                            * dXi01 * a * dXiA01 - (1 - dXi01) * a * a * dvR01[n, i] * dvR01[n, i] * dXiA01) * dXiA01 + 2 * (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * a
                            * dvR01[n, i] * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * a * dvR01[n, i] * dvR01[n, i] * dXiA01)) / (1 - dXiA12) / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1
                            - dXi01) * dXiA01) * dXiA01) - (a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiA12) * (-a * dvR01[n, i] * dXiA01
                            - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01
                            + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) * (double)Math.Pow((double)(1 - dXiA12), (double)(-2)) / (dXiA01
                            - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * a * dvR12[n, i] * dXiA12 - (a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1
                            - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiA12) * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1
                            - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i]
                            * dXiA01)) / (1 - dXiA12) * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * (-a * dvR01[n, i] * dXiA01
                            - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01
                            + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01));
                else if (p.PSIMildDCS)
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dvCurv[n] += p.Divers * ((-dvR12[n, i] * dvR12[n, i] * dXi12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - 2 * dvR12[n, i] * dvR12[n, i] * dXi12
                            * a * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + 2 * dvR12[n, i] * dXi12 * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01
                            + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01)
                            * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01) + (1 - dXi12) * a * a * dvR12[n, i] * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1
                            - (1 - dXi01) * dXiA01) * dXiA01) - 2 * (1 - dXi12) * a * dvR12[n, i] * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b
                            * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01)
                            * dXiA01) * a * dvR01[n, i] * dXiA01) + (1 - dXi12) * dXiA12 * (a * a * dvR01[n, i] * dvR01[n, i] * dXiA01 + dvR01[n, i] * dvR01[n, i] * dXi01 * dXiA01 + 2 * dvR01[n, i] * dvR01[n, i] * dXi01
                            * a * dXiA01 - (1 - dXi01) * a * a * dvR01[n, i] * dvR01[n, i] * dXiA01 + b * b * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - 2 * b * dvR01[n, i] * dXiB01
                            * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + 2 * b * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * a * dXiA01 - (1 - dXiB01)
                            * (dvR01[n, i] * dvR01[n, i] * dXi01 * dXiA01 + 2 * dvR01[n, i] * dvR01[n, i] * dXi01 * a * dXiA01 - (1 - dXi01) * a * a * dvR01[n, i] * dvR01[n, i] * dXiA01) * dXiA01 + 2 * (1 - dXiB01)
                            * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * a * dvR01[n, i] * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * a * dvR01[n, i] * dvR01[n, i] * dXiA01))
                            / (1 - dXi12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (dvR12[n, i] * dXi12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                            * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXi12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXi12) * dXiA12
                            * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01)
                            * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) * (double)Math.Pow((double)(1 - dXi12),
                            (double)(-2)) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * dvR12[n, i] * dXi12 + (dvR12[n, i] * dXi12 * dXiA12 * (dXiA01 - (1 - dXi01)
                            * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXi12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                            + (1 - dXi12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01
                            - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) / (1 - dXi12)
                            / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * a * dvR12[n, i] - (dvR12[n, i] * dXi12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                            * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXi12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXi12) * dXiA12
                            * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i]
                            * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) / (1 - dXi12) / dXiA12
                            * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01
                            + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01)
                            * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01));
                else if (p.IsMarginalDCS)
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dvCurv[n] += p.Divers * (-b * b * dvR12[n, i] * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + 2
                            * b * dvR12[n, i] * dXiB12 * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01)
                            * dXiA01) - 2 * b * dvR12[n, i] * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * a * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + 2 * b
                            * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01
                            * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a
                            * dvR01[n, i] * dXiA01) + (1 - dXiB12) * (dvR12[n, i] * dvR12[n, i] * dXi12 * dXiA12 + 2 * dvR12[n, i] * dvR12[n, i] * dXi12 * a * dXiA12 - (1 - dXi12) * a * a * dvR12[n, i] * dvR12[n, i]
                            * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - 2 * (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i]
                            * dXiA12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + 2 * (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12)
                            * a * dvR12[n, i] * dXiA12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01)
                            * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01) + (1 - dXiB12)
                            * (1 - (1 - dXi12) * dXiA12) * a * a * dvR12[n, i] * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - 2 * (1 - dXiB12) * (1 - (1 - dXi12)
                            * dXiA12) * a * dvR12[n, i] * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01)
                            * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01) + (1 - dXiB12)
                            * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (a * a * dvR01[n, i] * dvR01[n, i] * dXiA01 + dvR01[n, i] * dvR01[n, i] * dXi01 * dXiA01 + 2 * dvR01[n, i] * dvR01[n, i] * dXi01 * a * dXiA01 - (1 - dXi01)
                            * a * a * dvR01[n, i] * dvR01[n, i] * dXiA01 + b * b * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - 2 * b * dvR01[n, i] * dXiB01 * (-dvR01[n, i] * dXi01 * dXiA01
                            + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + 2 * b * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * a * dXiA01 - (1 - dXiB01) * (dvR01[n, i] * dvR01[n, i] * dXi01
                            * dXiA01 + 2 * dvR01[n, i] * dvR01[n, i] * dXi01 * a * dXiA01 - (1 - dXi01) * a * a * dvR01[n, i] * dvR01[n, i] * dXiA01) * dXiA01 + 2 * (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01)
                            * a * dvR01[n, i] * dXiA01) * a * dvR01[n, i] * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * a * dvR01[n, i] * dvR01[n, i] * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12)
                            / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (b * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01
                            - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01
                            - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                            * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i]
                            * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01)
                            * a * dvR01[n, i] * dXiA01)) * (double)Math.Pow((double)(1 - dXiB12), (double)(-2)) / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                            * dXiA01) * dXiA01) * b * dvR12[n, i] * dXiB12 - (b * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01)
                            * dXiA01) + (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                            - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12)
                            * dXiA12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01
                            - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) / (1 - dXiB12)
                            * (double)Math.Pow((double)(1 - (1 - dXi12) * dXiA12), (double)(-2)) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * (-dvR12[n, i] * dXi12
                            * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) + (b * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01)
                            * dXiA01) + (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                            - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12)
                            * dXiA12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01
                            - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) / (1 - dXiB12)
                            / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * a * dvR12[n, i] - (b * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12)
                            * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i]
                            * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[n, i] * dXiA12 * (dXiA01
                            - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01
                            + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i]
                            * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 * (double)Math.Pow((double)(dXiA01 - (1 - dXi01)
                            * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b
                            * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1
                            - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01);
                else
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dvCurv[n] += p.Divers * ((a * a * dvR03[n, i] * dvR03[n, i] * dXiA03 + dvR03[n, i] * dvR03[n, i] * dXi03 * dXiA03 + 2 * dvR03[n, i] * dvR03[n, i] * dXi03 * a * dXiA03 - (1 - dXi03) * a * a
                            * dvR03[n, i] * dvR03[n, i] * dXiA03 + b * b * dvR03[n, i] * dvR03[n, i] * dXiB03 * (1 - (1 - dXi03) * dXiA03) * dXiA03 - 2 * b * dvR03[n, i] * dXiB03 * (-dvR03[n, i] * dXi03 * dXiA03
                            + (1 - dXi03) * a * dvR03[n, i] * dXiA03) * dXiA03 + 2 * b * dvR03[n, i] * dvR03[n, i] * dXiB03 * (1 - (1 - dXi03) * dXiA03) * a * dXiA03 - (1 - dXiB03) * (dvR03[n, i] * dvR03[n, i]
                            * dXi03 * dXiA03 + 2 * dvR03[n, i] * dvR03[n, i] * dXi03 * a * dXiA03 - (1 - dXi03) * a * a * dvR03[n, i] * dvR03[n, i] * dXiA03) * dXiA03 + 2 * (1 - dXiB03) * (-dvR03[n, i] * dXi03
                            * dXiA03 + (1 - dXi03) * a * dvR03[n, i] * dXiA03) * a * dvR03[n, i] * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * a * a * dvR03[n, i] * dvR03[n, i] * dXiA03) / (dXiA03 - (1 - dXi03)
                            * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03) - (double)Math.Pow((double)(-a * dvR03[n, i] * dXiA03 - dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[n, i] * dXiA03
                            - b * dvR03[n, i] * dXiB03 * (1 - (1 - dXi03) * dXiA03) * dXiA03 - (1 - dXiB03) * (-dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[n, i] * dXiA03) * dXiA03 + (1 - dXiB03) * (1
                            - (1 - dXi03) * dXiA03) * a * dvR03[n, i] * dXiA03), (double)2) * (double)Math.Pow((double)(dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03), (double)(-2)));
            } // for i

            return dvCurv;
        } // verified but accuracy problems with finite differences

        public double OptimalGainTetranomialTimeOfOnsetAGainCrossDerivative(int n, double[,] dvR01, double[,] dvR12, double[,] dvR03)
        {
            double dHess = new double();

            double a = m_dACoefficient;
            double b = m_dBCoefficient;

            for (int i = 0; i < this.Profiles; i++)
            {
                double dTerm01 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();

                for (int j = 0; j < m_dvGain.Length; j++)
                {
                    dTerm01 -= m_dvGain[j] * dvR01[j, i];
                    dTerm12 -= m_dvGain[j] * dvR12[j, i];
                    dTerm03 -= m_dvGain[j] * dvR03[j, i];
                }

                double dXi01 = new double();
                double dXiA01 = new double();
                double dXiB01 = new double();
                double dXi12 = new double();
                double dXiA12 = new double();
                double dXiB12 = new double();
                double dXi03 = new double();
                double dXiA03 = new double();
                double dXiB03 = new double();

                dXi01 = Math.Exp(dTerm01);
                dXiA01 = Math.Exp(m_dACoefficient * dTerm01);
                dXiB01 = Math.Exp(m_dBCoefficient * dTerm01);
                dXi12 = Math.Exp(dTerm12);
                dXiA12 = Math.Exp(m_dACoefficient * dTerm12);
                dXiB12 = Math.Exp(m_dBCoefficient * dTerm12);
                dXi03 = Math.Exp(dTerm03);
                dXiA03 = Math.Exp(m_dACoefficient * dTerm03);
                dXiB03 = Math.Exp(m_dBCoefficient * dTerm03);

                EE1ntDiveProfile p = this.Entry(i);

                if (p.PSISeriousDCS)
                    dHess += p.Divers * ((dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - a * dvR12[n, i] * dvR12[n, i] * m_dvGain[n] * dXiA12 * (dXiA01
                        - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + a * dvR12[n, i] * dXiA12 * (-m_dvGain[n] * dvR01[n, i] * dXiA01 + (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01
                        - (1 - dXiB01) * (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * m_dvGain[n] * dvR01[n, i] * dXiA01) + m_dvGain[n] * dvR12[n, i] * dXiA12
                        * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i]
                        * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01) + (1 - dXiA12) * (-dvR01[n, i] * dXiA01 + a * dvR01[n, i]
                        * dvR01[n, i] * m_dvGain[n] * dXiA01 + dvR01[n, i] * dvR01[n, i] * dXi01 * m_dvGain[n] * dXiA01 + (1 - dXi01) * dvR01[n, i] * dXiA01 - (1 - dXi01) * a * dvR01[n, i] * dvR01[n, i] * m_dvGain[n] * dXiA01
                        - b * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - dXi01) * m_dvGain[n] * dXiA01 * dXiA01 + b * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * m_dvGain[n] * dXiA01 - (1 - dXiB01)
                        * (dvR01[n, i] * dvR01[n, i] * dXi01 * m_dvGain[n] * dXiA01 + (1 - dXi01) * dvR01[n, i] * dXiA01 - (1 - dXi01) * a * dvR01[n, i] * dvR01[n, i] * m_dvGain[n] * dXiA01) * dXiA01 + (1 - dXiB01)
                        * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * m_dvGain[n] * dvR01[n, i] * dXiA01 + (1 - dXiB01) * (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dvR01[n, i] * dXiA01 * dXiA01
                        * a + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dvR01[n, i] * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dvR01[n, i] * m_dvGain[n] * dXiA01)) / (1 - dXiA12) / (dXiA01
                        - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                        + (1 - dXiA12) * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01)
                        * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) * (double)Math.Pow((double)(1 - dXiA12),
                        (double)(-2)) / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * m_dvGain[n] * dvR12[n, i] * dXiA12 - (a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01
                        - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiA12) * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i]
                        * dXiA01)) / (1 - dXiA12) * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * (-m_dvGain[n] * dvR01[n, i] * dXiA01
                        + (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 - (1 - dXiB01) * (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * m_dvGain[n] * dvR01[n, i] * dXiA01));
                else if (p.PSIMildDCS)
                    dHess += p.Divers * ((-dvR12[n, i] * dvR12[n, i] * dXi12 * m_dvGain[n] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + dvR12[n, i] * dXi12 * dXiA12
                        * (-m_dvGain[n] * dvR01[n, i] * dXiA01 + (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 - (1 - dXiB01) * (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * m_dvGain[n] * dvR01[n, i] * dXiA01) - (1 - dXi12) * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXi12) * a
                        * dvR12[n, i] * dvR12[n, i] * m_dvGain[n] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXi12) * a * dvR12[n, i] * dXiA12 * (-m_dvGain[n]
                        * dvR01[n, i] * dXiA01 + (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 - (1 - dXiB01) * (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * m_dvGain[n]
                        * dvR01[n, i] * dXiA01) - (1 - dXi12) * m_dvGain[n] * dvR12[n, i] * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i]
                        * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a
                        * dvR01[n, i] * dXiA01) + (1 - dXi12) * dXiA12 * (-dvR01[n, i] * dXiA01 + a * dvR01[n, i] * dvR01[n, i] * m_dvGain[n] * dXiA01 + dvR01[n, i] * dvR01[n, i] * dXi01 * m_dvGain[n] * dXiA01 + (1 - dXi01)
                        * dvR01[n, i] * dXiA01 - (1 - dXi01) * a * dvR01[n, i] * dvR01[n, i] * m_dvGain[n] * dXiA01 - b * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - dXi01) * m_dvGain[n] * dXiA01 * dXiA01 + b * dvR01[n, i]
                        * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * m_dvGain[n] * dXiA01 - (1 - dXiB01) * (dvR01[n, i] * dvR01[n, i] * dXi01 * m_dvGain[n] * dXiA01 + (1 - dXi01) * dvR01[n, i] * dXiA01 - (1 - dXi01)
                        * a * dvR01[n, i] * dvR01[n, i] * m_dvGain[n] * dXiA01) * dXiA01 + (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * m_dvGain[n] * dvR01[n, i] * dXiA01 + (1 - dXiB01)
                        * (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dvR01[n, i] * dXiA01 * dXiA01 * a + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dvR01[n, i] * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a
                        * dvR01[n, i] * dvR01[n, i] * m_dvGain[n] * dXiA01)) / (1 - dXi12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (dvR12[n, i] * dXi12 * dXiA12
                        * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXi12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01) + (1 - dXi12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01)
                        * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) / (1 - dXi12)
                        / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * m_dvGain[n] * dvR12[n, i] - (dvR12[n, i] * dXi12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXi12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXi12) * dXiA12
                        * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i]
                        * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) / (1 - dXi12) / dXiA12 * (double)Math.Pow((double)(dXiA01
                        - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * (-m_dvGain[n] * dvR01[n, i] * dXiA01 + (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 - (1 - dXiB01)
                        * (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * m_dvGain[n] * dvR01[n, i] * dXiA01));
                else if (p.IsMarginalDCS)
                    dHess += p.Divers * ((b * dvR12[n, i] * dvR12[n, i] * dXiB12 * (1 - dXi12) * m_dvGain[n] * dXiA12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - b
                        * dvR12[n, i] * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * m_dvGain[n] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + b * dvR12[n, i]
                        * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-m_dvGain[n] * dvR01[n, i] * dXiA01 + (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 - (1 - dXiB01) * (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01
                        * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * m_dvGain[n] * dvR01[n, i] * dXiA01) + (1 - dXiB12) * (dvR12[n, i] * dvR12[n, i] * dXi12 * m_dvGain[n] * dXiA12 + (1 - dXi12) * dvR12[n, i]
                        * dXiA12 - (1 - dXi12) * a * dvR12[n, i] * dvR12[n, i] * m_dvGain[n] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12)
                        * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * m_dvGain[n] * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                        + (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * dXiA12 * (-m_dvGain[n] * dvR01[n, i] * dXiA01 + (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 - (1 - dXiB01)
                        * (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * m_dvGain[n] * dvR01[n, i] * dXiA01) - (1 - dXiB12) * (1 - dXi12) * m_dvGain[n] * dvR12[n, i]
                        * dvR12[n, i] * dXiA12 * dXiA12 * a * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dvR12[n, i] * dXiA12 * (dXiA01
                        - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[n, i] * dvR12[n, i] * m_dvGain[n] * dXiA12 * (dXiA01 - (1 - dXi01)
                        * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[n, i] * dXiA12 * (-m_dvGain[n] * dvR01[n, i] * dXiA01 + (1 - dXi01) * m_dvGain[n]
                        * dvR01[n, i] * dXiA01 - (1 - dXiB01) * (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * m_dvGain[n] * dvR01[n, i] * dXiA01) + (1 - dXiB12)
                        * (1 - dXi12) * m_dvGain[n] * dvR12[n, i] * dXiA12 * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1
                        - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i]
                        * dXiA01) - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * m_dvGain[n] * dvR12[n, i] * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b
                        * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * a * dvR01[n, i] * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-dvR01[n, i] * dXiA01 + a * dvR01[n, i] * dvR01[n, i] * m_dvGain[n] * dXiA01 + dvR01[n, i] * dvR01[n, i]
                        * dXi01 * m_dvGain[n] * dXiA01 + (1 - dXi01) * dvR01[n, i] * dXiA01 - (1 - dXi01) * a * dvR01[n, i] * dvR01[n, i] * m_dvGain[n] * dXiA01 - b * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - dXi01) * m_dvGain[n]
                        * dXiA01 * dXiA01 + b * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * m_dvGain[n] * dXiA01 - (1 - dXiB01) * (dvR01[n, i] * dvR01[n, i] * dXi01 * m_dvGain[n] * dXiA01 + (1 - dXi01)
                        * dvR01[n, i] * dXiA01 - (1 - dXi01) * a * dvR01[n, i] * dvR01[n, i] * m_dvGain[n] * dXiA01) * dXiA01 + (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * m_dvGain[n]
                        * dvR01[n, i] * dXiA01 + (1 - dXiB01) * (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dvR01[n, i] * dXiA01 * dXiA01 * a + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dvR01[n, i] * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dvR01[n, i] * m_dvGain[n] * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1
                        - (1 - dXi01) * dXiA01) * dXiA01) - (b * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                        + (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                        - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12)
                        * dXiA12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1
                        - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) / (1 - dXiB12)
                        * (double)Math.Pow((double)(1 - (1 - dXi12) * dXiA12), (double)(-2)) / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * (1 - dXi12) * m_dvGain[n]
                        * dvR12[n, i] + (b * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (-dvR12[n, i]
                        * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12)
                        * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-a * dvR01[n, i] * dXiA01
                        - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01)
                        * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01
                        - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * m_dvGain[n] * dvR12[n, i] - (b * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1
                        - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                        + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1
                        - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i]
                        * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2))
                        * (-m_dvGain[n] * dvR01[n, i] * dXiA01 + (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 - (1 - dXiB01) * (1 - dXi01) * m_dvGain[n] * dvR01[n, i] * dXiA01 * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * m_dvGain[n] * dvR01[n, i] * dXiA01));
                else
                    dHess += p.Divers * ((-dvR03[n, i] * dXiA03 + a * dvR03[n, i] * dvR03[n, i] * m_dvGain[n] * dXiA03 + dvR03[n, i] * dvR03[n, i] * dXi03 * m_dvGain[n] * dXiA03 + (1 - dXi03) * dvR03[n, i] * dXiA03 - (1 - dXi03)
                        * a * dvR03[n, i] * dvR03[n, i] * m_dvGain[n] * dXiA03 - b * dvR03[n, i] * dvR03[n, i] * dXiB03 * (1 - dXi03) * m_dvGain[n] * dXiA03 * dXiA03 + b * dvR03[n, i] * dvR03[n, i] * dXiB03 * (1 - (1 - dXi03)
                        * dXiA03) * m_dvGain[n] * dXiA03 - (1 - dXiB03) * (dvR03[n, i] * dvR03[n, i] * dXi03 * m_dvGain[n] * dXiA03 + (1 - dXi03) * dvR03[n, i] * dXiA03 - (1 - dXi03) * a * dvR03[n, i] * dvR03[n, i] * m_dvGain[n]
                        * dXiA03) * dXiA03 + (1 - dXiB03) * (-dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[n, i] * dXiA03) * m_dvGain[n] * dvR03[n, i] * dXiA03 + (1 - dXiB03) * (1 - dXi03) * m_dvGain[n] * dvR03[n, i]
                        * dvR03[n, i] * dXiA03 * dXiA03 * a + (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dvR03[n, i] * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * a * dvR03[n, i] * dvR03[n, i] * m_dvGain[n] * dXiA03)
                        / (dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03) - (-a * dvR03[n, i] * dXiA03 - dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[n, i] * dXiA03 - b
                        * dvR03[n, i] * dXiB03 * (1 - (1 - dXi03) * dXiA03) * dXiA03 - (1 - dXiB03) * (-dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[n, i] * dXiA03) * dXiA03 + (1 - dXiB03) * (1 - (1 - dXi03)
                        * dXiA03) * a * dvR03[n, i] * dXiA03) * (double)Math.Pow((double)(dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03), (double)(-2)) * (-m_dvGain[n] * dvR03[n, i]
                        * dXiA03 + (1 - dXi03) * m_dvGain[n] * dvR03[n, i] * dXiA03 - (1 - dXiB03) * (1 - dXi03) * m_dvGain[n] * dvR03[n, i] * dXiA03 * dXiA03 + (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * m_dvGain[n]
                        * dvR03[n, i] * dXiA03));
            } // for i

            return dHess;
        } // verified but accuracy problems with finite differences

        public double OptimalGainTetranomialTimeOfOnsetBGainCrossDerivative(int n, double[,] dvR01, double[,] dvR12, double[,] dvR03)
        {
            double dHess = new double();

            double a = m_dACoefficient;
            double b = m_dBCoefficient;

            for (int i = 0; i < this.Profiles; i++)
            {
                double dTerm01 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();

                for (int j = 0; j < m_dvGain.Length; j++)
                {
                    dTerm01 -= m_dvGain[j] * dvR01[j, i];
                    dTerm12 -= m_dvGain[j] * dvR12[j, i];
                    dTerm03 -= m_dvGain[j] * dvR03[j, i];
                }

                double dXi01 = new double();
                double dXiA01 = new double();
                double dXiB01 = new double();
                double dXi12 = new double();
                double dXiA12 = new double();
                double dXiB12 = new double();
                double dXi03 = new double();
                double dXiA03 = new double();
                double dXiB03 = new double();

                dXi01 = Math.Exp(dTerm01);
                dXiA01 = Math.Exp(m_dACoefficient * dTerm01);
                dXiB01 = Math.Exp(m_dBCoefficient * dTerm01);
                dXi12 = Math.Exp(dTerm12);
                dXiA12 = Math.Exp(m_dACoefficient * dTerm12);
                dXiB12 = Math.Exp(m_dBCoefficient * dTerm12);
                dXi03 = Math.Exp(dTerm03);
                dXiA03 = Math.Exp(m_dACoefficient * dTerm03);
                dXiB03 = Math.Exp(m_dBCoefficient * dTerm03);

                EE1ntDiveProfile p = this.Entry(i);

                if (p.PSISeriousDCS)
                    dHess += p.Divers * ((-a * dvR12[n, i] * dXiA12 * m_dvGain[n] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 + (1 - dXiA12)
                        * (-dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 + b * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01)
                        * dXiA01) * m_dvGain[n] * dXiA01 - m_dvGain[n] * dvR01[n, i] * dXiB01 * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a
                        * dvR01[n, i] * dXiA01) * dXiA01 + m_dvGain[n] * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * a * dXiA01))
                        / (1 - dXiA12) / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (a * dvR12[n, i] * dXiA12
                        * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiA12) * (-a * dvR01[n, i] * dXiA01
                        - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01)
                        * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01)) / (1 - dXiA12) * (double)Math.Pow((double)(dXiA01 - (1 - dXi01)
                        * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * m_dvGain[n] * dvR01[n, i] * dXiB01
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01);
                else if (p.PSIMildDCS)
                    dHess += p.Divers * ((-dvR12[n, i] * dXi12 * dXiA12 * m_dvGain[n] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01
                        + (1 - dXi12) * a * dvR12[n, i] * dXiA12 * m_dvGain[n] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01
                        + (1 - dXi12) * dXiA12 * (-dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 + b * dvR01[n, i] * dvR01[n, i]
                        * dXiB01 * (1 - (1 - dXi01) * dXiA01) * m_dvGain[n] * dXiA01 - m_dvGain[n] * dvR01[n, i] * dXiB01 * (-dvR01[n, i] * dXi01 * dXiA01
                        + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + m_dvGain[n] * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01)
                        * a * dXiA01)) / (1 - dXi12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01)
                        * dXiA01) + (dvR12[n, i] * dXi12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01)
                        * dXiA01) - (1 - dXi12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01) + (1 - dXi12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a
                        * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i]
                        * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a
                        * dvR01[n, i] * dXiA01)) / (1 - dXi12) / dXiA12 * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * m_dvGain[n] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01);
                else if (p.IsMarginalDCS)
                    dHess += p.Divers * ((dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01) - b * dvR12[n, i] * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * m_dvGain[n]
                        * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - b * dvR12[n, i] * dXiB12
                        * (1 - (1 - dXi12) * dXiA12) * dXiA12 * m_dvGain[n] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 + m_dvGain[n]
                        * dvR12[n, i] * dXiB12 * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01)
                        * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12)
                        * a * dvR12[n, i] * dXiA12) * dXiA12 * m_dvGain[n] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - m_dvGain[n]
                        * dvR12[n, i] * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * a * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[n, i] * dXiA12 * m_dvGain[n]
                        * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 + m_dvGain[n] * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12)
                        * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i]
                        * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i]
                        * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12)
                        * dXiA12) * dXiA12 * (-dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 + b * dvR01[n, i] * dvR01[n, i] * dXiB01
                        * (1 - (1 - dXi01) * dXiA01) * m_dvGain[n] * dXiA01 - m_dvGain[n] * dvR01[n, i] * dXiB01 * (-dvR01[n, i] * dXi01 * dXiA01
                        + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + m_dvGain[n] * dvR01[n, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01)
                        * a * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (b * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01)
                        * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12)
                        * a * dvR12[n, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01)
                        * dXiA01) - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-a * dvR01[n, i] * dXiA01
                        - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01
                        + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01))
                        * (double)Math.Pow((double)(1 - dXiB12), (double)(-2)) / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01)
                        * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * m_dvGain[n] * dvR12[n, i] * dXiB12 + (b * dvR12[n, i]
                        * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01) + (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * dXiA12
                        * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12) * (1 - (1 - dXi12)
                        * dXiA12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                        + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01
                        + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01)
                        * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01)
                        * a * dvR01[n, i] * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 * (double)Math.Pow((double)(dXiA01
                        - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * m_dvGain[n] * dvR01[n, i]
                        * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01);
                else
                    dHess += p.Divers * ((-dvR03[n, i] * dXiB03 * (1 - (1 - dXi03) * dXiA03) * dXiA03 + b * dvR03[n, i] * dvR03[n, i] * dXiB03 * (1 - (1
                        - dXi03) * dXiA03) * m_dvGain[n] * dXiA03 - m_dvGain[n] * dvR03[n, i] * dXiB03 * (-dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a
                        * dvR03[n, i] * dXiA03) * dXiA03 + m_dvGain[n] * dvR03[n, i] * dvR03[n, i] * dXiB03 * (1 - (1 - dXi03) * dXiA03) * a * dXiA03)
                        / (dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03) + (-a * dvR03[n, i] * dXiA03
                        - dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[n, i] * dXiA03 - b * dvR03[n, i] * dXiB03 * (1 - (1 - dXi03)
                        * dXiA03) * dXiA03 - (1 - dXiB03) * (-dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[n, i] * dXiA03) * dXiA03
                        + (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * a * dvR03[n, i] * dXiA03) * (double)Math.Pow((double)(dXiA03 - (1 - dXi03)
                        * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03), (double)(-2)) * m_dvGain[n] * dvR03[n, i] * dXiB03 * (1
                        - (1 - dXi03) * dXiA03) * dXiA03);
            } // for i

            return dHess;
        } // verified but accuracy problems with finite differences

        public double OptimalGainTetranomialTimeOfOnsetGainCrossDerivative(int m, int n, double[,] dvR01, double[,] dvR12, double[,] dvR03)
        {
            double dHess = new double();

            double a = m_dACoefficient;
            double b = m_dBCoefficient;

            for (int i = 0; i < this.Profiles; i++)
            {
                double dTerm01 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();

                for (int j = 0; j < m_dvGain.Length; j++)
                {
                    dTerm01 -= m_dvGain[j] * dvR01[j, i];
                    dTerm12 -= m_dvGain[j] * dvR12[j, i];
                    dTerm03 -= m_dvGain[j] * dvR03[j, i];
                }

                double dXi01 = new double();
                double dXiA01 = new double();
                double dXiB01 = new double();
                double dXi12 = new double();
                double dXiA12 = new double();
                double dXiB12 = new double();
                double dXi03 = new double();
                double dXiA03 = new double();
                double dXiB03 = new double();

                dXi01 = Math.Exp(dTerm01);
                dXiA01 = Math.Exp(m_dACoefficient * dTerm01);
                dXiB01 = Math.Exp(m_dBCoefficient * dTerm01);
                dXi12 = Math.Exp(dTerm12);
                dXiA12 = Math.Exp(m_dACoefficient * dTerm12);
                dXiB12 = Math.Exp(m_dBCoefficient * dTerm12);
                dXi03 = Math.Exp(dTerm03);
                dXiA03 = Math.Exp(m_dACoefficient * dTerm03);
                dXiB03 = Math.Exp(m_dBCoefficient * dTerm03);

                EE1ntDiveProfile p = this.Entry(i);

                if (p.PSISeriousDCS)
                    dHess += p.Divers * ((-a * a * dvR12[m, i] * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + a * dvR12[m, i] * dXiA12 * (-a * dvR01[n, i]
                        * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01
                        + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01) + a * dvR12[n, i] * dXiA12 * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i]
                        * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01 - b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a
                        * dvR01[m, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[m, i] * dXiA01) + (1 - dXiA12) * (a * a * dvR01[m, i] * dvR01[n, i] * dXiA01 + dvR01[m, i] * dvR01[n, i]
                        * dXi01 * dXiA01 + 2 * dvR01[m, i] * dXi01 * a * dvR01[n, i] * dXiA01 - (1 - dXi01) * a * a * dvR01[m, i] * dvR01[n, i] * dXiA01 + b * b * dvR01[m, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01 - b * dvR01[m, i] * dXiB01 * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + 2 * b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * a
                        * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 - (1 - dXiB01) * (dvR01[m, i] * dvR01[n, i] * dXi01 * dXiA01
                        + 2 * dvR01[m, i] * dXi01 * a * dvR01[n, i] * dXiA01 - (1 - dXi01) * a * a * dvR01[m, i] * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a
                        * dvR01[m, i] * dXiA01) * a * dvR01[n, i] * dXiA01 + (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * a * dvR01[m, i] * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * a * a * dvR01[m, i] * dvR01[n, i] * dXiA01)) / (1 - dXiA12) / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (a * dvR12[m, i]
                        * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiA12) * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01)
                        * a * dvR01[m, i] * dXiA01 - b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01
                        + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[m, i] * dXiA01)) * (double)Math.Pow((double)(1 - dXiA12), (double)(-2)) / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01) * a * dvR12[n, i] * dXiA12 - (a * dvR12[m, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiA12) * (-a * dvR01[m, i]
                        * dXiA01 - dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01 - b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01
                        + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[m, i] * dXiA01)) / (1 - dXiA12) * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01
                        - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i]
                        * dXiA01));
                else if (p.PSIMildDCS)
                    dHess += p.Divers * ((-dvR12[m, i] * dvR12[n, i] * dXi12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - 2 * dvR12[m, i] * dXi12 * a * dvR12[n, i]
                        * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + dvR12[m, i] * dXi12 * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01
                        + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01)
                        * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01) + (1 - dXi12) * a * a * dvR12[m, i] * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXi12) * a * dvR12[m, i] * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b
                        * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * a * dvR01[n, i] * dXiA01) + dvR12[n, i] * dXi12 * dXiA12 * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01 - b * dvR01[m, i]
                        * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01)
                        * a * dvR01[m, i] * dXiA01) - (1 - dXi12) * a * dvR12[n, i] * dXiA12 * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01 - b * dvR01[m, i]
                        * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01)
                        * a * dvR01[m, i] * dXiA01) + (1 - dXi12) * dXiA12 * (a * a * dvR01[m, i] * dvR01[n, i] * dXiA01 + dvR01[m, i] * dvR01[n, i] * dXi01 * dXiA01 + 2 * dvR01[m, i] * dXi01 * a * dvR01[n, i] * dXiA01
                        - (1 - dXi01) * a * a * dvR01[m, i] * dvR01[n, i] * dXiA01 + b * b * dvR01[m, i] * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - b * dvR01[m, i] * dXiB01 * (-dvR01[n, i] * dXi01
                        * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + 2 * b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (-dvR01[m, i]
                        * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 - (1 - dXiB01) * (dvR01[m, i] * dvR01[n, i] * dXi01 * dXiA01 + 2 * dvR01[m, i] * dXi01 * a * dvR01[n, i] * dXiA01 - (1 - dXi01)
                        * a * a * dvR01[m, i] * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * a * dvR01[n, i] * dXiA01 + (1 - dXiB01)
                        * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * a * dvR01[m, i] * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * a * dvR01[m, i] * dvR01[n, i] * dXiA01))
                        / (1 - dXi12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (dvR12[m, i] * dXi12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXi12) * a * dvR12[m, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXi12) * dXiA12
                        * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01 - b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01)
                        * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[m, i] * dXiA01))
                        * (double)Math.Pow((double)(1 - dXi12), (double)(-2)) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * dvR12[n, i] * dXi12 + (dvR12[m, i]
                        * dXi12 * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXi12) * a * dvR12[m, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXi12) * dXiA12 * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01 - b * dvR01[m, i] * dXiB01
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a
                        * dvR01[m, i] * dXiA01)) / (1 - dXi12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * a * dvR12[n, i] - (dvR12[m, i] * dXi12 * dXiA12
                        * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXi12) * a * dvR12[m, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01) + (1 - dXi12) * dXiA12 * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01 - b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[m, i] * dXiA01))
                        / (1 - dXi12) / dXiA12 * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i]
                        * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a
                        * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01));
                else if (p.IsMarginalDCS)
                    dHess += p.Divers * ((-b * b * dvR12[m, i] * dvR12[n, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + b
                        * dvR12[m, i] * dXiB12 * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                        - 2 * b * dvR12[m, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * a * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + b * dvR12[m, i]
                        * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01) + b
                        * dvR12[n, i] * dXiB12 * (-dvR12[m, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[m, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                        + (1 - dXiB12) * (dvR12[m, i] * dvR12[n, i] * dXi12 * dXiA12 + 2 * dvR12[m, i] * dXi12 * a * dvR12[n, i] * dXiA12 - (1 - dXi12) * a * a * dvR12[m, i] * dvR12[n, i] * dXiA12) * dXiA12 * (dXiA01
                        - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12) * (-dvR12[m, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[m, i] * dXiA12) * a * dvR12[n, i] * dXiA12
                        * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (-dvR12[m, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[m, i] * dXiA12) * dXiA12 * (-a
                        * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i]
                        * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01) - (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12
                        + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * a * dvR12[m, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12)
                        * dXiA12) * a * a * dvR12[m, i] * dvR12[n, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a
                        * dvR12[m, i] * dXiA12 * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01
                        - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01) + b * dvR12[n, i]
                        * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01 - b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[m, i] * dXiA01)
                        + (1 - dXiB12) * (-dvR12[n, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) * dXiA12 * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i]
                        * dXiA01 - b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 + (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * a * dvR01[m, i] * dXiA01) - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[n, i] * dXiA12 * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i] * dXi01 * dXiA01
                        + (1 - dXi01) * a * dvR01[m, i] * dXiA01 - b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i]
                        * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[m, i] * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (a * a * dvR01[m, i] * dvR01[n, i] * dXiA01
                        + dvR01[m, i] * dvR01[n, i] * dXi01 * dXiA01 + 2 * dvR01[m, i] * dXi01 * a * dvR01[n, i] * dXiA01 - (1 - dXi01) * a * a * dvR01[m, i] * dvR01[n, i] * dXiA01 + b * b * dvR01[m, i] * dvR01[n, i]
                        * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - b * dvR01[m, i] * dXiB01 * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + 2 * b * dvR01[m, i] * dXiB01
                        * (1 - (1 - dXi01) * dXiA01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01 * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 - (1 - dXiB01) * (dvR01[m, i]
                        * dvR01[n, i] * dXi01 * dXiA01 + 2 * dvR01[m, i] * dXi01 * a * dvR01[n, i] * dXiA01 - (1 - dXi01) * a * a * dvR01[m, i] * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (-dvR01[m, i] * dXi01
                        * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * a * dvR01[n, i] * dXiA01 + (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * a * dvR01[m, i] * dXiA01
                        - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * a * dvR01[m, i] * dvR01[n, i] * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (b * dvR12[m, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                        + (1 - dXiB12) * (-dvR12[m, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[m, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01)
                        - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[m, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12)
                        * dXiA12) * dXiA12 * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01 - b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01
                        - (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[m, i] * dXiA01))
                        * (double)Math.Pow((double)(1 - dXiB12), (double)(-2)) / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * b
                        * dvR12[n, i] * dXiB12 - (b * dvR12[m, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12)
                        * (-dvR12[m, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[m, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12)
                        * (1 - (1 - dXi12) * dXiA12) * a * dvR12[m, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12)
                        * dXiA12 * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01 - b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01)
                        * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[m, i] * dXiA01)) / (1 - dXiB12)
                        * (double)Math.Pow((double)(1 - (1 - dXi12) * dXiA12), (double)(-2)) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * (-dvR12[n, i]
                        * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[n, i] * dXiA12) + (b * dvR12[m, i] * dXiB12 * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01) + (1 - dXiB12) * (-dvR12[m, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[m, i] * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01)
                        * dXiA01) * dXiA01) - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[m, i] * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12)
                        * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01 - b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01)
                        * dXiA01 - (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[m, i] * dXiA01)) / (1 - dXiB12)
                        / (1 - (1 - dXi12) * dXiA12) / dXiA12 / (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) * a * dvR12[n, i] - (b * dvR12[m, i] * dXiB12 * (1 - (1 - dXi12)
                        * dXiA12) * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (-dvR12[m, i] * dXi12 * dXiA12 + (1 - dXi12) * a * dvR12[m, i] * dXiA12)
                        * dXiA12 * (dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) - (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * a * dvR12[m, i] * dXiA12 * (dXiA01 - (1 - dXi01)
                        * dXiA01 - (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * dXiA01) + (1 - dXiB12) * (1 - (1 - dXi12) * dXiA12) * dXiA12 * (-a * dvR01[m, i] * dXiA01 - dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a
                        * dvR01[m, i] * dXiA01 - b * dvR01[m, i] * dXiB01 * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[m, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[m, i] * dXiA01) * dXiA01
                        + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a * dvR01[m, i] * dXiA01)) / (1 - dXiB12) / (1 - (1 - dXi12) * dXiA12) / dXiA12 * (double)Math.Pow((double)(dXiA01 - (1 - dXi01) * dXiA01 - (1 - dXiB01)
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01), (double)(-2)) * (-a * dvR01[n, i] * dXiA01 - dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01 - b * dvR01[n, i] * dXiB01
                        * (1 - (1 - dXi01) * dXiA01) * dXiA01 - (1 - dXiB01) * (-dvR01[n, i] * dXi01 * dXiA01 + (1 - dXi01) * a * dvR01[n, i] * dXiA01) * dXiA01 + (1 - dXiB01) * (1 - (1 - dXi01) * dXiA01) * a
                        * dvR01[n, i] * dXiA01));
                else
                    dHess += p.Divers * ((a * a * dvR03[m, i] * dvR03[n, i] * dXiA03 + dvR03[m, i] * dvR03[n, i] * dXi03 * dXiA03 + 2 * dvR03[m, i] * dXi03 * a * dvR03[n, i] * dXiA03 - (1 - dXi03) * a * a * dvR03[m, i]
                        * dvR03[n, i] * dXiA03 + b * b * dvR03[m, i] * dvR03[n, i] * dXiB03 * (1 - (1 - dXi03) * dXiA03) * dXiA03 - b * dvR03[m, i] * dXiB03 * (-dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[n, i]
                        * dXiA03) * dXiA03 + 2 * b * dvR03[m, i] * dXiB03 * (1 - (1 - dXi03) * dXiA03) * a * dvR03[n, i] * dXiA03 - b * dvR03[n, i] * dXiB03 * (-dvR03[m, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[m, i]
                        * dXiA03) * dXiA03 - (1 - dXiB03) * (dvR03[m, i] * dvR03[n, i] * dXi03 * dXiA03 + 2 * dvR03[m, i] * dXi03 * a * dvR03[n, i] * dXiA03 - (1 - dXi03) * a * a * dvR03[m, i] * dvR03[n, i] * dXiA03)
                        * dXiA03 + (1 - dXiB03) * (-dvR03[m, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[m, i] * dXiA03) * a * dvR03[n, i] * dXiA03 + (1 - dXiB03) * (-dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a
                        * dvR03[n, i] * dXiA03) * a * dvR03[m, i] * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * a * a * dvR03[m, i] * dvR03[n, i] * dXiA03) / (dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03)
                        * (1 - (1 - dXi03) * dXiA03) * dXiA03) - (-a * dvR03[m, i] * dXiA03 - dvR03[m, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[m, i] * dXiA03 - b * dvR03[m, i] * dXiB03 * (1 - (1 - dXi03)
                        * dXiA03) * dXiA03 - (1 - dXiB03) * (-dvR03[m, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[m, i] * dXiA03) * dXiA03 + (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * a * dvR03[m, i] * dXiA03)
                        * (double)Math.Pow((double)(dXiA03 - (1 - dXi03) * dXiA03 - (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * dXiA03), (double)(-2)) * (-a * dvR03[n, i] * dXiA03 - dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[n, i] * dXiA03 - b * dvR03[n, i] * dXiB03 * (1 - (1 - dXi03) * dXiA03) * dXiA03 - (1 - dXiB03) * (-dvR03[n, i] * dXi03 * dXiA03 + (1 - dXi03) * a * dvR03[n, i] * dXiA03) * dXiA03 + (1 - dXiB03) * (1 - (1 - dXi03) * dXiA03) * a * dvR03[n, i] * dXiA03));
            } // for i

            return dHess;
        } // verified but accuracy problems with finite differences

        public void OptimalGainTetranomialTimeOfOnsetOptimize(MODEL m)
        {
            // calculate R once
            double[,] dvR01 = new double[this.m_dvGain.Length, this.Profiles];
            double[,] dvR12 = new double[this.m_dvGain.Length, this.Profiles];
            double[,] dvR03 = new double[this.m_dvGain.Length, this.Profiles];
            OptimalGainTetranomialTimeOfOnsetHazard(dvR01, dvR12, dvR03, m);

            // control parameters
            double dDamping = new double();
            double dEpsilon = new double();
            int iIteration = new int();
            int iMaxIteration = new int();

            dDamping = 0.5;
            dEpsilon = 1.0e-12;
            iMaxIteration = 500;

            GeneralMatrix mHess = new GeneralMatrix(2 + m_dvGain.Length, 2 + m_dvGain.Length);
            GeneralVector vGrad = new GeneralVector(2 + m_dvGain.Length);

            for (iIteration = 0; iIteration < iMaxIteration; iIteration++)
            {
                double[] dvGGrad = OptimalGainTetranomialTimeOfOnsetGainSlope(dvR01, dvR12, dvR03);
                double[] dvGCurv = OptimalGainTetranomialTimeOfOnsetGainCurvature(dvR01, dvR12, dvR03);

                // load the Hessian and gradient components
                for (int i = 0; i < 2 + m_dvGain.Length; i++)
                {
                    if (i < 3) // gain components
                    {
                        mHess[i, i] = dvGCurv[i];
                        vGrad[i] = dvGGrad[i];
                        for (int j = i + 1; j < 3; j++)
                            mHess[i, j] = mHess[j, i] = OptimalGainTetranomialTimeOfOnsetGainCrossDerivative(i, j, dvR01, dvR12, dvR03);
                    }
                    else if (i == 3) // A components
                    {
                        mHess[i, i] = OptimalGainTetranomialTimeOfOnsetACurvature(dvR01, dvR12, dvR03);
                        vGrad[i] = OptimalGainTetranomialTimeOfOnsetASlope(dvR01, dvR12, dvR03);
                        for (int j = 0; j < 3; j++)
                            mHess[i, j] = mHess[j, i] = OptimalGainTetranomialTimeOfOnsetAGainCrossDerivative(j, dvR01, dvR12, dvR03);
                    }
                    else if (i == 4) // B components
                    {
                        mHess[i, i] = OptimalGainTetranomialTimeOfOnsetBCurvature(dvR01, dvR12, dvR03);
                        vGrad[i] = OptimalGainTetranomialTimeOfOnsetBSlope(dvR01, dvR12, dvR03);
                        mHess[3, 4] = mHess[4, 3] = OptimalGainTetranomialTimeOfOnsetABCrossDerivative(dvR01, dvR12, dvR03);
                        for (int j = 0; j < 3; j++)
                            mHess[i, j] = mHess[j, i] = OptimalGainTetranomialTimeOfOnsetBGainCrossDerivative(j, dvR01, dvR12, dvR03);
                    }
                }

                // solve the system
                Vector vSol = mHess.Solve(vGrad, false);

                // update the solution
                double dSupNorm = new double();
                double dIncrement = new double();
                for (int i = 0; i < 3; i++)
                {
                    dSupNorm += Math.Abs(vSol[i]);
                    dIncrement = dDamping * vSol[i];
                    m_dvGain[i] -= dIncrement;
                }

                dSupNorm += Math.Abs(vSol[3]);
                dIncrement = dDamping * vSol[3];
                m_dACoefficient -= dIncrement;

                dSupNorm += Math.Abs(vSol[4]);
                dIncrement = dDamping * vSol[4];
                m_dBCoefficient -= dIncrement;

                double dLL = OptimalGainTetranomialTimeOfOnsetLogLikelihood(m);

                if (dSupNorm < dEpsilon)
                    break;
            } // iteration

            if (iIteration >= iMaxIteration - 1)
                throw new DCSException("Newton iteration failed to converce in EE1nt.EE1ntDiveData.OptimalGainTetranomialTimeOfOnsetOptimize");
        } // verified

        public double OptimalGainTetranomialTimeOfOnsetOptimizeReturnLogLikelihood(MODEL m)
        {
            // calculate R once
            double[,] dvR01 = new double[this.m_dvGain.Length, this.Profiles];
            double[,] dvR12 = new double[this.m_dvGain.Length, this.Profiles];
            double[,] dvR03 = new double[this.m_dvGain.Length, this.Profiles];

            OptimalGainTetranomialTimeOfOnsetHazard(dvR01, dvR12, dvR03, m);

            // control parameters
            double dDamping = new double();
            double dEpsilon = new double();
            int iIteration = new int();
            int iMaxIteration = new int();

            dDamping = 0.5;
            dEpsilon = 1.0e-12;
            iMaxIteration = 500;

            GeneralMatrix mHess = new GeneralMatrix(2 + m_dvGain.Length, 2 + m_dvGain.Length);
            GeneralVector vGrad = new GeneralVector(2 + m_dvGain.Length);

            for (iIteration = 0; iIteration < iMaxIteration; iIteration++)
            {
                double[] dvGGrad = OptimalGainTetranomialTimeOfOnsetGainSlope(dvR01, dvR12, dvR03);
                double[] dvGCurv = OptimalGainTetranomialTimeOfOnsetGainCurvature(dvR01, dvR12, dvR03);

                // load the Hessian and gradient components
                for (int i = 0; i < 2 + m_dvGain.Length; i++)
                {
                    if (i < 3) // gain components
                    {
                        mHess[i, i] = dvGCurv[i];
                        vGrad[i] = dvGGrad[i];
                        for (int j = i + 1; j < 3; j++)
                            mHess[i, j] = mHess[j, i] = OptimalGainTetranomialTimeOfOnsetGainCrossDerivative(i, j, dvR01, dvR12, dvR03);
                    }
                    else if (i == 3) // A components
                    {
                        mHess[i, i] = OptimalGainTetranomialTimeOfOnsetACurvature(dvR01, dvR12, dvR03);
                        vGrad[i] = OptimalGainTetranomialTimeOfOnsetASlope(dvR01, dvR12, dvR03);
                        for (int j = 0; j < 3; j++)
                            mHess[i, j] = mHess[j, i] = OptimalGainTetranomialTimeOfOnsetAGainCrossDerivative(j, dvR01, dvR12, dvR03);
                    }
                    else if (i == 4) // B components
                    {
                        mHess[i, i] = OptimalGainTetranomialTimeOfOnsetBCurvature(dvR01, dvR12, dvR03);
                        vGrad[i] = OptimalGainTetranomialTimeOfOnsetBSlope(dvR01, dvR12, dvR03);
                        mHess[3, 4] = mHess[4, 3] = OptimalGainTetranomialTimeOfOnsetABCrossDerivative(dvR01, dvR12, dvR03);
                        for (int j = 0; j < 3; j++)
                            mHess[i, j] = mHess[j, i] = OptimalGainTetranomialTimeOfOnsetBGainCrossDerivative(j, dvR01, dvR12, dvR03);
                    }
                }

                // solve the system
                Vector vSol = mHess.Solve(vGrad, false);

                // update the solution
                double dSupNorm = new double();
                double dIncrement = new double();
                for (int i = 0; i < 3; i++)
                {
                    if (double.IsNaN(vSol[i]))
                        throw new DCSException("vSol.IsNaN in EE1nt.EE1ntDiveData.OptimalGainTetranomialTimeOfOnsetOptimize");
                    dSupNorm += Math.Abs(vSol[i]);
                    dIncrement = dDamping * vSol[i];
                    m_dvGain[i] -= dIncrement;
                }

                dSupNorm += Math.Abs(vSol[3]);
                dIncrement = dDamping * vSol[3];
                m_dACoefficient -= dIncrement;

                dSupNorm += Math.Abs(vSol[4]);
                dIncrement = dDamping * vSol[4];
                m_dBCoefficient -= dIncrement;

                double dLL = OptimalGainTetranomialTimeOfOnsetLogLikelihood(m);

                if (dSupNorm < dEpsilon)
                    break;
            } // iteration

            if (iIteration >= iMaxIteration - 1)
                throw new DCSException("Newton iteration failed to converge in EE1nt.EE1ntDiveData.OptimalGainTetranomialTimeOfOnsetOptimize");

            return OptimalGainTetranomialTimeOfOnsetLogLikelihood(m, dvR01, dvR12, dvR03);
        } // verified

#endif
    }
}