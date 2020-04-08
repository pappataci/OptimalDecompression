using System;

namespace Decompression
{
    /// <summary>
    /// DiveDataCondition class - derives from DiveDataTissue class
    /// </summary>
    /// <typeparam name="P">profile of type ProfileCondition</typeparam>
    /// <typeparam name="N">node of type NodeCondition</typeparam>
    public class DiveDataCondition<P, N> : DiveDataTissue<P, N>
        where P : ProfileCondition<N>
        where N : NodeCondition
    {
        private double [ ] dvN2RateFactor = new double [ NodeTissue.NumberOfTissues ];
        private double [ ] dvO2RateFactor = new double [ NodeTissue.NumberOfTissues ];
        private double [ ] dvHeRateFactor = new double [ NodeTissue.NumberOfTissues ];

        private double [ ] dvImmersionFactor = new double [ NodeTissue.NumberOfTissues ];

        /// <summary>
        /// Default constructor
        /// </summary>
        public DiveDataCondition ( )
            : base ( )
        {
            for ( int i = 0; i < NodeTissue.NumberOfTissues; i++ )
            {
                dvN2RateFactor [ i ] = 1.0;
                dvO2RateFactor [ i ] = 1.0;
                dvHeRateFactor [ i ] = 1.0;
                dvImmersionFactor [ i ] = 1.0;
            }
        }

        /// <summary>
        /// Set/get immersion factor
        /// </summary>
        public double [ ] ImmersionFactor
        {
            set { dvImmersionFactor = value; CalculateN2Rate ( ); }
            get { return dvImmersionFactor; }
        }

        /// <summary>
        /// Set/get N2 exercise factor
        /// </summary>
        public double [ ] N2Factor
        {
            set { dvN2RateFactor = value; CalculateN2Rate ( ); }
            get { return dvN2RateFactor; }
        }

        /// <summary>
        /// Set/get O2 exercise factor
        /// </summary>
        public double [ ] O2Factor
        {
            set { dvO2RateFactor = value; CalculateO2Rate ( ); }
            get { return dvO2RateFactor; }
        }

        /// <summary>
        /// Set/get He exercise factor
        /// </summary>
        public double [ ] HeFactor
        {
            set { dvHeRateFactor = value; CalculateHeRate ( ); }
            get { return dvHeRateFactor; }
        }

        private void CalculateN2Rate ( )
        {
#warning this exception reports as unhandled
#if false
            foreach ( Double d in dvN2RateFactor )
                if ( d > 10.0 || d < 0.1 )
                    throw new DCSUtilities.DCSException ( "Bad N2 rate factor in Decompression.DiveDataCondtion.CalculateN2Rate" );
#endif
            foreach ( P p in DiveProfiles )
                for ( int i = 0; i < p.Nodes; i++ )
                {
                    if ( IsExercising ( i, p ) )
                    {
                        double [ ] rate = new double [ NodeTissue.NumberOfTissues ];
                        for ( int j = 0; j < NodeTissue.NumberOfTissues; j++ )
                            rate [ j ] = GetSingleN2Rate ( j ) * dvN2RateFactor [ j ];
                        p.Node ( i ).N2TissueRate = rate;
                    }
                    else
                        p.Node ( i ).N2TissueRate = N2TissueRate;

                    if ( IsWet ( p ) )
                    {
                        double [ ] rate = new double [ NodeTissue.NumberOfTissues ];
                        for ( int j = 0; j < NodeTissue.NumberOfTissues; j++ )
                            rate [ j ] = GetSingleN2Rate ( j ) * dvImmersionFactor [ j ];
                        p.Node ( i ).N2TissueRate = rate;
                    }
                    else
                        p.Node ( i ).N2TissueRate = N2TissueRate;
                }
        }

        private void CalculateO2Rate ( )
        {
            foreach ( P p in DiveProfiles )
                for ( int i = 0; i < p.Nodes; i++ )
                    p.Node ( i ).O2TissueRate = O2TissueRate;
        }

        private void CalculateHeRate ( )
        {
            foreach ( P p in DiveProfiles )
                for ( int i = 0; i < p.Nodes; i++ )
                    p.Node ( i ).HeTissueRate = HeTissueRate;
        }

        /// <summary>
        /// Set/get N2 tissue rate and recalculate exercise tissue condition rates
        /// </summary>
        public override double [ ] N2TissueRate
        {
            get
            {
                return base.N2TissueRate;
            }
            set
            {
                base.N2TissueRate = value;
                CalculateN2Rate ( );
            }
        }

        /// <summary>
        /// Set N2 tissue rate for a single tissue and recalculate exercise tissue condition rates
        /// </summary>
        /// <param name="t">tissue index</param>
        /// <param name="_r">tissue rate</param>
        public override void SetSingleN2Rate ( int t, double _r )
        {
            base.SetSingleN2Rate ( t, _r );
            CalculateSingleN2Rate ( t );
        }

        /// <summary>
        /// Set/get O2 tissue rate and recalculate exercise tissue condition rates
        /// </summary>
        public override double [ ] O2TissueRate
        {
            get
            {
                return base.O2TissueRate;
            }
            set
            {
                base.O2TissueRate = value;
                CalculateHeRate ( );
            }
        }

        /// <summary>
        /// Set O2 tissue rate for a single tissue and recalculate exercise tissue condition rates
        /// </summary>
        /// <param name="t">tissue index</param>
        /// <param name="_r">tissue rate</param>
        public override void SetSingleO2Rate ( int t, double _r )
        {
            base.SetSingleO2Rate ( t, _r );
            CalculateSingleO2Rate ( t );
        }

        /// <summary>
        /// Set/get He tissue rate and recalculate exercise tissue condition rates
        /// </summary>
        public override double [ ] HeTissueRate
        {
            get
            {
                return base.HeTissueRate;
            }
            set
            {
                base.HeTissueRate = value;
                CalculateHeRate ( );
            }
        }

        /// <summary>
        /// Set He tissue rate for a single tissue and recalculate exercise tissue condition rates
        /// </summary>
        /// <param name="t">tissue index</param>
        /// <param name="_r">tissue rate</param>
        public override void SetSingleHeRate ( int t, double _r )
        {
            base.SetSingleHeRate ( t, _r );
        }

        /// <summary>
        /// Calculate the node-by-node N2 rate for a single tissue for all profiles using the N2 tissue rate and N2 factor
        /// </summary>
        /// <param name="t">tissue index</param>
        private void CalculateSingleN2Rate ( int t )
        {
            foreach ( P p in DiveProfiles )
                for ( int n = 0; n < p.Nodes; n++ )
                    if ( IsExercising ( n, p ) )
                        p.Node ( n ).SetSingleN2Rate ( t, GetSingleN2Rate ( t ) * dvN2RateFactor [ t ] );
        }

        /// <summary>
        /// Calculate the node-by-node O2 rate for a single tissue for all profiles using the O2 tissue rate and O2 factor
        /// </summary>
        /// <param name="t">tissue index</param>
        private void CalculateSingleO2Rate ( int t )
        {
            foreach ( P p in DiveProfiles )
                for ( int n = 0; n < p.Nodes; n++ )
                    if ( IsExercising ( n, p ) )
                        p.Node ( n ).SetSingleO2Rate ( t, GetSingleO2Rate ( t ) * dvN2RateFactor [ t ] );
        }

        /// <summary>
        /// Calculate the node-by-node He rate for a single tissue for all profiles using the He tissue rate and He factor
        /// </summary>
        /// <param name="t">tissue index</param>
        private void CalculateSingleHeRate ( int t )
        {
            foreach ( P p in DiveProfiles )
                for ( int n = 0; n < p.Nodes; n++ )
                    if ( IsExercising ( n, p ) )
                        p.Node ( n ).SetSingleHeRate ( t, GetSingleHeRate ( t ) * dvHeRateFactor [ t ] );
        }

        /// <summary>
        /// Get node exercising status
        /// </summary>
        /// <param name="n">node</param>
        /// <param name="p">profile</param>
        /// <returns>true if exercising</returns>
        public static bool IsExercising ( int n, P p )
        {
            string s = p.FileName;

            if ( n == 0 )
                return false;

            #region dcw4.dat exceptions

            if ( s.Equals ( @"dc4w.dat", StringComparison.CurrentCultureIgnoreCase ) )
            {
                // singles
                if ( p.ProfileNumber == 15 )
                    if ( n == 2 )
                        return true;
                    else
                        return false;

                if ( p.ProfileNumber == 27 ||
                    p.ProfileNumber == 33 ||
                    p.ProfileNumber == 34 ||
                    p.ProfileNumber == 85 ||
                    p.ProfileNumber == 91 ||
                    p.ProfileNumber == 93 ||
                    p.ProfileNumber == 103 ||
                    p.ProfileNumber == 105 ||
                    p.ProfileNumber == 107 ||
                    p.ProfileNumber == 108 ||
                    p.ProfileNumber == 109 ||
                    p.ProfileNumber == 110 ||
                    p.ProfileNumber == 112 ||
                    p.ProfileNumber == 113 ||
                    p.ProfileNumber == 115 ||
                    p.ProfileNumber == 116 ||
                    p.ProfileNumber == 117 ||
                    p.ProfileNumber == 118 ||
                    p.ProfileNumber == 127 ||
                    p.ProfileNumber == 128 ||
                    p.ProfileNumber == 133 ||
                    p.ProfileNumber == 136 )
                    if ( n == 3 )
                        return true;
                    else
                        return false;

                if ( p.ProfileNumber == 29 ||
                    p.ProfileNumber == 57 ||
                    p.ProfileNumber == 58 ||
                    p.ProfileNumber == 60 )
                    if ( n == 4 )
                        return true;
                    else
                        return false;

                if ( p.ProfileNumber == 56 ||
                    p.ProfileNumber == 134 ||
                    p.ProfileNumber == 135 )
                    if ( n == 5 )
                        return true;
                    else
                        return false;

                if ( p.ProfileNumber == 54 )
                    if ( n == 6 )
                        return true;
                    else
                        return false;

                if ( p.ProfileNumber == 129 )
                    if ( n == 9 )
                        return true;
                    else
                        return false;

                // doubles
                if ( p.ProfileNumber == 87 ||
                    p.ProfileNumber == 97 )
                    if ( n == 3 || n == 4 )
                        return true;
                    else
                        return false;

                if ( p.ProfileNumber == 10 ||
                    p.ProfileNumber == 22 ||
                    p.ProfileNumber == 44 )
                    if ( n == 4 || n == 5 )
                        return true;
                    else
                        return false;

                if ( p.ProfileNumber == 26 )
                    if ( n == 5 || n == 6 )
                        return true;
                    else
                        return false;

                if ( p.ProfileNumber == 9 )
                    if ( n == 6 || n == 7 )
                        return true;
                    else
                        return false;

                if ( p.ProfileNumber == 5 ||
                    p.ProfileNumber == 143 )
                    if ( n == 7 || n == 8 )
                        return true;
                    else
                        return false;

                if ( p.ProfileNumber == 31 ||
                    p.ProfileNumber == 32 )
                    if ( n == 8 || n == 9 )
                        return true;
                    else
                        return false;

                // ranges
                if ( p.ProfileNumber == 6 ||
                    p.ProfileNumber == 16 ||
                    p.ProfileNumber == 20 ||
                    p.ProfileNumber == 50 )
                    if ( n > 2 && n < 6 )
                        return true;
                    else
                        return false;

                if ( p.ProfileNumber == 3 ||
                    p.ProfileNumber == 7 ||
                    p.ProfileNumber == 18 ||
                    p.ProfileNumber == 28 )
                    if ( n > 2 && n < 7 )
                        return true;
                    else
                        return false;

                if ( p.ProfileNumber == 8 )
                    if ( n > 3 && n < 8 )
                        return true;
                    else
                        return false;

                if ( p.ProfileNumber == 141 ||
                    p.ProfileNumber == 142 )
                    if ( n > 5 && n < 9 )
                        return true;
                    else
                        return false;
            }

            #endregion dcw4.dat exceptions

            #region edu885a.dat exceptions

            if ( s.Equals ( @"edu885a.dat", StringComparison.CurrentCultureIgnoreCase ) )
            {
                if ( p.ProfileNumber == 15 ||
                    p.ProfileNumber == 16 )
                {
                    if ( n == 3 )
                        return true;
                    else
                        return false;
                }
            }

            #endregion edu885a.dat exceptions

            #region edu885s.dat exceptions

            if ( s.Equals ( @"edu885s.dat", StringComparison.CurrentCultureIgnoreCase ) )
            {
                if ( p.ProfileNumber == 1 ||
                    p.ProfileNumber == 2 ||
                    p.ProfileNumber == 3 ||
                    p.ProfileNumber == 13 ||
                    p.ProfileNumber == 14 )
                {
                    if ( n > 3 && n < 7 )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 4 )
                {
                    if ( n > 5 && n < 9 )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 5 ||
                    p.ProfileNumber == 6 )
                {
                    if ( ( n > 3 && n < 9 ) || ( n == 15 ) || ( n > 21 && n < 25 ) )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 7 ||
                    p.ProfileNumber == 8 ||
                    p.ProfileNumber == 9 )
                {
                    if ( ( n > 3 && n < 8 ) || ( n > 12 && n < 16 ) )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 10 ||
                    p.ProfileNumber == 11 )
                {
                    if ( ( n > 3 && n < 8 ) || ( n == 17 ) || ( n > 20 && n < 24 ) )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 12 )
                {
                    if ( ( n > 3 && n < 9 ) || ( n > 14 && n < 19 ) )
                        return true;
                    else
                        return false;
                }
            }

            #endregion edu885s.dat exceptions

            #region edu885ar.dat exceptions

            if ( s.Equals ( @"edu885ar.dat", StringComparison.CurrentCultureIgnoreCase ) )
            {
                if ( p.ProfileNumber == 5 )
                {
                    if ( n == 3 || n == 16 )
                        return true;
                    else
                        return false;
                }
            }

            #endregion edu885ar.dat exceptions

            #region edu885as.dat exceptions

            if ( s.Equals ( @"edu885as.dat", StringComparison.CurrentCultureIgnoreCase ) )
            {
                if ( p.ProfileNumber == 1 ||
                    p.ProfileNumber == 2 ||
                    p.ProfileNumber == 3 )
                {
                    if ( n > 3 && n < 7 )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 4 )
                {
                    if ( n > 5 && n < 9 )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 5 ||
                    p.ProfileNumber == 6 )
                {
                    if ( ( n > 3 && n < 9 ) || ( n == 15 ) || ( n > 21 && n < 25 ) )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 7 ||
                    p.ProfileNumber == 8 ||
                    p.ProfileNumber == 9 )
                {
                    if ( ( n > 3 && n < 9 ) || ( n == 15 ) || ( n > 21 && n < 25 ) )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 10 ||
                    p.ProfileNumber == 11 )
                {
                    if ( ( n > 3 && n < 8 ) || ( n == 17 ) || ( n > 20 && n < 24 ) )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 12 )
                {
                    if ( ( n > 3 && n < 9 ) || ( n > 14 && n < 19 ) )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 13 ||
                    p.ProfileNumber == 14 )
                {
                    if ( n > 3 && n < 7 )
                        return true;
                    else
                        return false;
                }
            }

            #endregion edu885as.dat exceptions

            #region edu1180r.dat exceptions

            if ( s.Equals ( @"edu1180r.dat", StringComparison.CurrentCultureIgnoreCase ) )
            {
                if ( p.ProfileNumber == 2 ||
                    p.ProfileNumber == 3 )
                {
                    if ( n == 7 || n == 19 || n == 27 || n == 31 )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 4 )
                {
                    if ( n == 10 || n == 26 )
                        return true;
                    else
                        return false;
                }
            }

            #endregion edu1180r.dat exceptions

            #region nmr8697.dat exceptions

            if ( s.Equals ( @"nmr8697.dat", StringComparison.CurrentCultureIgnoreCase ) )
            {
                if ( p.ProfileNumber > 149 &&
                    p.ProfileNumber < 192 )
                {
                    if ( n > 0 && n < 4 )
                        return true;
                    else
                        return false;
                }
            }

            #endregion nmr8697.dat exceptions

            #region nmrnsw2.dat exceptions

            if ( s.Equals ( @"nmrnsw2.dat", StringComparison.CurrentCultureIgnoreCase ) )
            {
                if ( p.ProfileNumber == 1 )
                {
                    if ( n == 2 || n == 3 )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 15 )
                {
                    if ( n == 5 || n == 9 )
                        return true;
                    else
                        return false;
                }
            }

            #endregion nmrnsw2.dat exceptions

            #region pamla.dat exceptions

            if ( s.Equals ( @"pamla.dat", StringComparison.CurrentCultureIgnoreCase ) )
            {
                if ( p.ProfileNumber == 40 ||
                    p.ProfileNumber == 42 )
                {
                    if ( n == 7 || n == 11 )
                        return true;
                    else
                        return false;
                }

                if ( p.ProfileNumber == 41 )
                {
                    if ( n == 7 )
                        return true;
                    else
                        return false;
                }
            }

            #endregion pamla.dat exceptions

            if ( s.Equals ( @"edu849lt2.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu1157.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"dc8aod.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"asatare.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"asatarePSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"asatnsm.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"asatnsmPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"asatedu.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"asateduPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"surex.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"surexmcorrected.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"asatnmr.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"asatnmrPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"big292allinone.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"nmr9209.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"eduas45.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"asatdc.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"rnplx50.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"asatfr85.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"ups290.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"subx87.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"mauinostopcpsi.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"big292allinonewofull.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"subx87PSI.dat", StringComparison.CurrentCultureIgnoreCase ) )
                return false;

            if ( s.Equals ( @"nmr94eod.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"nsm6hr.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"nsm6hrPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu557.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu885a.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu885aPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"dc4w.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"dc4wPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu1351nl.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"nmr97nod.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"nmr97nodPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"nmrnsw2.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"nmrnsw.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"nmrnswPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"pasa.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"pasaPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"nmr8697.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"nmr8697PSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu1180s.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu1180sPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu885m.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu885mPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"dc4wr.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"dc4wrPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu885ar.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu885arPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"pamla.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"pamlaPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"para.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"paraPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu657.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu657corrected.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu184.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu184PSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu885s.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu885sPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"pamlaod.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"pamlaodPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"pamlaos.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"pamlaosPSI.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu1180r.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"edu1351d.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"dcasurw.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"dcsurepw.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"nmrosur90.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"nmrasur90.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"ExerTest.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.StartsWith ( "Test" ) ||
                s.Equals ( @"dc8aow.dat", StringComparison.CurrentCultureIgnoreCase ) )
            {
                // if (((Node)p.Node(n)).Depth >= 0.95 * p.MaxDepth)
                // {
                if ( ( ( Node ) p.Node ( n - 1 ) ).PressureRate > 0.0 &&
                    ( ( Node ) p.Node ( n ) ).PressureRate == 0.0 &&
                    ( ( Node ) p.Node ( n + 1 ) ).PressureRate < 0.0 )
                    return true;
                // }
                return false;
            }

            // string err = "Unknown exercise conditions for file " + s + " in Decompression.DiveDataCondition.IsExercising";
            // throw new DCSUtilities.DCSException(s);

            return false;
        }

        /// <summary>
        /// Get profile wet status
        /// </summary>
        /// <param name="p">profile</param>
        /// <returns>true if wet</returns>
        static public bool IsWet ( P p )
        {
            string s = p.FileName;

            if ( s.Equals ( @"asatare.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"asatedu.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"asatnmr.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"asatnsm.dat", StringComparison.CurrentCultureIgnoreCase ) ||
                s.Equals ( @"nsm6hr.dat", StringComparison.CurrentCultureIgnoreCase ) )
                return false;

            return true;
        }

        /// <summary>
        /// Get node resting status
        /// </summary>
        /// <param name="n">node index</param>
        /// <param name="p">profile</param>
        /// <returns>true if resting</returns>
        static public bool IsResting ( int n, P p )
        {
            return !IsExercising ( n, p );
        }

        /// <summary>
        /// Get profile dry static
        /// </summary>
        /// <param name="p">profile</param>
        /// <returns>true if dry</returns>
        static public bool IsDry ( P p )
        {
            return !IsWet ( p );
        }
    }
}