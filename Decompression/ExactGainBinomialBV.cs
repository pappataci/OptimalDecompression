using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using DCSUtilities;
using Extreme.Mathematics;
using Extreme.Mathematics.EquationSolvers;
using Extreme.Mathematics.LinearAlgebra.IterativeSolvers.Preconditioners;

namespace Decompression
{
    /// <summary>
    /// Static class to calculate the exact gain for binary bubble volume problems
    /// </summary>
    public static class ExactGainBinomialBV
    {

        public static OPTIMIZATIONTYPE OptimizationType = OPTIMIZATIONTYPE.TIMEOFONSETNOMARGINAL;

        private static double[] m_dvGain;
        private static DiveDataCondition<ProfileCondition<NodeBubble>, NodeBubble> m_Data;
        private static int m_iNumberOfRestarts = 128;
        private static double[] dvScale = new double[NodeTissue.NumberOfTissues];

        public static double[] Gain { set { m_dvGain = value; } get { return m_dvGain; } }

        public static int Restarts { set { m_iNumberOfRestarts = value; } get { return m_iNumberOfRestarts; } }

        public static DiveDataCondition<ProfileCondition<NodeBubble>, NodeBubble> Data { set { m_Data = value; } get { return m_Data; } }

        public static double[] RandomInitialGainVector ( )
        {

            double[] vec = new double[NodeTissue.NumberOfTissues];
            Random r     = new Random ( );

            for (int i = 0; i < vec.Length; i++)
                vec[i] = 1.0e-6 + ( 1.0e-3 - 1.0e-6 ) * r.NextDouble ( );

            return vec;

        }

        #region binary, incidence only, no fractional weight

        private static Vector Equation89RHS ( )
        {

            var dvSumRcz = Vector.Create ( NodeTissue.NumberOfTissues );

            foreach (int i in m_Data.NoDCSProfileIndicies)
            {

                var p = m_Data[i];

                var dvR = p.FinalNode.IntegratedRisk;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcz[c] += p.Divers * dvR[c];

            }

            return dvSumRcz;

        }

        private static Vector Equation89LHS ( )
        {

            var dvSumRcs = Vector.Create ( NodeTissue.NumberOfTissues );

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p = m_Data[i];

                var dvR = p.FinalNode.IntegratedRisk;
                var dXi_s = 0.0;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXi_s += m_dvGain[c] * dvR[c];

                dXi_s = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, 1.0 / ( Math.Exp ( dXi_s ) - 1.0 ) );
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcs[c] += p.Divers * dvR[c] * dXi_s;

            }

            return dvSumRcs;

        }

        private static Vector Equation90 ( )
        {

            var dvSum = Vector.Create ( NodeTissue.NumberOfTissues );

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p = m_Data[i];

                var dvR = p.FinalNode.IntegratedRisk;
                var dXi_s = 0.0;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXi_s += m_dvGain[c] * dvR[c];

                dXi_s = Math.Exp ( dXi_s );

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSum[c] -= p.Divers * dXi_s * Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, Math.Pow ( dvR[c] / ( dXi_s - 1.0 ), 2.0 ) );

            }

            return dvSum;

        }

        private static double[,] Equation91 ( )
        {

            var dmSum = new double[NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues];

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p = m_Data[i];

                var dvR = p.FinalNode.IntegratedRisk;
                var dXi_s = 0.0;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXi_s += m_dvGain[c] * dvR[c];

                dXi_s = Math.Exp ( dXi_s );

                for (int c1 = 0; c1 < NodeTissue.NumberOfTissues; c1++)
                    for (int c2 = 0; c2 < NodeTissue.NumberOfTissues; c2++)
                        dmSum[c1, c2] -= p.Divers *
                                            Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon,
                                                ( dvR[c1] * dvR[c2] * dXi_s ) /
                                                Math.Pow ( dXi_s - 1.0, 2.0 ) );

            }

            return dmSum;

        }

        #endregion binary, incidence only, no fractional weight

        #region binary, incidence only, fractional weight

        private static Vector Equation95RHS ( )
        {

            //var actions = new Action [ 2 ];

            var dvSumRcz = Vector.Create ( NodeTissue.NumberOfTissues );

            //actions [ 0 ] = ( ( ) =>
            //{

            foreach (int i in m_Data.NoDCSProfileIndicies)
            {

                var p = m_Data[i];

                var dvR = p.FinalNode.IntegratedRisk;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcz[c] += p.Divers * dvR[c];
            }

            //} );

            var dvSumRcn = Vector.Create ( NodeTissue.NumberOfTissues );
            //actions [ 1 ] = ( ( ) =>
            //{
            foreach (int i in m_Data.MarginalDCSProfileIndicies)
            {

                var p = m_Data[i];

                var dvR = p.FinalNode.IntegratedRisk;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcn[c] += p.Divers * ( 1.0 - p.DCS ) * dvR[c];

            }

            //} );

            //Parallel.Invoke ( actions );

            return Vector.Add ( dvSumRcz, dvSumRcn );

        }

        private static Vector Equation95LHS ( )
        {

            //var actions = new Action [ 2 ];

            var dvSumRcs = Vector.Create ( NodeTissue.NumberOfTissues );

            //actions [ 0 ] = ( ( ) =>
            //{

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {
                var p = m_Data[i];

                var dvR = p.FinalNode.IntegratedRisk;
                var dXi_s = 0.0;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXi_s += m_dvGain[c] * dvR[c];

                dXi_s = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, 1.0 / ( Math.Exp ( dXi_s ) - 1.0 ) );
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcs[c] += p.Divers * dvR[c] * dXi_s;
            }

            //} );

            var dvSumRcn = Vector.Create ( NodeTissue.NumberOfTissues );

            //actions [ 1 ] = ( ( ) =>
            //{

            foreach (int i in m_Data.MarginalDCSProfileIndicies)
            {
                var p = m_Data[i];

                var dvR = p.FinalNode.IntegratedRisk;
                var dXi_s = 0.0;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXi_s += m_dvGain[c] * dvR[c];

                dXi_s = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, 1.0 / ( Math.Exp ( dXi_s ) - 1.0 ) );
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcn[c] += p.Divers * p.DCS * dvR[c] * dXi_s;
            }

            //} );

            //Parallel.Invoke ( actions );

            return Vector.Add ( dvSumRcs, dvSumRcn );

        }

        private static Vector Equation96 ( )
        {

            //var actions = new Action [ 2 ];

            var dvSumRcs = Vector.Create ( NodeTissue.NumberOfTissues );

            //actions [ 0 ] = ( ( ) =>
            //{

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {
                var p = m_Data[i];

                var dvR = p.FinalNode.IntegratedRisk;
                var dXi = 0.0;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXi += m_dvGain[c] * dvR[c];

                dXi = Math.Exp ( dXi );

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcs[c] -= p.Divers * dXi *
                                      Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon,
                                          Math.Pow ( dvR[c] / ( dXi - 1.0 ), 2.0 ) );
            }

            //} );

            var dvSumRcn = Vector.Create ( NodeTissue.NumberOfTissues );

            //actions [ 1 ] = ( ( ) =>
            //{

            foreach (int i in m_Data.MarginalDCSProfileIndicies)
            {
                var p = m_Data[i];

                var dvR = p.FinalNode.IntegratedRisk;
                var dXi = 0.0;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXi += m_dvGain[c] * dvR[c];

                dXi = Math.Exp ( dXi );

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcn[c] -= p.Divers * p.DCS * dXi *
                                      Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon,
                                          Math.Pow ( dvR[c] / ( dXi - 1.0 ), 2.0 ) );
            }

            //} );

            //Parallel.Invoke ( actions );

            return Vector.Add ( dvSumRcs, dvSumRcn );

        }

        private static double[,] Equation97 ( )
        {

            //var actions = new Action [ 2 ];

            var dmSumRcs = new double[NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues];

            //actions [ 0 ] = ( ( ) =>
            //{

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {
                var p = m_Data[i];

                var dvR = p.FinalNode.IntegratedRisk;
                var dXi = 0.0;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXi += m_dvGain[c] * dvR[c];

                dXi = Math.Exp ( dXi );

                for (int c1 = 0; c1 < NodeTissue.NumberOfTissues; c1++)
                    for (int c2 = 0; c2 < NodeTissue.NumberOfTissues; c2++)
                        dmSumRcs[c1, c2] -= p.Divers *
                                               Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon,
                                                   ( dvR[c1] * dvR[c2] * dXi ) /
                                                   Math.Pow ( dXi - 1.0, 2.0 ) );
            }

            //} );

            var dmSumRcn = new double[NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues];

            //actions [ 1 ] = ( ( ) =>
            //{

            foreach (int i in m_Data.MarginalDCSProfileIndicies)
            {
                var p = m_Data[i];

                var dvR = p.FinalNode.IntegratedRisk;
                var dXi = 0.0;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXi += m_dvGain[c] * dvR[c];

                dXi = Math.Exp ( dXi );

                for (int c1 = 0; c1 < NodeTissue.NumberOfTissues; c1++)
                    for (int c2 = 0; c2 < NodeTissue.NumberOfTissues; c2++)
                        dmSumRcn[c1, c2] -= p.Divers * p.DCS *
                                               Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon,
                                                   ( dvR[c1] * dvR[c2] * dXi ) /
                                                   Math.Pow ( dXi - 1.0, 2.0 ) );
            }

            //} );

            //Parallel.Invoke ( actions );

            var dmSum = new double[NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues];
            for (int c1 = 0; c1 < NodeTissue.NumberOfTissues; c1++)
                for (int c2 = 0; c2 < NodeTissue.NumberOfTissues; c2++)
                    dmSum[c1, c2] = dmSumRcs[c1, c2] + dmSumRcn[c1, c2];

            return dmSum;


        }

        #endregion binary, incidence only, fractional weight

        #region binary, time of onset, no fractional weight

        public static Vector Equation100RHS ( )
        {

            //var actions = new Action [ 2 ];

            var dvSumRcs = Vector.Create ( NodeTissue.NumberOfTissues );
            //actions [ 0 ] = ( ( ) =>
            //{
            foreach (int i in m_Data.FullDCSProfileIndicies)
            {
                var p = m_Data[i];
                var dvRcs01 = p.Time1Node.IntegratedRisk;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcs[c] += p.Divers * dvRcs01[c];
            }
            //} );

            var dvSumRcz = Vector.Create ( NodeTissue.NumberOfTissues );
            //actions [ 1 ] = ( ( ) =>
            //{
            foreach (int i in m_Data.NoDCSProfileIndicies)
            {
                var p = m_Data[i];
                var dvRcz03 = p.FinalNode.IntegratedRisk;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcz[c] += p.Divers * dvRcz03[c];
            }
            //} );

            //Parallel.Invoke ( actions );

            return Vector.Add ( dvSumRcs, dvSumRcz );

        }

        public static Vector Equation100LHS ( )
        {

            var dvSumLHS = Vector.Create ( NodeTissue.NumberOfTissues );

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p = m_Data[i];
                var dvRcs02 = p.Time2Node.IntegratedRisk;
                var dvRcs01 = p.Time1Node.IntegratedRisk;
                var dXis12 = 0.0;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXis12 += m_dvGain[c] * ( dvRcs02[c] - dvRcs01[c] );

                dXis12 = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, 1.0 / ( Math.Exp ( dXis12 ) - 1.0 ) );

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumLHS[c] += p.Divers * ( dvRcs02[c] - dvRcs01[c] ) * dXis12;

            }

            return dvSumLHS;

        }

        public static Vector Equation101 ( )
        {

            var dvSum = Vector.Create ( NodeTissue.NumberOfTissues );

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p       = m_Data[i];
                var dvRcs02 = p.Time2Node.IntegratedRisk;
                var dvRcs01 = p.Time1Node.IntegratedRisk;
                var dXis12  = 0.0;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXis12 += m_dvGain[c] * ( dvRcs02[c] - dvRcs01[c] );

                dXis12 = Math.Exp ( dXis12 );

                var dDenom = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, 1.0 / ( dXis12 - 1.0 ) );

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSum[c] -= p.Divers * Math.Pow ( dDenom * ( dvRcs02[c] - dvRcs01[c] ), 2.0 ) * dXis12;

            }

            return dvSum;

        }

        public static double[,] Equation102 ( )
        {

            var dmSum = new double[NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues];

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p       = m_Data[i];
                var dvRcs02 = p.Time2Node.IntegratedRisk;
                var dvRcs01 = p.Time1Node.IntegratedRisk;
                var dXis12  = 0.0;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXis12 += m_dvGain[c] * ( dvRcs02[c] - dvRcs01[c] );

                dXis12 = Math.Exp ( dXis12 );

                double dDenom = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, 1.0 / Math.Pow ( ( dXis12 - 1.0 ), 2.0 ) );

                for (int c1 = 0; c1 < NodeTissue.NumberOfTissues; c1++)
                    for (int c2 = 0; c2 < NodeTissue.NumberOfTissues; c2++)
                        dmSum[c1, c2] -= p.Divers * dDenom * ( dvRcs02[c1] - dvRcs01[c1] )
                                            * ( dvRcs02[c2] - dvRcs01[c2] ) * dXis12;

            }

            return dmSum;

        }

        #endregion binary, time of onset, no fractional weight

        #region binary, time of onset, fractional weight

        private static Vector Equation103cRHS ( )
        {

            //var actions = new Action [ 3 ];

            var dvSumRcs = Vector.Create ( NodeTissue.NumberOfTissues );

            //actions [ 0 ] = new Action ( ( ) =>
            //{

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {
                var p = m_Data[i];

                var dvRcs01 = p.Time1Node.IntegratedRisk;
                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcs[c] += p.Divers * dvRcs01[c];

            }

            //} );

            var dvSumRcn = Vector.Create ( NodeTissue.NumberOfTissues );

            //actions [ 1 ] = new Action ( ( ) =>
            //{

            foreach (int i in m_Data.MarginalDCSProfileIndicies)
            {

                var p       = m_Data[i];
                var dvRcn03 = p.FinalNode.IntegratedRisk;
                var dvRcn01 = p.Time1Node.IntegratedRisk;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcn[c] += p.Divers *
                                      ( dvRcn03[c] - p.DCS * ( dvRcn03[c] - dvRcn01[c] ) );

            }
            //} );

            var dvSumRcz = Vector.Create ( NodeTissue.NumberOfTissues );
            //actions [ 2 ] = new Action ( ( ) =>
            //{
            foreach (int i in m_Data.NoDCSProfileIndicies)
            {

                var p       = m_Data[i];
                var dvRcz03 = p.FinalNode.IntegratedRisk;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcz[c] += p.Divers * dvRcz03[c];

            }
            //} );

            //Parallel.Invoke ( actions );

            return Vector.Add ( dvSumRcn, Vector.Add ( dvSumRcz, dvSumRcs ) );

        }

        private static Vector Equation103cLHS ( )
        {

            //var actions = new Action [ 2 ];

            var dvSumRcs = Vector.Create ( NodeTissue.NumberOfTissues );

            //actions [ 0 ] = new Action ( ( ) =>
            //{

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p       = m_Data[i];
                var dvRcs02 = p.Time2Node.IntegratedRisk;
                var dvRcs01 = p.Time1Node.IntegratedRisk;
                var dXis12  = 0.0;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXis12 += m_dvGain[c] * ( dvRcs02[c] - dvRcs01[c] );

                dXis12 = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, 1.0 / ( Math.Exp ( dXis12 ) - 1.0 ) );

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcs[c] += p.Divers * ( dvRcs02[c] - dvRcs01[c] ) * dXis12;

            }

            //} );

            var dvSumRcn = Vector.Create ( NodeTissue.NumberOfTissues );

            //actions [ 1 ] = new Action ( ( ) =>
            //{

            foreach (int i in m_Data.MarginalDCSProfileIndicies)
            {

                var p       = m_Data[i];
                var dvRcn02 = p.Time2Node.IntegratedRisk;
                var dvRcn01 = p.Time1Node.IntegratedRisk;
                var dXin12  = 0.0;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXin12 += m_dvGain[c] * ( dvRcn02[c] - dvRcn01[c] );

                dXin12 = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, 1.0 / ( Math.Exp ( dXin12 ) - 1.0 ) );

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcn[c] += p.Divers * p.DCS * ( dvRcn02[c] - dvRcn01[c] ) * dXin12;
            }

            //} );

            //Parallel.Invoke ( actions );

            return Vector.Add ( dvSumRcs, dvSumRcn );

        }

        private static Vector Equation104 ( )
        {

            //var actions = new Action [ 2 ];

            var dvSumRcs = Vector.Create ( NodeTissue.NumberOfTissues );

            //actions [ 0 ] = new Action ( ( ) =>
            //{

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p       = m_Data[i];
                var dvRcs02 = p.Time2Node.IntegratedRisk;
                var dvRcs01 = p.Time1Node.IntegratedRisk;
                var dXis12  = 0.0;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXis12 += m_dvGain[c] * ( dvRcs02[c] - dvRcs01[c] );

                dXis12 = Math.Exp ( dXis12 );

                var dDenom = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, 1.0 / ( dXis12 - 1.0 ) );

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcs[c] -= p.Divers *
                                      Math.Pow ( dDenom * ( dvRcs02[c] - dvRcs01[c] ), 2.0 ) * dXis12;
            }

            //} );

            var dvSumRcn = Vector.Create ( NodeTissue.NumberOfTissues );

            //actions [ 1 ] = new Action ( ( ) =>
            //{

            foreach (int i in m_Data.MarginalDCSProfileIndicies)
            {

                var p       = m_Data[i];
                var dvRcn02 = p.Time2Node.IntegratedRisk;
                var dvRcn01 = p.Time1Node.IntegratedRisk;
                var dXin12  = 0.0;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXin12 += m_dvGain[c] * ( dvRcn02[c] - dvRcn01[c] );

                dXin12 = Math.Exp ( dXin12 );

                var dDenom = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, 1.0 / ( dXin12 - 1.0 ) );

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumRcn[c] -= p.Divers * p.DCS *
                                      Math.Pow ( dDenom * ( dvRcn02[c] - dvRcn01[c] ), 2.0 ) * dXin12;
            }

            //} );

            //Parallel.Invoke ( actions );

            return Vector.Add ( dvSumRcs, dvSumRcn );

        }

        private static double[,] Equation105 ( )
        {

            //var actions = new Action [ 2 ];

            var dvSumRcs = new double[NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues];

            //actions [ 0 ] = new Action ( ( ) =>
            //{

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p       = m_Data[i];
                var dvRcs02 = p.Time2Node.IntegratedRisk;
                var dvRcs01 = p.Time1Node.IntegratedRisk;
                var dXis12  = 0.0;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXis12 += m_dvGain[c] * ( dvRcs02[c] - dvRcs01[c] );

                dXis12 = Math.Exp ( dXis12 );

                var dDenom = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, 1.0 / Math.Pow ( ( dXis12 - 1.0 ), 2.0 ) );

                for (int c1 = 0; c1 < NodeTissue.NumberOfTissues; c1++)
                    for (int c2 = 0; c2 < NodeTissue.NumberOfTissues; c2++)
                        dvSumRcs[c1, c2] -= p.Divers * dDenom * ( dvRcs02[c1] - dvRcs01[c1] )
                                               * ( dvRcs02[c2] - dvRcs01[c2] ) * dXis12;

            }

            //} );

            var dvSumRcn = new double[NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues];

            //actions [ 1 ] = new Action ( ( ) =>
            //{

            foreach (int i in m_Data.MarginalDCSProfileIndicies)
            {

                var p       = m_Data[i];
                var dvRcn02 = p.Time2Node.IntegratedRisk;
                var dvRcn01 = p.Time1Node.IntegratedRisk;
                var dXin12  = 0.0;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXin12 += m_dvGain[c] * ( dvRcn02[c] - dvRcn01[c] );

                dXin12 = Math.Exp ( dXin12 );

                var dDenom = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, 1.0 / Math.Pow ( ( dXin12 - 1.0 ), 2.0 ) );

                for (int c1 = 0; c1 < NodeTissue.NumberOfTissues; c1++)
                    for (int c2 = 0; c2 < NodeTissue.NumberOfTissues; c2++)
                        dvSumRcn[c1, c2] -= p.Divers * p.DCS * dDenom *
                                               ( dvRcn02[c1] - dvRcn01[c1] )
                                               * ( dvRcn02[c2] - dvRcn01[c2] ) * dXin12;

            }

            //} );

            //Parallel.Invoke ( actions );

            var dvSum = new double[NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues];
            for (int c1 = 0; c1 < NodeTissue.NumberOfTissues; c1++)
                for (int c2 = 0; c2 < NodeTissue.NumberOfTissues; c2++)
                    dvSum[c1, c2] = dvSumRcs[c1, c2] + dvSumRcn[c1, c2];

            return dvSum;

        }

        #endregion binary, time of onset, fractional weight

        public static double[] Optimize ( )
        {

            // Assign dummy Funcs - these are reset in the switch statement.
            Func<Vector> sumRHS = ( ) => new double[NodeTissue.NumberOfTissues];
            Func<Vector> sumLHS = ( ) => new double[NodeTissue.NumberOfTissues];
            Func<double[,]> Jacobian = ( ) => new double[NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues];

            switch (OptimizationType)
            {

                case OPTIMIZATIONTYPE.INCIDENCONLYNOMARGINAL:
                    sumRHS = ( ) => Equation89RHS ( );
                    sumLHS = ( ) => Equation89LHS ( );
                    Jacobian = ( ) => Equation91 ( );
                    break;

                case OPTIMIZATIONTYPE.INCIDENCEONLYFRACTIONALMARGINAL:
                    sumRHS = ( ) => Equation95RHS ( );
                    sumLHS = ( ) => Equation95LHS ( );
                    Jacobian = ( ) => Equation97 ( );
                    break;

                case OPTIMIZATIONTYPE.TIMEOFONSETNOMARGINAL:
                    sumRHS = ( ) => Equation100RHS ( );
                    sumLHS = ( ) => Equation100LHS ( );
                    Jacobian = ( ) => Equation102 ( );
                    break;

                case OPTIMIZATIONTYPE.TIMEOFONSETFRACTIONALMARGINAL:
                    sumRHS = ( ) => Equation103cRHS ( );
                    sumLHS = ( ) => Equation103cLHS ( );
                    Jacobian = ( ) => Equation105 ( );
                    break;

                default:
                    throw new DCSException ( "Bad OPTIMIZATIONTYPE enumeration value in Decompression.ExactGainBinomialBV.OptimizeGain" );
            }

            #region assign lambda functions

            // Non-persistent counter included in the lambda closure.
            int cVal = new int ( );

            // Calculate the right hand side once.
            Vector dvSumRHS = sumRHS ( );
            Vector dvSumLHS = new double[NodeTissue.NumberOfTissues];

            // The multi-dimensional nonlinear system.
            Func<Vector, double>[] f = new Func<Vector, double>[NodeTissue.NumberOfTissues];
            for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                if (c == 0)
                    f[c] = ( x ) =>
                    {
                        cVal = 0;
                        m_dvGain = x.ToArray ( );
                        dvSumLHS = sumLHS ( );
                        return dvSumLHS[cVal] - dvSumRHS[cVal];
                    };
                else
                    f[c] = ( x ) =>
                    {
                        cVal++;
                        return dvSumLHS[cVal] - dvSumRHS[cVal];
                    };

            // The Jacobian of the non-linear system.
            double[,] dmJac = new double[NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues];
            Func<Vector, Vector, Vector>[] df = new Func<Vector, Vector, Vector>[NodeTissue.NumberOfTissues];
            for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                if (c == 0)
                    df[c] = ( x, y ) =>
                    {
                        cVal = 0;
                        m_dvGain = x.ToArray ( );
                        dmJac = Jacobian ( );
                        for (int d = 0; d < NodeTissue.NumberOfTissues; d++)
                            y[d] = dmJac[cVal, d];
                        return y;
                    };
                else
                    df[c] = ( x, y ) =>
                    {
                        cVal++;
                        for (int d = 0; d < NodeTissue.NumberOfTissues; d++)
                            y[d] = dmJac[cVal, d];
                        return y;
                    };

            #endregion assign lambda functions

            Vector initialGuess = Vector.Create ( NodeTissue.NumberOfTissues );
            for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                initialGuess[c] = m_dvGain[c];

            DoglegSystemSolver solver                = new DoglegSystemSolver ( f, df, initialGuess );
            solver.TrustRegionRadius                 = 1.0e-3;
            solver.MaxIterations                     = 256;
            solver.ValueTest.Tolerance               = 1e-6;
            solver.ValueTest.Norm                    = Extreme.Mathematics.Algorithms.VectorConvergenceNorm.Maximum;
            solver.SolutionTest.Tolerance            = 1e-6;
            solver.SolutionTest.ConvergenceCriterion = ConvergenceCriterion.WithinRelativeTolerance;

            int n = 0;
            do
            {
                if (n++ > m_iNumberOfRestarts)
                    throw new DCSException ( "Too many restarts required in Decompression.ExactGainBinomialBV.OptimizeGain" );

                solver.InitialGuess = initialGuess;
                try
                {
                    Vector solution = solver.Solve ( );
                    for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                        m_dvGain[c] = solution[c];
                }
                catch (Exception e)
                {
                    throw new DCSException ( "Solver exception thrown in Decompression.ExactGainBinomialBV.Optimize " );
                }

                initialGuess = ExactGainBinomialBV.RandomInitialGainVector ( );
            } while (solver.Status != AlgorithmStatus.Converged);

            return m_dvGain;

        }

    }

}
