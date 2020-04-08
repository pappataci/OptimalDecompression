using System;
using System.Threading.Tasks;
using DCSUtilities;
using Extreme.Mathematics;
using Extreme.Mathematics.EquationSolvers;
using System.Diagnostics;
using Decompression;

namespace Decompression
{
    public static class ExactGainTrinomial
    {


        private static int m_iNumberOfRestarts          = 32;
        private static double [ ] dvScale               = new double [ NodeTissue.NumberOfTissues ];
        private static double m_dTrinomialScaleFactor   = 0.25;
        private static bool m_bUseFailureTimes          = true;
        public static OPTIMIZATIONTYPE OptimizationType = OPTIMIZATIONTYPE.TIMEOFONSETFRACTIONALMARGINAL;

        private static double [ ] m_dvGain;
        private static DiveDataCondition<ProfileCondition<NodeCondition>, NodeCondition> m_Data;

        public static double [ ] Gain { set { m_dvGain = value; } get { return m_dvGain; } }
        public static int Restarts { set { m_iNumberOfRestarts = value; } get { return m_iNumberOfRestarts; } }
        public static double TrinomialScaleFactor { set { m_dTrinomialScaleFactor = value; } get { return m_dTrinomialScaleFactor; } }
        public static DiveDataCondition<ProfileCondition<NodeCondition>, NodeCondition> Data { set { m_Data = value; } get { return m_Data; } }
        public static bool UseFailureTimes { set { m_bUseFailureTimes = value; } get { return m_bUseFailureTimes; } }

        private static double [ ] m_dvP0;
        private static double [ ] m_dvPM;
        private static double [ ] m_dvPS;
        
        private static double [ ] RandomInitialGainVector ( )
        {
            Random r = new Random ( );
            double [ ] vec = new double [ NodeTissue.NumberOfTissues + 1 ];
            for ( int i = 0; i < vec.Length - 1; i++ )
                vec [ i ] = 1.0e-5 + ( 1.0e-2 - 1.0e-5 ) * r.NextDouble ( );

            vec [ vec.Length - 1 ] = 0.1 + ( 0.5 - 0.1 ) * r.NextDouble ( );

            return vec;
        }
#if false
        #region trinomial, incidence only, no fractional weight

        //private static Vector Equation89RHS ( )
        //{
        //    var dvSumRcz = Vector.Create ( NodeTissue.NumberOfTissues );

        //    foreach ( int i in m_Data.NoDCSProfileIndicies )
        //    {
        //        ProfileCondition<NodeCondition> p = m_Data [ i ];

        //        var dvR = p.FinalNode.IntegratedRisk;
        //        for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //            dvSumRcz [ c ] += p.Divers * dvR [ c ];
        //    }

        //    return dvSumRcz;
        //}

        //private static Vector Equation89LHS ( )
        //{
        //    var dvSumRcs = Vector.Create ( NodeTissue.NumberOfTissues );

        //    foreach ( int i in m_Data.FullDCSProfileIndicies )
        //    {
        //        ProfileCondition<NodeCondition> p = m_Data [ i ];

        //        var dvR = p.FinalNode.IntegratedRisk;
        //        var dXi_s = 0.0;
        //        for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //            dXi_s += m_dvGain [ c ] * dvR [ c ];

        //        dXi_s = 1.0 / ( Math.Exp ( dXi_s ) - 1.0 );
        //        for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //            dvSumRcs [ c ] += p.Divers * dvR [ c ] * dXi_s;
        //    }

        //    return dvSumRcs;
        //}

        //private static Vector Equation90 ( )
        //{
        //    var dvSum = Vector.Create ( NodeTissue.NumberOfTissues );

        //    foreach ( int i in m_Data.FullDCSProfileIndicies )
        //    {
        //        ProfileCondition<NodeCondition> p = m_Data [ i ];

        //        var dvR = p.FinalNode.IntegratedRisk;
        //        var dXi_s = 0.0;
        //        for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //            dXi_s += m_dvGain [ c ] * dvR [ c ];

        //        dXi_s = Math.Exp ( dXi_s );

        //        for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //            dvSum [ c ] -= p.Divers * dXi_s * Math.Pow ( dvR [ c ] / ( dXi_s - 1.0 ), 2.0 );
        //    }

        //    return dvSum;
        //}

        //private static double [ , ] Equation91 ( )
        //{
        //    var dmSum = new double [ NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues ];

        //    foreach ( int i in m_Data.FullDCSProfileIndicies )
        //    {
        //        ProfileCondition<NodeCondition> p = m_Data [ i ];

        //        var dvR = p.FinalNode.IntegratedRisk;
        //        var dXi_s = 0.0;
        //        for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //            dXi_s += m_dvGain [ c ] * dvR [ c ];

        //        dXi_s = Math.Exp ( dXi_s );

        //        for ( int c1 = 0; c1 < NodeTissue.NumberOfTissues; c1++ )
        //            for ( int c2 = 0; c2 < NodeTissue.NumberOfTissues; c2++ )
        //                dmSum [ c1, c2 ] -= p.Divers * ( dvR [ c1 ] * dvR [ c2 ] * dXi_s ) / Math.Pow ( dXi_s - 1.0, 2.0 );
        //    }

        //    return dmSum;
        //}

        #endregion trinomial, incidence only, no fractional weight

        #region trinomial, incidence only, fractional weight

        //private static Vector Equation95RHS ( )
        //{
        //    var actions = new Action [ 2 ];

        //    var dvSumRcz = Vector.Create ( NodeTissue.NumberOfTissues );
        //    actions [ 0 ] = ( ( ) =>
        //    {
        //        foreach ( int i in m_Data.NoDCSProfileIndicies )
        //        {
        //            ProfileCondition<NodeCondition> p = m_Data [ i ];

        //            var dvR = p.FinalNode.IntegratedRisk;
        //            for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //                dvSumRcz [ c ] += p.Divers * dvR [ c ];
        //        }
        //    } );

        //    var dvSumRcn = Vector.Create ( NodeTissue.NumberOfTissues );
        //    actions [ 1 ] = ( ( ) =>
        //    {
        //        foreach ( int i in m_Data.MarginalDCSProfileIndicies )
        //        {
        //            ProfileCondition<NodeCondition> p = m_Data [ i ];

        //            var dvR = p.FinalNode.IntegratedRisk;
        //            for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //                dvSumRcn [ c ] += p.Divers * ( 1.0 - p.DCS ) * dvR [ c ];
        //        }
        //    } );

        //    Parallel.Invoke ( actions );

        //    return Vector.Add ( dvSumRcz, dvSumRcn );
        //}

        //private static Vector Equation95LHS ( )
        //{
        //    var actions = new Action [ 2 ];

        //    var dvSumRcs = Vector.Create ( NodeTissue.NumberOfTissues );
        //    actions [ 0 ] = ( ( ) =>
        //    {
        //        foreach ( int i in m_Data.FullDCSProfileIndicies )
        //        {
        //            ProfileCondition<NodeCondition> p = m_Data [ i ];

        //            var dvR = p.FinalNode.IntegratedRisk;
        //            var dXi_s = 0.0;
        //            for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //                dXi_s += m_dvGain [ c ] * dvR [ c ];

        //            dXi_s = 1.0 / ( Math.Exp ( dXi_s ) - 1.0 );
        //            for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //                dvSumRcs [ c ] += p.Divers * dvR [ c ] * dXi_s;
        //        }
        //    } );

        //    var dvSumRcn = Vector.Create ( NodeTissue.NumberOfTissues );
        //    actions [ 1 ] = ( ( ) =>
        //    {
        //        foreach ( int i in m_Data.MarginalDCSProfileIndicies )
        //        {
        //            ProfileCondition<NodeCondition> p = m_Data [ i ];

        //            var dvR = p.FinalNode.IntegratedRisk;
        //            var dXi_s = 0.0;
        //            for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //                dXi_s += m_dvGain [ c ] * dvR [ c ];

        //            dXi_s = 1.0 / ( Math.Exp ( dXi_s ) - 1.0 );
        //            for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //                dvSumRcn [ c ] += p.Divers * p.DCS * dvR [ c ] * dXi_s;
        //        }
        //    } );

        //    Parallel.Invoke ( actions );

        //    return Vector.Add ( dvSumRcs, dvSumRcn );
        //}

        //private static Vector Equation96 ( )
        //{
        //    var actions = new Action [ 2 ];

        //    var dvSumRcs = Vector.Create ( NodeTissue.NumberOfTissues );
        //    actions [ 0 ] = ( ( ) =>
        //    {
        //        foreach ( int i in m_Data.FullDCSProfileIndicies )
        //        {
        //            ProfileCondition<NodeCondition> p = m_Data [ i ];

        //            var dvR = p.FinalNode.IntegratedRisk;
        //            var dXi = 0.0;
        //            for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //                dXi += m_dvGain [ c ] * dvR [ c ];

        //            dXi = Math.Exp ( dXi );

        //            for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //                dvSumRcs [ c ] -= p.Divers * dXi * Math.Pow ( dvR [ c ] / ( dXi - 1.0 ), 2.0 );
        //        }
        //    } );

        //    var dvSumRcn = Vector.Create ( NodeTissue.NumberOfTissues );
        //    actions [ 1 ] = ( ( ) =>
        //    {
        //        foreach ( int i in m_Data.MarginalDCSProfileIndicies )
        //        {
        //            ProfileCondition<NodeCondition> p = m_Data [ i ];

        //            var dvR = p.FinalNode.IntegratedRisk;
        //            var dXi = 0.0;
        //            for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //                dXi += m_dvGain [ c ] * dvR [ c ];

        //            dXi = Math.Exp ( dXi );

        //            for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //                dvSumRcn [ c ] -= p.Divers * p.DCS * dXi * Math.Pow ( dvR [ c ] / ( dXi - 1.0 ), 2.0 );
        //        }
        //    } );

        //    Parallel.Invoke ( actions );

        //    return Vector.Add ( dvSumRcs, dvSumRcn );
        //}

        //private static double [ , ] Equation97 ( )
        //{
        //    var actions = new Action [ 2 ];

        //    var dmSumRcs = new double [ NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues ];
        //    actions [ 0 ] = ( ( ) =>
        //    {
        //        foreach ( int i in m_Data.FullDCSProfileIndicies )
        //        {
        //            ProfileCondition<NodeCondition> p = m_Data [ i ];

        //            var dvR = p.FinalNode.IntegratedRisk;
        //            var dXi = 0.0;
        //            for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //                dXi += m_dvGain [ c ] * dvR [ c ];

        //            dXi = Math.Exp ( dXi );

        //            for ( int c1 = 0; c1 < NodeTissue.NumberOfTissues; c1++ )
        //                for ( int c2 = 0; c2 < NodeTissue.NumberOfTissues; c2++ )
        //                    dmSumRcs [ c1, c2 ] -= p.Divers * ( dvR [ c1 ] * dvR [ c2 ] * dXi ) / Math.Pow ( dXi - 1.0, 2.0 );
        //        }
        //    } );

        //    var dmSumRcn = new double [ NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues ];
        //    actions [ 1 ] = ( ( ) =>
        //    {
        //        foreach ( int i in m_Data.MarginalDCSProfileIndicies )
        //        {
        //            ProfileCondition<NodeCondition> p = m_Data [ i ];

        //            var dvR = p.FinalNode.IntegratedRisk;
        //            var dXi = 0.0;
        //            for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
        //                dXi += m_dvGain [ c ] * dvR [ c ];

        //            dXi = Math.Exp ( dXi );

        //            for ( int c1 = 0; c1 < NodeTissue.NumberOfTissues; c1++ )
        //                for ( int c2 = 0; c2 < NodeTissue.NumberOfTissues; c2++ )
        //                    dmSumRcn [ c1, c2 ] -= p.Divers * p.DCS * ( dvR [ c1 ] * dvR [ c2 ] * dXi ) / Math.Pow ( dXi - 1.0, 2.0 );
        //        }
        //    } );

        //    Parallel.Invoke ( actions );

        //    var dmSum = new double [ NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues ];
        //    for ( int c1 = 0; c1 < NodeTissue.NumberOfTissues; c1++ )
        //        for ( int c2 = 0; c2 < NodeTissue.NumberOfTissues; c2++ )
        //            dmSum [ c1, c2 ] = dmSumRcs [ c1, c2 ] + dmSumRcn [ c1, c2 ];

        //    return dmSum;
        //}

        #endregion trinomial, incidence only, fractional weight
#endif
        #region trinomial, time of onset, no fractional weight
        
        private static Vector Equations46_47RHS ( )
        {

            return Vector.Create ( NodeTissue.NumberOfTissues + 1 );
            
        }

        public static Vector Equations46_47RHS_MARGINAL()
        {

            return Vector.Create(NodeTissue.NumberOfTissues + 1);

        }

        private static Vector Equations46_47LHS ( )
        {

            #region equation 47 - gain parameters

            var dvSumEq47LHS_Serious = new double [ NodeTissue.NumberOfTissues ];

            foreach ( int i in m_Data.PSISeriousDCSProfileIndicies )
            {

                var p         = m_Data [ i ];
                var dvRcs01   = p.Time1Node.IntegratedRisk;
                var dvRcs02   = p.Time2Node.IntegratedRisk;
                var dHazard12 = new Double ( );

                for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
                    dHazard12 += m_dvGain [ c ] * ( dvRcs02 [ c ] - dvRcs01 [ c ] );

                var dXcs12 = Math.Exp ( m_dTrinomialScaleFactor * dHazard12 );

                // dXcs12 = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon , 1.0 / ( dXcs12 - 1.0 ) );

                dXcs12 = 1.0 / ( dXcs12 - 1.0 );

                for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
                    dvSumEq47LHS_Serious [ c ] += p.Divers * ( m_dTrinomialScaleFactor * ( dvRcs02 [ c ] - dvRcs01 [ c ] ) * dXcs12
                        - ( 1.0 + m_dTrinomialScaleFactor ) * dvRcs01 [ c ] );

            }

            var dvSumEq47LHS_Mild = new double [ NodeTissue.NumberOfTissues ];

            foreach ( int i in m_Data.PSIMildDCSProfileIndicies )
            {

                var p         = m_Data [ i ];
                var dvRcm01   = p.Time1Node.IntegratedRisk;
                var dvRcm02   = p.Time2Node.IntegratedRisk;
                var dHazard12 = new Double ( );

                for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
                    dHazard12 += m_dvGain [ c ] * ( dvRcm02 [ c ] - dvRcm01 [ c ] );

                var dXcm12 = Math.Exp ( dHazard12 );

                // dXcm12 = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon, 1.0 / ( dXcm12 - 1.0 ) );

                dXcm12 = 1.0 / ( dXcm12 - 1.0 );

                for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
                    dvSumEq47LHS_Mild [ c ] += p.Divers * ( ( dvRcm02 [ c ] - dvRcm01 [ c ] ) * dXcm12
                        - dvRcm01 [ c ] - m_dTrinomialScaleFactor * dvRcm02 [ c ] );

            }

            var dvSumEq47LHS_NoDCS = new double [ NodeTissue.NumberOfTissues ];

            foreach ( int i in m_Data.NoDCSProfileIndicies )
            {

                var p       = m_Data [ i ];
                var dvRcz03 = p.FinalNode.IntegratedRisk;

                for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
                    dvSumEq47LHS_NoDCS [ c ] += p.Divers * dvRcz03 [ c ];

            }

            for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                dvSumEq47LHS_NoDCS [ c ] *= - ( m_dTrinomialScaleFactor + 1.0 );

            #endregion

            #region equation 46 - scaling parameter

            var dSumEq46LHS_Serious = new Double ( );

            foreach ( int i in m_Data.PSISeriousDCSProfileIndicies )
            {

                var p         = m_Data [ i ];
                var dvRcs01   = p.Time1Node.IntegratedRisk;
                var dvRcs02   = p.Time2Node.IntegratedRisk;
                var dHazard01 = new Double ( );
                var dHazard12 = new Double ( );

                for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
                {
                    dHazard01 += m_dvGain [ c ] * dvRcs01 [ c ];
                    dHazard12 += m_dvGain [ c ] * ( dvRcs02 [ c ] - dvRcs01 [ c ] );
                }

                var dXcs12 = Math.Exp ( m_dTrinomialScaleFactor * dHazard12 );

                // dXcs12 = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon , 1.0 / ( dXcs12 - 1.0 ) );

                dXcs12 = 1.0 / ( dXcs12 - 1.0 );

                dSumEq46LHS_Serious += p.Divers * ( dHazard12 * dXcs12 - dHazard01 );

            }

            var dSumEq46LHS_Mild = new Double ( );

            foreach ( int i in m_Data.PSIMildDCSProfileIndicies )
            {

                var p         = m_Data [ i ];
                var dvRcm02   = p.Time2Node.IntegratedRisk;
                var dHazard02 = new Double ( );

                for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
                    dHazard02 += m_dvGain [ c ] * dvRcm02 [ c ];

                dSumEq46LHS_Mild -= p.Divers * dHazard02;

            }

            var dSumEq46LHS_NoDCS = new Double ( );

            foreach ( int i in m_Data.NoDCSProfileIndicies )
            {

                var p         = m_Data [ i ];
                var dvRcz03   = p.FinalNode.IntegratedRisk;
                var dHazard03 = new Double ( );

                for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
                    dHazard03 += m_dvGain [ c ] * dvRcz03 [ c ];

                dSumEq46LHS_NoDCS -= p.Divers * dHazard03;

            }

            #endregion

            #region calculate, load and return the Gradient vector

            var dvEquations46_47 = new double [ NodeTissue.NumberOfTissues + 1 ];

            for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
                dvEquations46_47 [ c ] = dvSumEq47LHS_Serious [ c ] + dvSumEq47LHS_Mild [ c ] + dvSumEq47LHS_NoDCS [ c ];   // gain components

            dvEquations46_47 [ dvEquations46_47.Length - 1 ] = dSumEq46LHS_Serious + dSumEq46LHS_Mild + dSumEq46LHS_NoDCS;  // trinomial scale component

            return Vector.Create ( dvEquations46_47 );

            #endregion

        }

        private static Vector Equations46_47LHS_MARGINAL()
        {

            #region equation 47 - gain parameters

            var dvSumEq47LHS_Full = Vector.Create ( NodeTissue.NumberOfTissues );

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p       = m_Data[i];
                var dvRcs01 = p.Time1Node.IntegratedRisk;
                var dvRcs02 = p.Time2Node.IntegratedRisk;
                var dXcs12  = new Double();

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXcs12 += m_dvGain[c] * (dvRcs02[c] - dvRcs01[c]);

                dXcs12 = Math.Min(1.0 / DCSUtilities.Constants.Epsilon, 1.0 / (Math.Exp(m_dTrinomialScaleFactor * dXcs12) - 1.0));

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumEq47LHS_Full[c] += p.Divers * (m_dTrinomialScaleFactor * (dvRcs02[c] - dvRcs01[c]) * dXcs12
                        - (1.0 + m_dTrinomialScaleFactor) * dvRcs01[c]);

            }

            var dvSumEq47LHS_Marginal = Vector.Create(NodeTissue.NumberOfTissues);

            foreach (int i in m_Data.MarginalDCSProfileIndicies)
            {

                var p       = m_Data[i];
                var dvRcm01 = p.Time1Node.IntegratedRisk;
                var dvRcm02 = p.Time2Node.IntegratedRisk;
                var dXcm12  = new Double();

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXcm12 += m_dvGain[c] * (dvRcm02[c] - dvRcm01[c]);

                dXcm12 = Math.Min(1.0 / DCSUtilities.Constants.Epsilon, 1.0 / (Math.Exp(dXcm12) - 1.0));

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumEq47LHS_Marginal[c] += p.Divers * ((dvRcm02[c] - dvRcm01[c]) * dXcm12
                        - dvRcm01[c] - m_dTrinomialScaleFactor * dvRcm02[c]);

            }

            var dvSumEq47LHS_NoDCS = Vector.Create(NodeTissue.NumberOfTissues);

            foreach (int i in m_Data.NoDCSProfileIndicies)
            {

                var p = m_Data[i];
                var dvRcz03 = p.FinalNode.IntegratedRisk;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumEq47LHS_NoDCS[c] += p.Divers * dvRcz03[c];

            }

            for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                dvSumEq47LHS_NoDCS[c] *= -(m_dTrinomialScaleFactor + 1.0);

            #endregion

            #region equation 46 - scaling parameter

            var dSumEq46LHS_Full = new Double();

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p         = m_Data[i];
                var dvRcs01   = p.Time1Node.IntegratedRisk;
                var dvRcs02   = p.Time2Node.IntegratedRisk;
                var dXcs12    = new Double();
                var dHazard01 = new Double();
                var dHazard12 = new Double();

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                {
                    dHazard01 += m_dvGain[c] * dvRcs01[c];
                    dHazard12 += m_dvGain[c] * (dvRcs02[c] - dvRcs01[c]);
                }

                dXcs12 = Math.Min(1.0 / DCSUtilities.Constants.Epsilon, 1.0 / (Math.Exp(m_dTrinomialScaleFactor * dHazard12) - 1.0));

                dSumEq46LHS_Full += p.Divers * (dHazard12 * dXcs12 - dHazard01);

            }

            var dSumEq46LHS_Marginal = new Double();

            foreach (int i in m_Data.MarginalDCSProfileIndicies)
            {

                var p         = m_Data[i];
                var dvRcm02   = p.Time2Node.IntegratedRisk;
                var dHazard02 = new Double();

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dHazard02 += m_dvGain[c] * dvRcm02[c];

                dSumEq46LHS_Marginal -= p.Divers * dHazard02;

            }

            var dSumEq46LHS_NoDCS = new Double();

            foreach (int i in m_Data.NoDCSProfileIndicies)
            {

                var p = m_Data[i];
                var dvRcz03 = p.FinalNode.IntegratedRisk;
                var dHazard03 = new Double();

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dHazard03 += m_dvGain[c] * dvRcz03[c];

                dSumEq46LHS_NoDCS -= p.Divers * dHazard03;

            }

            #endregion

            #region calculate, load and return the Gradient vector

            var dvEquations46_47 = Vector.Create(NodeTissue.NumberOfTissues + 1);

            for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                dvEquations46_47[c] = dvSumEq47LHS_Full[c] + dvSumEq47LHS_Marginal[c] + dvSumEq47LHS_NoDCS[c];

            dvEquations46_47[dvEquations46_47.Length - 1] = dSumEq46LHS_Full + dSumEq46LHS_Marginal + dSumEq46LHS_NoDCS;

            return dvEquations46_47;

            #endregion

        }

        public static double [ , ] Equations49_53 ( )
        {

            #region equation 49

            var dSumEq49_Serious = new Double ( );

            foreach ( int i in m_Data.PSISeriousDCSProfileIndicies )
            {

                var p         = m_Data [ i ];
                var dvRcs01   = p.Time1Node.IntegratedRisk;
                var dvRcs02   = p.Time2Node.IntegratedRisk;
                var dHazard12 = new Double ( );

                for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                    dHazard12 += m_dvGain [ c ] * ( dvRcs02 [ c ] - dvRcs01 [ c ] );

                var dXcs12 = Math.Exp ( m_dTrinomialScaleFactor * dHazard12 );

                var denom = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon , 1.0 / Math.Pow ( ( dXcs12 - 1.0 ) , 2.0 ) ); // modified 03/24/2013

                dSumEq49_Serious -= p.Divers * Math.Pow ( dHazard12 , 2.0 ) * dXcs12 * denom;

            }

            #endregion

            #region equation 50

            var dvSumEq50_Serious = new double [ NodeTissue.NumberOfTissues ];

            foreach ( int i in m_Data.PSISeriousDCSProfileIndicies )
            {

                var p         = m_Data [ i ];
                var dvRcs01   = p.Time1Node.IntegratedRisk;
                var dvRcs02   = p.Time2Node.IntegratedRisk;
                var dHazard12 = new Double ( );

                for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                    dHazard12 += m_dvGain [ c ] * ( dvRcs02 [ c ] - dvRcs01 [ c ] );

                var dXcs12 = Math.Exp ( m_dTrinomialScaleFactor * dHazard12 ); // modified 02/08/2018

                var denom = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon , 1.0 / Math.Pow ( ( dXcs12 - 1.0 ) , 2.0 ) ); // modified 03/24/2013

                for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                    dvSumEq50_Serious [ c ] -= p.Divers * Math.Pow ( m_dTrinomialScaleFactor , 2.0 ) * Math.Pow ( ( dvRcs02 [ c ] - dvRcs01 [ c ] ) , 2.0 ) * dXcs12 * denom;

            }

            var dvSumEq50_Mild = new double [ NodeTissue.NumberOfTissues ];

            foreach ( int i in m_Data.PSIMildDCSProfileIndicies )
            {

                var p         = m_Data [ i ];
                var dvRcm01   = p.Time1Node.IntegratedRisk;
                var dvRcm02   = p.Time2Node.IntegratedRisk;
                var dHazard12 = new Double ( );

                for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                    dHazard12 += m_dvGain [ c ] * ( dvRcm02 [ c ] - dvRcm01 [ c ] );

                var dXcm12 = Math.Exp ( dHazard12 );  // modified 02/08/2018

                var denom = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon , 1.0 / Math.Pow ( ( dXcm12 - 1.0 ) , 2.0 ) );  // modified 02/24/2013

                for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                    dvSumEq50_Mild [ c ] -= p.Divers * Math.Pow ( ( dvRcm02 [ c ] - dvRcm01 [ c ] ) , 2.0 ) * dXcm12 * denom;

            }

            #endregion           

            #region equation 51

            var dvSumEq51_Serious = new double [ NodeTissue.NumberOfTissues ];

            foreach ( int i in m_Data.PSISeriousDCSProfileIndicies )
            {

                var p         = m_Data [ i ];
                var dvRcs01   = p.Time1Node.IntegratedRisk;
                var dvRcs02   = p.Time2Node.IntegratedRisk;
                var dHazard12 = new Double ( );

                for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                    dHazard12 += m_dvGain [ c ] * ( dvRcs02 [ c ] - dvRcs01 [ c ] );

                var dXcs12 = Math.Exp ( m_dTrinomialScaleFactor * dHazard12 );

                var denom = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon , 1.0 / Math.Pow ( ( dXcs12 - 1.0 ) , 2.0 ) ); // modified 03/24/2013

                for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                    dvSumEq51_Serious [ c ] += p.Divers * ( ( dvRcs02 [ c ] - dvRcs01 [ c ] ) * ( dXcs12 * ( 1.0 + Math.Log ( dXcs12 ) ) - 1.0 ) * denom - dvRcs01 [ c ] );

            }

            var dvSumEq51_Mild = new double [ NodeTissue.NumberOfTissues ];

            foreach ( int i in m_Data.PSIMildDCSProfileIndicies )
            {

                var p       = m_Data [ i ];
                var dvRcm02 = p.Time2Node.IntegratedRisk;

                for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                    dvSumEq51_Mild [ c ] -= p.Divers * dvRcm02 [ c ];

            }

            var dvSumEq51_NoDCS = new double [ NodeTissue.NumberOfTissues ];

            foreach ( int i in m_Data.NoDCSProfileIndicies )
            {

                var p       = m_Data [ i ];
                var dvRcz03 = p.FinalNode.IntegratedRisk;

                for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                    dvSumEq51_NoDCS [ c ] -= p.Divers * dvRcz03 [ c ];

            }

            #endregion

            #region equation 52

            var dmSumEq52_Serious = new double [ NodeTissue.NumberOfTissues , NodeTissue.NumberOfTissues ];

            foreach ( int i in m_Data.PSISeriousDCSProfileIndicies )
            {

                var p = m_Data [ i ];
                var dvRcs01 = p.Time1Node.IntegratedRisk;
                var dvRcs02 = p.Time2Node.IntegratedRisk;
                var dHazard12 = new Double ( );

                for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                    dHazard12 += m_dvGain [ c ] * ( dvRcs02 [ c ] - dvRcs01 [ c ] );

                var dXcs12 = Math.Exp ( m_dTrinomialScaleFactor * dHazard12 );

                var denom = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon , 1.0 / Math.Pow ( ( dXcs12 - 1.0 ) , 2.0 ) ); // modified 03/24/2013

                for ( int ci = 0 ; ci < NodeTissue.NumberOfTissues ; ci++ )
                    for ( int cj = ci ; cj < NodeTissue.NumberOfTissues ; cj++ )
                        dmSumEq52_Serious [ ci , cj ] -= p.Divers * Math.Pow ( m_dTrinomialScaleFactor , 2.0 ) * ( dvRcs02 [ ci ] - dvRcs01 [ ci ] ) * ( dvRcs02 [ cj ] - dvRcs01 [ cj ] ) * dXcs12 * denom;

            }

            var dmSumEq52_Mild = new double [ NodeTissue.NumberOfTissues , NodeTissue.NumberOfTissues ];

            foreach ( int i in m_Data.PSIMildDCSProfileIndicies )
            {

                var p         = m_Data [ i ];
                var dvRcm01   = p.Time1Node.IntegratedRisk;
                var dvRcm02   = p.Time2Node.IntegratedRisk;
                var dHazard12 = new Double ( );

                for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                    dHazard12 += m_dvGain [ c ] * ( dvRcm02 [ c ] - dvRcm01 [ c ] );

                var dXcm12 = Math.Exp ( dHazard12 ); // modified 02/08/2018

                var denom = Math.Min ( 1.0 / DCSUtilities.Constants.Epsilon , 1.0 / Math.Pow ( ( dXcm12 - 1.0 ) , 2.0 ) ); // modified 03/24/2013

                for ( int ci = 0 ; ci < NodeTissue.NumberOfTissues ; ci++ )
                    for ( int cj = ci ; cj < NodeTissue.NumberOfTissues ; cj++ )
                        dmSumEq52_Mild [ ci , cj ] -= p.Divers * ( dvRcm02 [ ci ] - dvRcm01 [ ci ] ) * ( dvRcm02 [ cj ] - dvRcm01 [ cj ] ) * dXcm12 * denom;

            }

            #endregion

            #region calculate, load, and return the Hessian matrix

            var dmHessian = new double [ NodeTissue.NumberOfTissues + 1 , NodeTissue.NumberOfTissues + 1 ];

            // first diagonal terms
            for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                dmHessian [ c , c ] = dvSumEq50_Serious [ c ] + dvSumEq50_Mild [ c ];

            // last row/column
            for ( int c = 0 ; c < NodeTissue.NumberOfTissues ; c++ )
                dmHessian [ NodeTissue.NumberOfTissues , c ] = dmHessian [ c , NodeTissue.NumberOfTissues ] = dvSumEq51_Serious [ c ] + dvSumEq51_Mild [ c ] + dvSumEq51_NoDCS [ c ];

            // triangular mixed gain terms
            for ( int ci = 0 ; ci < NodeTissue.NumberOfTissues ; ci++ )
                for ( int cj = ci + 1 ; cj < NodeTissue.NumberOfTissues ; cj++ )
                    dmHessian [ ci , cj ] = dmHessian [ cj , ci ] = dmSumEq52_Serious [ ci , cj ] + dmSumEq52_Mild [ ci , cj ];

            // last diagonal term
            dmHessian [ NodeTissue.NumberOfTissues , NodeTissue.NumberOfTissues ] = dSumEq49_Serious;

            return dmHessian;

            #endregion

        }

        public static double[,] Equations49_53_MARGINAL()
        {

            #region equation 49

            var dSumEq49_Full = new Double();

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p = m_Data[i];
                var dvRcs01 = p.Time1Node.IntegratedRisk;
                var dvRcs02 = p.Time2Node.IntegratedRisk;
                var dHazard12 = new Double();
                var dXcs12 = new Double();

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dHazard12 += m_dvGain[c] * (dvRcs02[c] - dvRcs01[c]);

                dXcs12 = Math.Exp(m_dTrinomialScaleFactor * dHazard12);

                var denom = Math.Min(1.0 / DCSUtilities.Constants.Epsilon, 1.0 / Math.Pow((dXcs12 - 1.0), 2.0)); // modified 03/24/2013

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dSumEq49_Full -= p.Divers * Math.Pow(dHazard12, 2.0) * dXcs12 * denom;

            }

            #endregion

            #region equation 50

            var dvSumEq50_Full = Vector.Create(NodeTissue.NumberOfTissues);

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p = m_Data[i];
                var dvRcs01 = p.Time1Node.IntegratedRisk;
                var dvRcs02 = p.Time2Node.IntegratedRisk;
                var dXcs12 = new Double();

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXcs12 += m_dvGain[c] * (dvRcs02[c] - dvRcs01[c]);

                var denom = Math.Min(1.0 / DCSUtilities.Constants.Epsilon, 1.0 / Math.Pow((Math.Exp(m_dTrinomialScaleFactor * dXcs12) - 1.0), 2.0)); // modified 03/24/2013

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumEq50_Full[c] -= p.Divers * Math.Pow(m_dTrinomialScaleFactor, 2.0) * Math.Pow((dvRcs02[c] - dvRcs01[c]), 2.0) * dXcs12 * denom;

            }

            var dvSumEq50_Marginal = Vector.Create(NodeTissue.NumberOfTissues);

            foreach (int i in m_Data.MarginalDCSProfileIndicies)
            {

                var p = m_Data[i];
                var dvRcm01 = p.Time1Node.IntegratedRisk;
                var dvRcm02 = p.Time2Node.IntegratedRisk;
                var dXcm12 = new Double();

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXcm12 += m_dvGain[c] * (dvRcm02[c] - dvRcm01[c]);

                var denom = Math.Min(1.0 / DCSUtilities.Constants.Epsilon, 1.0 / Math.Pow((Math.Exp(dXcm12) - 1.0), 2.0)); // modified 02/24/2013

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumEq50_Marginal[c] -= p.Divers * Math.Pow((dvRcm02[c] - dvRcm01[c]), 2.0) * dXcm12 * denom;

            }

            #endregion           

            #region equation 51

            var dvSumEq51_Full = Vector.Create(NodeTissue.NumberOfTissues);

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p = m_Data[i];
                var dvRcs01 = p.Time1Node.IntegratedRisk;
                var dvRcs02 = p.Time2Node.IntegratedRisk;
                var dXcs12 = new Double();

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXcs12 += m_dvGain[c] * (dvRcs02[c] - dvRcs01[c]);

                dXcs12 = Math.Exp(m_dTrinomialScaleFactor * dXcs12);

                var denom = Math.Min(1.0 / DCSUtilities.Constants.Epsilon, 1.0 / Math.Pow((dXcs12 - 1.0), 2.0)); // modified 03/24/2013

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumEq51_Full[c] += p.Divers * ((dvRcs02[c] - dvRcs01[c]) * (dXcs12 * (1.0 + Math.Log(dXcs12)) - 1.0) * denom - dvRcs01[c]);

            }

            var dvSumEq51_Marginal = Vector.Create(NodeTissue.NumberOfTissues);

            foreach (int i in m_Data.MarginalDCSProfileIndicies)
            {

                var p = m_Data[i];
                var dvRcm02 = p.Time2Node.IntegratedRisk;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumEq51_Marginal[c] -= p.Divers * dvRcm02[c];

            }

            var dvSumEq51_NoDCS = Vector.Create(NodeTissue.NumberOfTissues);

            foreach (int i in m_Data.NoDCSProfileIndicies)
            {

                var p = m_Data[i];
                var dvRcz03 = p.FinalNode.IntegratedRisk;

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dvSumEq51_NoDCS[c] -= p.Divers * dvRcz03[c];

            }

            #endregion

            #region equation 52

            var dmSumEq52_Full = new double[NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues];

            foreach (int i in m_Data.FullDCSProfileIndicies)
            {

                var p = m_Data[i];
                var dvRcs01 = p.Time1Node.IntegratedRisk;
                var dvRcs02 = p.Time2Node.IntegratedRisk;
                var dXcs12 = new Double();

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXcs12 += m_dvGain[c] * (dvRcs02[c] - dvRcs01[c]);

                dXcs12 = Math.Exp(m_dTrinomialScaleFactor * dXcs12);

                var denom = Math.Min(1.0 / DCSUtilities.Constants.Epsilon, 1.0 / Math.Pow((dXcs12 - 1.0), 2.0)); // modified 03/24/2013

                for (int ci = 0; ci < NodeTissue.NumberOfTissues; ci++)
                    for (int cj = ci; cj < NodeTissue.NumberOfTissues; cj++)
                        dmSumEq52_Full[ci, cj] -= p.Divers * Math.Pow(m_dTrinomialScaleFactor, 2.0) * (dvRcs02[ci] - dvRcs01[ci]) * (dvRcs02[cj] - dvRcs01[cj]) * dXcs12 * denom;

            }

            var dmSumEq52_Marginal = new double[NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues];

            foreach (int i in m_Data.MarginalDCSProfileIndicies)
            {

                var p = m_Data[i];
                var dvRcm01 = p.Time1Node.IntegratedRisk;
                var dvRcm02 = p.Time2Node.IntegratedRisk;
                var dXcm12 = new Double();

                for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                    dXcm12 += m_dvGain[c] * (dvRcm02[c] - dvRcm01[c]);

                dXcm12 = Math.Exp(dXcm12);

                var denom = Math.Min(1.0 / DCSUtilities.Constants.Epsilon, 1.0 / Math.Pow((dXcm12 - 1.0), 2.0)); // modified 03/24/2013

                for (int ci = 0; ci < NodeTissue.NumberOfTissues; ci++)
                    for (int cj = ci; cj < NodeTissue.NumberOfTissues; cj++)
                        dmSumEq52_Marginal[ci, cj] -= p.Divers * (dvRcm02[ci] - dvRcm01[ci]) * (dvRcm02[cj] - dvRcm01[cj]) * dXcm12 * denom;

            }

            #endregion

            #region calculate, load, and return the Hessian matrix

            var dmHessian = new double[NodeTissue.NumberOfTissues + 1, NodeTissue.NumberOfTissues + 1];

            // first diagonal terms
            for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                dmHessian[c, c] = dvSumEq50_Full[c] + dvSumEq50_Marginal[c];

            // last row/column
            for (int c = 0; c < NodeTissue.NumberOfTissues; c++)
                dmHessian[NodeTissue.NumberOfTissues, c] = dmHessian[c, NodeTissue.NumberOfTissues] = dvSumEq51_Full[c] + dvSumEq51_Marginal[c] + dvSumEq51_NoDCS[c];

            // triangular mixed gain terms
            for (int ci = 0; ci < NodeTissue.NumberOfTissues; ci++)
                for (int cj = ci + 1; cj < NodeTissue.NumberOfTissues; cj++)
                    dmHessian[ci, cj] = dmHessian[cj, ci] = dmSumEq52_Full[ci, cj] + dmSumEq52_Marginal[ci, cj];

            // last diagonal term
            dmHessian[NodeTissue.NumberOfTissues, NodeTissue.NumberOfTissues] = dSumEq49_Full;

            return dmHessian;

            #endregion

        }
        
        #endregion trinomial, time of onset, no fractional weight

        /// <summary>
        /// Calculate P0, PM, and PS for all profiles in the data collection. 
        /// If m_bUseFailuretimes == true, the T1 and T2 windowed probabilities are calculated.
        /// This method is not used internally to the static Decompression.ExactGainTrinomial class.
        /// </summary>
        private static void CalculateProbabilities ( )
        {

            if ( !Profile<Node>.UsePSI )
                throw new DCSException ( "Profile<Node>.UsePSI flag is not set in Decompression.ExactGainTrinomial.CalculateProbabilities" );

            m_dvP0 = new double [ m_Data.Profiles ];
            m_dvPM = new double [ m_Data.Profiles ];
            m_dvPS = new double [ m_Data.Profiles ];

            for ( int i = 0; i < m_Data.Profiles; i++ )
            {

                var p = m_Data [ i ];

                var dRiskInt = p.FinalNode.IntegratedRisk;
                var dR = 0.0;

                for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
                    dR += m_dvGain [ c ] * dRiskInt [ c ];

                var dPCS = 1.0 - Math.Exp ( -m_dTrinomialScaleFactor * dR );
                var dPCM = 1.0 - Math.Exp ( -dR );

                var dPHS = dPCS;
                var dPHM = dPCM * ( 1.0 - dPCS );
                var dPH0 = 1.0 - dPHS - dPHM;


                m_dvPS [ i ] = dPHS; // a serious DCS event
                m_dvPM [ i ] = dPHM; // a mild DCS event
                m_dvP0 [ i ] = dPH0; // no DCS event

                #region time of failure probability

                if ( m_bUseFailureTimes )
                {

                    if ( p.Severity == PSISEVERITY.MILD || p.Severity == PSISEVERITY.SERIOUS )
                    {

                        var dR1 = new double ( );
                        var dR2 = new double ( );
                        var dRT1 = new double [ NodeTissue.NumberOfTissues ];
                        var dRT2 = new double [ NodeTissue.NumberOfTissues ];

                        dRT1 = p.Time1Node.IntegratedRisk;
                        dRT2 = p.Time2Node.IntegratedRisk;

                        for ( int c = 0; c < NodeTissue.NumberOfTissues; c++ )
                        {
                            dR1 += m_dvGain [ c ] * dRT1 [ c ];
                            dR2 += m_dvGain [ c ] * dRT2 [ c ];
                        }

                        var dPCS1 = 1.0 - Math.Exp ( -m_dTrinomialScaleFactor * dR1 );
                        var dPCM1 = 1.0 - Math.Exp ( -dR1 );

                        var dPHS1 = dPCS1;
                        var dPHM1 = dPCM1 * ( 1.0 - dPCS1 );
                        var dPH01 = 1.0 - dPHS1 - dPHM1;

                        var dPCS2 = 1.0 - Math.Exp ( -m_dTrinomialScaleFactor * dR2 );
                        var dPCM2 = 1.0 - Math.Exp ( -dR2 );

                        var dPHS2 = dPCS2;
                        var dPHM2 = dPCM2 * ( 1.0 - dPCS2 );

                        if ( p.Severity == PSISEVERITY.SERIOUS )
                            m_dvPS [ i ] = dPH01 * ( dPHS2 - dPHS1 );

                        if ( p.Severity == PSISEVERITY.MILD )
                            m_dvPM [ i ] = dPH01 * ( dPHM2 - dPHM1 );

                        m_dvP0 [ i ] = 1.0 - m_dvPS [ i ] - m_dvPM [ i ];

                    } // if Severity

                } // if bUseFailureTime

                #endregion time of failure probability

            } // for i

            return;

        }

        public static double [ ] Optimize ( )
        {

            // calculate the competitive and hierarchical event probabilities
            //CalculateProbabilities ( );

            // assign dummy Funcs - these are reset in the switch statement
            Func<Vector> sumRHS         = ( ) => new double [ NodeTissue.NumberOfTissues + 1 ];
            Func<Vector> sumLHS         = ( ) => new double [ NodeTissue.NumberOfTissues + 1 ];
            Func<double [ , ]> Jacobian = ( ) => new double [ NodeTissue.NumberOfTissues + 1, NodeTissue.NumberOfTissues + 1 ];

            switch ( OptimizationType )
            {

                case OPTIMIZATIONTYPE.INCIDENCONLYNOMARGINAL:
                    throw new NotImplementedException ( "Decompression.ExactGainTrinomial.Optimize ( ) case OPTIMIZATIONTYPE.INCIDENCONLYNOMARGINAL: not yet implemented" );
                    // break;

                case OPTIMIZATIONTYPE.TIMEOFONSETNOMARGINAL:
                    sumRHS   = ( ) => Equations46_47RHS ( );
                    sumLHS   = ( ) => Equations46_47LHS ( );
                    Jacobian = ( ) => Equations49_53 ( );
                    break;

                case OPTIMIZATIONTYPE.TIMEOFONSETTRINOMIALMARGINAL:
                    sumRHS   = ( ) => Equations46_47RHS_MARGINAL ( );
                    sumLHS   = ( ) => Equations46_47LHS_MARGINAL ( );
                    Jacobian = ( ) => Equations49_53_MARGINAL ( );
                    break;

                case OPTIMIZATIONTYPE.INCIDENCEONLYFRACTIONALMARGINAL:
                    throw new NotImplementedException ( "Decompression.ExactGainTrinomial.Optimize ( ) case OPTIMIZATIONTYPE.INCIDENCEONLYFRACTIONALMARGINAL: not yet implemented" );
                    // break;

                case OPTIMIZATIONTYPE.TIMEOFONSETFRACTIONALMARGINAL:
                    throw new NotImplementedException ( "Decompression.ExactGainTrinomial.Optimize ( ) case OPTIMIZATIONTYPE.TIMEOFONSETFRACTIONALMARGINAL: not yet implemented" );
                    // break;

                default:
                    throw new NotImplementedException ( "Bad OPTIMIZATIONTYPE enumeration value in Decompression.ExactGainTrinomial.Optimize ( )" );
                    //break;

            }

            #region assign lambda functions
            // non-persistent counter included in the lambda closure
            int cVal = new int ( );

            // calculate the right hand side once
            Vector dvSumRHS = sumRHS ( );
            Vector dvSumLHS = new double [ NodeTissue.NumberOfTissues +1 ];

            // the multi-dimensional nonlinear system
            Func<Vector, double> [ ] f = new Func<Vector, double> [ NodeTissue.NumberOfTissues + 1 ];
            for ( int c = 0; c < NodeTissue.NumberOfTissues + 1; c++ )
                if ( c == 0 )
                    f [ c ] = ( x ) =>
                    {
                        cVal = 0;
                        Array.Copy ( x.ToArray ( ), m_dvGain, NodeTissue.NumberOfTissues );
                        m_dTrinomialScaleFactor = x [ x.Length - 1 ];
                        dvSumLHS = sumLHS ( );
                        return dvSumLHS [ cVal ] - dvSumRHS [ cVal ];
                    };
                else
                    f [ c ] = ( x ) =>
                    {
                        cVal++;
                        return dvSumLHS [ cVal ] - dvSumRHS [ cVal ];
                    };

            // the Jacobian of the non-linear system
            double [ , ] dmJac = new double [ NodeTissue.NumberOfTissues + 1, NodeTissue.NumberOfTissues +1 ];
            Func<Vector, Vector, Vector> [ ] df = new Func<Vector, Vector, Vector> [ NodeTissue.NumberOfTissues + 1 ];
            for ( int c = 0; c < NodeTissue.NumberOfTissues + 1; c++ )
                if ( c == 0 )
                    df [ c ] = ( x, y ) =>
                    {
                        cVal = 0;
                        //m_dvGain = x.ToArray ( );
                        Array.Copy ( x.ToArray ( ), m_dvGain, NodeTissue.NumberOfTissues );
                        m_dTrinomialScaleFactor = x [ x.Length - 1 ];
                        dmJac = Jacobian ( );
                        for ( int d = 0; d < NodeTissue.NumberOfTissues + 1; d++ )
                            y [ d ] = dmJac [ cVal, d ];
                        return y;
                    };
                else
                    df [ c ] = ( x, y ) =>
                    {
                        cVal++;
                        for ( int d = 0; d < NodeTissue.NumberOfTissues + 1; d++ )
                            y [ d ] = dmJac [ cVal, d ];
                        return y;
                    };

            #endregion assign lambda functions

            Vector initialGuess                      = Vector.Create ( NodeTissue.NumberOfTissues + 1 );
            initialGuess [ initialGuess.Length - 1 ] = m_dTrinomialScaleFactor;
            for ( int i                              = 0; i < NodeTissue.NumberOfTissues; i++ )
                initialGuess [ i ]                   = m_dvGain [ i ];
            //var solver                             = new NewtonRaphsonSystemSolver ( f, initialGuess );
            var solver                               = new DoglegSystemSolver ( f, df, initialGuess );
            solver.TrustRegionRadius                 = 1.0e-3;
            solver.MaxIterations                     = 1024;
            solver.ValueTest.Tolerance               = 1e-6;
            solver.ValueTest.Norm                    = Extreme.Mathematics.Algorithms.VectorConvergenceNorm.Maximum;
            solver.SolutionTest.Tolerance            = 1e-6;
            solver.SolutionTest.ConvergenceCriterion = ConvergenceCriterion.WithinRelativeTolerance;

            int n = 0;
            do
            {
                if ( n++ > m_iNumberOfRestarts )
                    throw new DCSException ( "Too many restarts required in Decompression.ExactGainTrinomial.OptimizeGain" );

                solver.InitialGuess = initialGuess;
                try
                {

                    Vector solution = solver.Solve ( );
                    var error       = solver.EstimatedError;
                    var evals       = solver.EvaluationsNeeded;
                    var status      = solver.Status;
                    var report      = solver.SolutionReport;
                    var vec         = solution.ToArray ( );
                    
                }
                catch ( Exception e )
                {
                    throw new DCSException ( "Exception thrown in Decompression.ExactGainTriomial.Optimize" );
                }

                initialGuess = RandomInitialGainVector ( ); // corrected 20130325

            } while ( solver.Status != AlgorithmStatus.Converged );
            
            return m_dvGain;

        }

#if false

        public void OptimalGainTrinomialTimeOfOnsetHazard(double[,] dvR01, double[,] dvR12, double[,] dvR03, MODEL m)
        { // verified 20090220
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
        } // EOM

        public double OptimalGainTrinomialTimeOfOnsetLogLikelihood(MODEL m)
        { // verified 20090220
            // see equation 45 in Howle's decompression sickness 1 lab notebook

            double[,] dvR01 = new double[m_dvGain.Length, this.Profiles];
            double[,] dvR12 = new double[m_dvGain.Length, this.Profiles];
            double[,] dvR03 = new double[m_dvGain.Length, this.Profiles];
            double dLL = new double();

            OptimalGainTrinomialTimeOfOnsetHazard(dvR01, dvR12, dvR03, m);

            for (int i = 0; i < this.Profiles; i++)
            {
                EE1ntDiveProfile p = this.Entry(i);

                double dTerm01 = new double();
                double dTerm02 = new double();
                double dTerm12 = new double();
                double dTerm03 = new double();
                for (int n = 0; n < m_dvGain.Length; n++)
                {
                    dTerm01 -= m_dvGain[n] * dvR01[n, i];
                    dTerm12 -= m_dvGain[n] * dvR12[n, i];
                    dTerm02 -= m_dvGain[n] * (dvR01[n, i] + dvR12[n, i]);
                    dTerm03 -= m_dvGain[n] * dvR03[n, i];
                }

                if (p.PSISeriousDCS)
                    dLL += (double)p.Divers * Math.Log(Math.Exp((this.ACoefficient + 1.0) * dTerm01) - Math.Exp(dTerm01 + this.ACoefficient * dTerm02));
                else if (p.PSIMildDCS)
                    dLL += (double)p.Divers * Math.Log(Math.Exp(dTerm01 + this.ACoefficient * dTerm02) - Math.Exp((this.ACoefficient + 1.0) * dTerm02));
                else
                    dLL += (double)p.Divers * (this.ACoefficient + 1.0) * dTerm03;  // Note: Term03 is negative, therefore this term is additive
            } // for i

            return dLL;
        }

        public double OptimalGainTrinomialTimeOfOnsetASlope(double[,] dvR01, double[,] dvR12, double[,] dvR03)
        { // verified 20090220
            // see equation 46 in Howle's decompression sickness 1 lab book

            double dSlope = new double();

            for (int i = 0; i < this.Profiles; i++)
            {
                if (this.Entry(i).PSISeriousDCS)
                {
                    double dTerm01 = new double();
                    double dTerm12 = new double();
                    for (int n = 0; n < m_dvGain.Length; n++)
                    {
                        dTerm01 += m_dvGain[n] * dvR01[n, i];
                        dTerm12 += m_dvGain[n] * dvR12[n, i];
                    }

                    dSlope += (double)this.Entry(i).Divers * (dTerm12 / (Math.Exp(this.ACoefficient * dTerm12) - 1.0) - dTerm01);
                } // if
                else if (this.Entry(i).PSIMildDCS)
                {
                    double dTerm02 = new double();
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dTerm02 += m_dvGain[n] * (dvR01[n, i] + dvR12[n, i]);

                    dSlope -= (double)this.Entry(i).Divers * dTerm02;
                }
                else
                {
                    double dTerm03 = new double();
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dTerm03 += m_dvGain[n] * dvR03[n, i];

                    dSlope -= (double)this.Entry(i).Divers * dTerm03;
                }
            } // for i

            return dSlope;
        } // EOM

        public double[] OptimalGainTrinomialTimeOfOnsetGainSlope(double[,] dvR01, double[,] dvR12, double[,] dvR03)
        { // verified 20090220
            // see equation 47 in Howle's decompression sickness 1 lab book

            double[] dvSlope = new double[m_dvGain.Length];

            for (int i = 0; i < this.Profiles; i++)
            {
                if (this.Entry(i).PSISeriousDCS)
                { // working
                    double dTerm12 = new double();

                    for (int n = 0; n < m_dvGain.Length; n++)
                        dTerm12 += m_dvGain[n] * dvR12[n, i];

                    for (int n = 0; n < m_dvGain.Length; n++)
                        dvSlope[n] += (double)this.Entry(i).Divers * (
                                this.ACoefficient * dvR12[n, i] / (Math.Exp(this.ACoefficient * dTerm12) - 1.0)
                                - (this.ACoefficient + 1.0) * dvR01[n, i]);
                }
                else if (this.Entry(i).PSIMildDCS)
                { // working
                    double dTerm12 = new double();

                    for (int n = 0; n < m_dvGain.Length; n++)
                        dTerm12 += m_dvGain[n] * dvR12[n, i];

                    for (int n = 0; n < m_dvGain.Length; n++)
                        dvSlope[n] += (double)this.Entry(i).Divers * (
                                dvR12[n, i] / (Math.Exp(dTerm12) - 1.0)
                                - dvR01[n, i] - this.ACoefficient * (dvR01[n, i] + dvR12[n, i]));
                }
                else
                { // working
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dvSlope[n] -= (double)this.Entry(i).Divers * (this.ACoefficient + 1.0) * dvR03[n, i];
                }
            } // for i

            return dvSlope;
        } // EOM

        public double OptimalGainTrinomialTimeOfOnsetACurvature(double[,] dvR12)
        { // verified 20090220
            // see equation 49 in Howle's decompression sickness 1 lab notebook

            double dCurvature = new double();

            for (int i = 0; i < this.Profiles; i++)
            {
                if (this.Entry(i).PSISeriousDCS)
                {
                    double dTerm12 = new double();
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dTerm12 += m_dvGain[n] * dvR12[n, i];

                    dCurvature -= (double)this.Entry(i).Divers * Math.Pow(dTerm12, 2.0)
                        * Math.Exp(this.ACoefficient * dTerm12) / Math.Pow(Math.Exp(this.ACoefficient * dTerm12) - 1.0, 2.0);
                }
            }

            return dCurvature;
        } // EOM

        public double[] OptimalGainTrinomialTimeOfOnsetGainCurvature(double[,] dvR12)
        { // verified 20090220
            // see equation 50 in Howle's decompression sickness 1 lab notebook

            double[] dvCurvature = new double[m_dvGain.Length];

            for (int i = 0; i < this.Profiles; i++)
            {
                if (this.Entry(i).PSISeriousDCS)
                {
                    double dTerm12 = new double();
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dTerm12 += m_dvGain[n] * dvR12[n, i];

                    dTerm12 = Math.Exp(this.ACoefficient * dTerm12);

                    for (int n = 0; n < m_dvGain.Length; n++)
                        dvCurvature[n] -= (double)this.Entry(i).Divers * Math.Pow(this.ACoefficient * dvR12[n, i], 2.0) * dTerm12 / Math.Pow(dTerm12 - 1.0, 2.0);
                }
                else if (this.Entry(i).PSIMildDCS)
                {
                    double dTerm12 = new double();
                    for (int n = 0; n < m_dvGain.Length; n++)
                        dTerm12 += m_dvGain[n] * dvR12[n, i];

                    dTerm12 = Math.Exp(dTerm12);

                    for (int n = 0; n < m_dvGain.Length; n++)
                        dvCurvature[n] -= (double)this.Entry(i).Divers * Math.Pow(dvR12[n, i], 2.0) * dTerm12 / Math.Pow(dTerm12 - 1.0, 2.0);
                }
            }

            return dvCurvature;
        } // EOM

        public double OptimalGainTrinomialTimeOfOnsetAGainCrossDerivative(int n, double[,] dvR01, double[,] dvR12, double[,] dvR03)
        { // verified 20090221
            // see equation 51 in Howle's decompression sickness 1 lab notebook

            double dHess = new double();

            for (int i = 0; i < this.Profiles; i++)
            {
                if (this.Entry(i).PSISeriousDCS)
                {
                    double dTerm12 = new double();
                    for (int j = 0; j < m_dvGain.Length; j++)
                        dTerm12 += m_dvGain[j] * dvR12[j, i];

                    dHess += (double)this.Entry(i).Divers * (dvR12[n, i] * (Math.Exp(this.ACoefficient * dTerm12) * ((1.0 - this.ACoefficient * dTerm12)) - 1.0) / Math.Pow((Math.Exp(this.ACoefficient * dTerm12) - 1.0), 2.0) - dvR01[n, i]);
                }
                else if (this.Entry(i).PSIMildDCS)
                    dHess -= (double)this.Entry(i).Divers * (dvR01[n, i] + dvR12[n, i]);
                else
                    dHess -= (double)this.Entry(i).Divers * dvR03[n, i];
            } // for i

            return dHess;
        }

        public double OptimalGainTrinomialTimeOfOnsetGainGainCrossDerivative(int m, int n, double[,] dvR12)
        { // verified 20090221
            // see equation 52 in Howle's decompression sickness 1 lab notebook

            double dHess = new double();

            for (int i = 0; i < this.Profiles; i++)
            {
                if (this.Entry(i).PSISeriousDCS)
                {
                    double dTerm12 = new double();
                    for (int j = 0; j < m_dvGain.Length; j++)
                        dTerm12 += m_dvGain[j] * dvR12[j, i];

                    dHess -= (double)this.Entry(i).Divers * this.ACoefficient * dvR12[m, i] * dvR12[n, i] * Math.Exp(this.ACoefficient * dTerm12) / Math.Pow(Math.Exp(dTerm12) - 1.0, 2.0);
                }
                else if (this.Entry(i).PSIMildDCS)
                {
                    double dTerm12 = new double();
                    for (int j = 0; j < m_dvGain.Length; j++)
                        dTerm12 += m_dvGain[j] * dvR12[j, i];

                    dHess -= (double)this.Entry(i).Divers * dvR12[m, i] * dvR12[n, i] * Math.Exp(this.ACoefficient * dTerm12) / Math.Pow(Math.Exp(dTerm12) - 1.0, 2.0);
                }
            } // for i

            return dHess;
        }

        public void OptimalGainTrinomialTimeOfOnsetOptimize(MODEL m)
        { // verified 02/21/2009
            // calculate R once
            double[,] dvR01 = new double[this.m_dvGain.Length, this.Profiles];
            double[,] dvR12 = new double[this.m_dvGain.Length, this.Profiles];
            double[,] dvR03 = new double[this.m_dvGain.Length, this.Profiles];
            OptimalGainTrinomialTimeOfOnsetHazard(dvR01, dvR12, dvR03, m);

            // create a temporary gain set and step size
            double dDamping = new double();
            double dEpsilon = new double();
            int iIteration = new int();
            int iMaxIteration = new int();

            dDamping = 0.5;
            dEpsilon = 1.0e-12;
            iMaxIteration = 500;

            GeneralMatrix mHess = new GeneralMatrix(1 + m_dvGain.Length, 1 + m_dvGain.Length);
            GeneralVector vGrad = new GeneralVector(1 + m_dvGain.Length);

            for (iIteration = 0; iIteration < iMaxIteration; iIteration++)
            {
                double dAGrad = OptimalGainTrinomialTimeOfOnsetASlope(dvR01, dvR12, dvR03);
                double dACurv = OptimalGainTrinomialTimeOfOnsetACurvature(dvR12);
                double[] dvGrad = OptimalGainTrinomialTimeOfOnsetGainSlope(dvR01, dvR12, dvR03);
                double[] dvCurv = OptimalGainTrinomialTimeOfOnsetGainCurvature(dvR12);

                // load the Hessian and gradient components
                for (int i = 0; i < 1 + m_dvGain.Length; i++)
                {
                    if (i == 0)
                    {
                        mHess[i, i] = dACurv;
                        vGrad[i] = dAGrad;
                        for (int j = i + 1; j < 1 + m_dvGain.Length; j++)
                            mHess[i, j] = mHess[j, i] = OptimalGainTrinomialTimeOfOnsetAGainCrossDerivative(j - 1, dvR01, dvR12, dvR03);
                    }
                    else
                    {
                        mHess[i, i] = dvCurv[i - 1];
                        vGrad[i] = dvGrad[i - 1];
                        for (int j = i + 1; j < 1 + m_dvGain.Length; j++)
                            mHess[i, j] = mHess[j, i] = OptimalGainTrinomialTimeOfOnsetGainGainCrossDerivative(i - 1, j - 1, dvR12);
                    }
                }

                // solve the system
                Vector vSol = mHess.Solve(vGrad, false);

                // update the solution
                double dSupNorm = new double();
                for (int i = 0; i < vSol.Length; i++)
                {
                    dSupNorm += Math.Abs(vSol[i]);
                    double dIncrement = dDamping * vSol[i];
                    if (i == 0)
                    {
                        m_dACoefficient -= dIncrement;
                        m_dACoefficient = Math.Max(m_dACoefficient, 0.0);
                    }
                    else
                    {
                        m_dvGain[i - 1] -= dIncrement;
                        m_dvGain[i - 1] = Math.Max(m_dvGain[i - 1], 0.0);
                    }
                } // i

                if (dSupNorm < dEpsilon)
                    break;
            } // iteration

            if (iIteration >= iMaxIteration - 1)
                throw new DCSException("Newton iteration failed to converge in Decompression.ExactGainTrinomial.OptimalGainTrinomialTimeOfOnsetOptimize");
        }

#endif
    }
}