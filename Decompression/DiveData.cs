using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using DCSUtilities;
using System.Threading;
using Extreme.Statistics;

namespace Decompression
{
    /// <summary>
    /// DiveData class containing dive data
    /// </summary>
    /// <typeparam name="P">profile type with base Profile</typeparam>
    /// <typeparam name="N">node class with base Node</typeparam>
    public class DiveData<P, N>
        where P : Profile<N>
        where N : Node
    {
        /// <summary>
        /// Generic collection of dive profiles
        /// </summary>
        protected ProfileList<P, N> DiveProfiles = new ProfileList<P, N> ( );

        private DCS enLastDCS                             = DCS.END;
        private string sFileName                          = string.Empty;
        private string sDirectory                         = string.Empty;
        private static bool bFractionalMarginals          = true;
        private List<int> listFullDCSProfileIndicies      = null;
        private List<int> listMarginalDCSProfileIndicies  = null;
        private List<int> listNoDCSProfileIndicies        = null;
        private List<int> listPSIMildDCSProfileIndicies   = null;
        private List<int> listPSISeriousDCSProfileIndices = null;

        #region ODF histogram bounds

        public double [] ODFHistogramBins           = null;
        public double ODFHistogramStride            = 0.0;
        public static double ODFHistogramLowerBound = -9900.0;
        public static double ODFHistogramUpperBound =  9900.0;
        
        #endregion

        #region Divedata class methods and properties

        /// <summary>
        /// The dive data constructor
        /// </summary>
        public DiveData ( )
        {
            DiveProfiles.Clear ( );
            enLastDCS                       = DCS.END;
            listFullDCSProfileIndicies      = null;
            listMarginalDCSProfileIndicies  = null;
            listNoDCSProfileIndicies        = null;
            listPSISeriousDCSProfileIndices = null;
            listPSIMildDCSProfileIndicies   = null;

        }

        /// <summary>
        /// Indexer for the dive profiles contained in the DiveData profile collection
        /// </summary>
        /// <param name="index">profile index</param>
        /// <returns>generic profile of type P</returns>
        public P this [ int index ]
        {
            get { return DiveProfiles [ index ]; }
        }

        #endregion Divedata class methods and properties

        #region Dive profile methods and properties

        /// <summary>
        /// Set the data type
        /// </summary>
        /// <param name="s">string from a DCSDiveFile</param>
        /// <returns>DCS type enumeration</returns>
        public DCS Type ( string s )
        {

            string sDelim    = ",";
            char [ ] cDelim  = sDelim.ToCharArray ( );
            string [ ] sList = s.Split ( cDelim );

            if ( enLastDCS == DCS.END )
            {
                enLastDCS = DCS.HEADER1;
                return DCS.HEADER1;
            }

            if ( enLastDCS == DCS.HEADER1 )
            {
                enLastDCS = DCS.HEADER2;
                return DCS.HEADER2;
            }

            if ( ( enLastDCS == DCS.HEADER2 || enLastDCS == DCS.NODE ) && !s.Contains("-") /* s[0] != '-' */ ) //.IndexOf ( '-' ) == -1 )
            {
                enLastDCS = DCS.NODE;
                return DCS.NODE;
            }

            if ( enLastDCS == DCS.NODE && s.Contains("-") /* s [ 0 ] == '-' */ ) //.IndexOf ( '-' ) > -1 )
            {
                enLastDCS = DCS.END;
                return DCS.END;
            }

            return DCS.UNKNOWN;

        }

        /// <summary>
        /// Add a new profile, header, or node depending on the value of the passed string
        /// </summary>
        /// <param name="s">string from a DCSDiveFile</param>
        public void Add ( string s )
        {

            DCS d = this.Type ( s );

            switch ( d )
            {
                case DCS.HEADER1:

                    // add a now dive profile
                    this.AddProfile ( s );
                    break;

                case DCS.HEADER2:

                    // set the DCS header information
                    this.AddHeader ( s );
                    break;

                case DCS.NODE:

                    // add a dive node
                    this.AddNode ( s );
                    break;

                case DCS.END:

                    // finalize the dive profile
                    this.Generate ( );
                    break;

                default:
                    throw new DCSException ( "Bad message type in Decompression.DiveData.Add" );

                // break;
            }
        }

        /// <summary>
        /// Add a new dive profile to this dive data set
        /// </summary>
        /// <param name="s">string from DCSDiveFile</param>
        protected void AddProfile ( string s )
        {
            Type t = typeof ( P );
            System.Reflection.ConstructorInfo c = t.GetConstructor ( new Type [ ] { typeof ( string ), typeof ( string ) } );
            DiveProfiles.Add ( ( P ) c.Invoke ( new object [ ] { s, sFileName } ) );
        }

        /// <summary>
        /// Add the DCS header information to the most recently created dive profile
        /// </summary>
        /// <param name="s">string from DCSDiveFile</param>
        protected void AddHeader ( string s )
        {
            DiveProfiles [ DiveProfiles.Length - 1 ].AddHeader ( s );
        }

        /// <summary>
        /// Add a dive node to the most recently created dive profile
        /// </summary>
        /// <param name="s">string from DCSDiveFile</param>
        protected void AddNode ( string s )
        {
            DiveProfiles [ DiveProfiles.Length - 1 ].AddNode ( s );
        }

        /// <summary>
        /// Generate the profile information after the profile is loaded.
        /// This method inserts gas switch , T1, T2, and endpoint nodes
        /// and sets the gas rates. This function is called internally.
        /// The user does not need to call this function.
        /// </summary>
        protected void Generate ( )
        {

            #region gas switches

            // get the most recently created profile
            P p = DiveProfiles [ DiveProfiles.Length - 1 ];
            int iLimit = p.Nodes - 1;

            if ( p.Node ( iLimit ).IsGasSwitchNode )
            {
                p.InsertNode ( p.Nodes, p.Node ( p.Nodes - 1 ).Time + p.Node ( p.Nodes - 1 ).SwitchTime, p.Node ( p.Nodes - 1 ).Depth );
                iLimit = p.Nodes - 1;
            }

            // set the initial node
            SetGasPressuresByGasCode ( p.Node ( 0 ), p.OriginatingGas );

            for ( int i = iLimit; i > 0; i-- )
            {
                N n0 = p.Node ( i - 1 );
                N n1 = p.Node ( i );

                if ( n0.IsGasSwitchNode )
                { // process the gas switch
                    double dEndSwitchTime = n0.Time + n0.SwitchTime;

                    if ( IsStraddledSwitch ( n0, n1 ) )
                    { // switch ends between n0 and n1
                        // insert new node using n0 gas code
                        p.InsertNode ( i, dEndSwitchTime, InterpolatedDepth ( n0, dEndSwitchTime, n1 ) );
                        N ni = p.Node ( i );
                        SetGasPressuresByGasCode ( ni, n0.Gas );
                        ni.IsInsertedNode = true;
                        ni.IsSwitchEndNode = true;

                        // set n1 using n0 gas code
                        SetGasPressuresByGasCode ( n1, n0.Gas );
                    }
                    else if ( IsSwitchAtEndingNode ( n0, n1 ) )
                    { // switch ends at n1
                        // set n1 using n0 gas code
                        SetGasPressuresByGasCode ( n1, n0.Gas );
                        n1.IsSwitchEndNode = true;
                    }
                    else
                    { // switch ends after n1
                        // set the n0 contents
                        double dGas = p.OriginatingGas;
                        if ( i > 1 ) dGas = p.Node ( i - 2 ).Gas;
                        SetGasPressuresByGasCode ( n0, dGas );

                        if ( IsNodeAtTime ( p, dEndSwitchTime, i ) )
                        { // there is a node at switch end time
                            // modify the node at dEndSwitchTime
                            N ni = NodeAtTime ( p, dEndSwitchTime, i );
                            SetGasPressuresByGasCode ( ni, n0.Gas );
                            ni.IsSwitchEndNode = true;
                        }
                        else
                        { // there is not a node at switch end time
                            // insert a node at dEndSwitchTime
                            int l = IndexOfNodeBeforeTime ( p, dEndSwitchTime, i );
                            p.InsertNode ( l + 1, dEndSwitchTime, InterpolatedDepth ( p.Node ( l ), dEndSwitchTime, p.Node ( l + 1 ) ) );
                            N ni = p.Node ( l + 1 );
                            SetGasPressuresByGasCode ( ni, n0.Gas );
                            ni.IsInsertedNode = true;
                            ni.IsSwitchEndNode = true;
                        }

                        // modify the straddled nodes i<=n<j
                        int j = IndexOfNodeAtTime ( p, dEndSwitchTime, i );
                        N ne = NodeAtTime ( p, dEndSwitchTime, i );
                        for ( int k = i; k < j; k++ )
                        {
                            N n = p.Node ( k );
                            n.IsStraddledNode = true;
                            InterpolateNodeStraddledByGasSwitch ( n0, n, ne );
                        }
                    }
                }
                else if ( n1.IsGasSwitchNode )
                { // set n1 using n0 gas code - contents might be modified later
                    SetGasPressuresByGasCode ( n1, n0.Gas );
                }
                else
                { // set the n1 using its gas code - contents might be modified later
                    SetGasPressuresByGasCode ( n1 );
                }
            }

            #endregion gas switches

            #region append an ending node

            double dIntegrationLimit = this.LastSurfaceTime ( p );
            if ( p.IsSaturationDive )

                // this is a saturation dive
                dIntegrationLimit += 48.0 * 60.0;
            else

                // this is a bounce dive
                dIntegrationLimit += 24.0 * 60.0;

            double dLastNodeTime = p.Node ( p.Nodes - 1 ).Time;

            if ( ( dIntegrationLimit - dLastNodeTime ) > Profile<N>.TimeTolerance )
            {
#if true
                // append a node to the end of the profile
                p.InsertNode ( p.Nodes, dIntegrationLimit, p.Node ( p.Nodes - 1 ).Depth );
                p.Node ( p.Nodes - 1 ).Pressure       = p.Node ( p.Nodes - 2 ).Pressure;
                p.Node ( p.Nodes - 1 ).N2Pressure     = p.Node ( p.Nodes - 2 ).N2Pressure;
                p.Node ( p.Nodes - 1 ).O2Pressure     = p.Node ( p.Nodes - 2 ).O2Pressure;
                p.Node ( p.Nodes - 1 ).HePressure     = p.Node ( p.Nodes - 2 ).HePressure;
                p.Node ( p.Nodes - 1 ).IsInsertedNode = true;
#else
                p.Node ( p.Nodes - 1 ).Time = dIntegrationLimit;
#endif
            }
            else if ( Math.Abs ( dIntegrationLimit - dLastNodeTime ) <= Profile<N>.TimeTolerance )
            {
                // last node is at the integration limit - no action needed
            }
            else if ( ( dIntegrationLimit - dLastNodeTime ) < -Profile<N>.TimeTolerance )
            {
                // last node time is beyond the intetration limit - no action needed
            }
            else
                throw new DCSException ( "Bad integration limit logic in Decompression.DiveData.Generate" );

            #endregion append an ending node

            #region T1 and T2 nodes

            if ( p.IsAnyDCS )
            {
                double dT1 = new double ( );
                double dT2 = new double ( );

                if ( p.GoodTimes )
                {
                    dT1 = p.Time1;
                    dT2 = p.Time2;
                }
                else
                {
                    dT1 = p.FirstDecompressionStartTime;
                    if ( p.IsSaturationDive )
                        dT2 = 48.0 * 60.0 + p.LastSurfaceTime;
                    else
                        dT2 = 24.0 * 60.0 + p.LastSurfaceTime;

                    p.Time1 = dT1;
                    p.Time2 = dT2;
                }

                // process T1
                if ( Math.Abs ( p.Node ( p.MostRecentNodeIndex ( dT1 ) ).Time - dT1 ) <= Profile<N>.TimeTolerance )
                { // a node exists at T1
                    p.Node ( p.MostRecentNodeIndex ( dT1 ) ).IsT1Node = true;
                }
                else
                { // inset a new node
                    N n0 = p.Node ( p.MostRecentNodeIndex ( dT1 ) );
                    N n1 = p.Node ( p.MostRecentNodeIndex ( dT1 ) + 1 );

                    p.InsertNode ( p.MostRecentNodeIndex ( dT1 ) + 1, dT1, 0.0 );
                    N ni                 = p.Node ( p.MostRecentNodeIndex ( dT1 ) );
                    ni.IsInsertedNode    = true;
                    ni.IsT1Node          = true;
                    double dTimeFraction = ( ni.Time - n0.Time ) / ( n1.Time - n0.Time );
                    ni.Depth             = n0.Depth + dTimeFraction * ( n1.Depth - n0.Depth );
                    ni.Pressure          = GAS.Pressure ( ni.Depth );
                    ni.N2Pressure        = n0.N2Pressure + dTimeFraction * ( n1.N2Pressure - n0.N2Pressure );
                    ni.O2Pressure        = n0.O2Pressure + dTimeFraction * ( n1.O2Pressure - n0.O2Pressure );
                    ni.HePressure        = n0.HePressure + dTimeFraction * ( n1.HePressure - n0.HePressure );
                }

                // process T2
                if ( Math.Abs ( p.Node ( p.MostRecentNodeIndex ( dT2 ) ).Time - dT2 ) <= Profile<N>.TimeTolerance )
                { // a node exists at T2
                    p.Node ( p.MostRecentNodeIndex ( dT2 ) ).IsT2Node = true;
                }
                else if ( dT2 > p.Node ( p.Nodes - 1 ).Time )
                { // T2 is beyond the end of the profile, append a new node
                    p.InsertNode ( p.MostRecentNodeIndex ( dT2 ) + 1, dT2, p.Node ( p.Nodes - 1 ).Depth );
                    N n0              = p.Node ( p.Nodes - 2 );
                    N ni              = p.Node ( p.Nodes - 1 );
                    ni.IsInsertedNode = true;
                    ni.IsT2Node       = true;
                    ni.Depth          = n0.Depth;
                    ni.Pressure       = n0.Pressure;
                    ni.N2Pressure     = n0.N2Pressure;
                    ni.O2Pressure     = n0.O2Pressure;
                    ni.HePressure     = n0.HePressure;
                }
                else
                { // inset a new node
                    N n0 = p.Node ( p.MostRecentNodeIndex ( dT2 ) );
                    N n1 = p.Node ( p.MostRecentNodeIndex ( dT2 ) + 1 );

                    p.InsertNode ( p.MostRecentNodeIndex ( dT2 ) + 1, dT2, 0.0 );
                    N ni = p.Node ( p.MostRecentNodeIndex ( dT2 ) );
                    ni.IsInsertedNode = true;
                    ni.IsT2Node = true;
                    double dTimeFraction = ( ni.Time - n0.Time ) / ( n1.Time - n0.Time );
                    ni.Depth = n0.Depth + dTimeFraction * ( n1.Depth - n0.Depth );
                    ni.Pressure = GAS.Pressure ( ni.Depth );
                    ni.N2Pressure = n0.N2Pressure + dTimeFraction * ( n1.N2Pressure - n0.N2Pressure );
                    ni.O2Pressure = n0.O2Pressure + dTimeFraction * ( n1.O2Pressure - n0.O2Pressure );
                    ni.HePressure = n0.HePressure + dTimeFraction * ( n1.HePressure - n0.HePressure );
                }
            }

            // set the T1 and T2 indicies
            for ( int i = 0; i < p.Nodes; i++ )
            {
                if ( p.Node ( i ).IsT1Node )
                    p.T1NodeIndex = i;
                if ( p.Node ( i ).IsT2Node )
                    p.T2NodeIndex = i;
            }

            #endregion T1 and T2 nodes

            #region break up long final surface intervals
#if false
            var bStraddled   = p.Node ( p.Nodes - 2 ).IsStraddledNode;
            var bSwitch       = p.Node ( p.Nodes - 2 ).IsGasSwitchNode;

            if ( bStraddled )
                throw new DCSException ( "Unprocessed straddled node in Decompression.DiveData<P,N>.Generate()" );

            if ( bSwitch )
                throw new DCSException ( "Unprocessed switch node in Decompression.DiveData<P,N>.Generate()" );

            var finalTime = new double ( );
            var previousTime = new double ( );

            do
            {

                finalTime = p.Node ( p.Nodes - 1 ).Time;
                previousTime = p.Node ( p.Nodes - 2 ).Time;

                if ( finalTime - previousTime > Profile < N >.LargeSurfaceInterval )
                {

                    N n0 = p.Node ( p.MostRecentNodeIndex ( previousTime ) );
                    N n1 = p.Node ( p.MostRecentNodeIndex ( previousTime ) + 1 );

                    p.InsertNode ( p.MostRecentNodeIndex ( previousTime ) + 1,
                        previousTime + Profile < N >.LargeSurfaceInterval, 0.0 );
                    N ni = p.Node ( p.MostRecentNodeIndex ( previousTime + Profile < N >.LargeSurfaceInterval + 1 ) );
                    ni.IsInsertedNode = true;
                    double dTimeFraction = ( ni.Time - n0.Time ) / ( n1.Time - n0.Time );
                    ni.Depth = n0.Depth + dTimeFraction * ( n1.Depth - n0.Depth );
                    ni.Pressure = GAS.Pressure ( ni.Depth );
                    ni.N2Pressure = n0.N2Pressure + dTimeFraction * ( n1.N2Pressure - n0.N2Pressure );
                    ni.O2Pressure = n0.O2Pressure + dTimeFraction * ( n1.O2Pressure - n0.O2Pressure );
                    ni.HePressure = n0.HePressure + dTimeFraction * ( n1.HePressure - n0.HePressure );

                }

            } while ( finalTime - previousTime > Profile < N >.LargeSurfaceInterval ); 
#endif
            #endregion break up long final surface intervals

            #region set the gas pressure rates

            for ( int i = 1; i < p.Nodes; i++ )
            {
                N n0 = p.Node ( i - 1 );
                N n1 = p.Node ( i );
                double time = n1.Time - n0.Time;
                double press = n1.Pressure - n0.Pressure;
                double N2press = n1.N2Pressure - n0.N2Pressure;
                double O2press = n1.O2Pressure - n0.O2Pressure;
                double Hepress = n1.HePressure - n0.HePressure;
                n0.PressureRate = press / time;
                n0.N2PressureRate = N2press / time;
                n0.O2PressureRate = O2press / time;
                n0.HePressureRate = Hepress / time;
            }
            N n2 = p.Node ( p.Nodes - 1 );
            n2.PressureRate = 0.0;
            n2.N2PressureRate = 0.0;
            n2.O2PressureRate = 0.0;
            n2.HePressureRate = 0.0;

            #endregion set the gas pressure rates

        }

        private void InterpolateNodeStraddledByGasSwitch ( N n0, N n, N n1 )
        {
            double frac = new double ( );
            double fgas0 = new double ( );
            double fgas1 = new double ( );

            n.Pressure = GAS.Pressure ( n.Depth );

            frac = ( n.Time - n0.Time ) / ( n1.Time - n0.Time );

            fgas0 = n0.N2Pressure / n0.Pressure;
            fgas1 = n1.N2Pressure / n1.Pressure;
            n.N2Pressure = n.Pressure * ( fgas0 + frac * ( fgas1 - fgas0 ) );

            fgas0 = n0.O2Pressure / n0.Pressure;
            fgas1 = n1.O2Pressure / n1.Pressure;
            n.O2Pressure = n.Pressure * ( fgas0 + frac * ( fgas1 - fgas0 ) );

            fgas0 = n0.HePressure / n0.Pressure;
            fgas1 = n1.HePressure / n1.Pressure;
            n.HePressure = n.Pressure * ( fgas0 + frac * ( fgas1 - fgas0 ) );
        }

        private N NodeAtTime ( P p, double t, int i )
        {
            for ( int j = i; j < p.Nodes; j++ )
                if ( Math.Abs ( p.Node ( j ).Time - t ) <= Profile<N>.TimeTolerance )
                    return p.Node ( j );

            throw new DCSException ( "No node found at inidated time in Decompression.DiveData.NodeAtTime" );
        }

        private int IndexOfNodeBeforeTime ( P p, double t, int i )
        {
            for ( int j = p.Nodes - 1; j >= i; j-- )
                if ( p.Node ( j ).Time - t < -Profile<N>.TimeTolerance )
                    return j;

            throw new DCSException ( "No node found before indicated time in Decompression.DiveData.IndexOfNodeBeforeTime" );
        }

        private int IndexOfNodeAtTime ( P p, double t, int i )
        {
            for ( int j = i; j < p.Nodes; j++ )
                if ( Math.Abs ( p.Node ( j ).Time - t ) <= Profile<N>.TimeTolerance )
                    return j;

            throw new DCSException ( "No node found at inidated time in Decompression.DiveData.IndexOfNodeAtTime" );
        }

        private bool IsNodeAtTime ( P p, double t, int i )
        {
            for ( int j = i; j < p.Nodes; j++ )
                if ( Math.Abs ( p.Node ( j ).Time - t ) <= Profile<N>.TimeTolerance )
                    return true;

            return false;
        }

        private double InterpolatedDepth ( Node n0, double time, Node n1 )
        {
            if ( time < n0.Time || time > n1.Time )
                throw new DCSException ( "Time out of range in Decompression.DiveData.InterpolatedDepth" );

            return n0.Depth + ( n1.Depth - n0.Depth ) * ( time - n0.Time ) / ( n1.Time - n0.Time );
        }

        private bool IsStraddledSwitch ( Node n0, Node n1 )
        {
            if ( n1.Time - ( n0.Time + n0.SwitchTime ) > Profile<N>.TimeTolerance )
                return true;

            return false;
        }

        private bool IsSwitchAtEndingNode ( Node n0, Node n1 )
        {
            if ( Math.Abs ( n1.Time - ( n0.Time + n0.SwitchTime ) ) <= Profile<N>.TimeTolerance )
                return true;

            return false;
        }

        private void SetGasPressuresByGasCode ( Node n )
        {
            n.Pressure = GAS.Pressure ( n.Depth );
            n.N2Pressure = GAS.N2PressureByGasCode ( n.Gas, n.Pressure );
            n.O2Pressure = GAS.O2PressureByGasCode ( n.Gas, n.Pressure );
            n.HePressure = GAS.HePressureByGasCode ( n.Gas, n.Pressure );
        }

        private void SetGasPressuresByGasCode ( Node n, double g )
        {
            n.Pressure = GAS.Pressure ( n.Depth );
            n.N2Pressure = GAS.N2PressureByGasCode ( g, n.Pressure );
            n.O2Pressure = GAS.O2PressureByGasCode ( g, n.Pressure );
            n.HePressure = GAS.HePressureByGasCode ( g, n.Pressure );
        }

        private void InterpolateGasPressures ( Node n0, Node ni, Node n1 )
        {
            double dTF = new double ( );
            dTF = ( ni.Time - n0.Time ) / ( n1.Time - n0.Time );
            ni.N2Pressure = n0.N2Pressure + dTF * ( n1.N2Pressure - n0.N2Pressure );
            ni.O2Pressure = n0.O2Pressure + dTF * ( n1.O2Pressure - n0.O2Pressure );
            ni.HePressure = n0.HePressure + dTF * ( n1.HePressure - n0.HePressure );
        }

        private void InterpolateDepthAndPressure ( Node n0, Node ni, Node n1 )
        {
            ni.Depth = n0.Depth + ( n1.Depth - n0.Depth ) * ( ni.Time - n0.Time ) / ( n1.Time - n0.Time );
            ni.Pressure = GAS.Pressure ( ni.Depth );
        }

        /// <summary>
        /// Get the number of dive profiles in this data set.
        /// </summary>
        public int Profiles { get { return DiveProfiles.Length; } }

        /// <summary>
        /// Glear the dive data set
        /// </summary>
        public void Clear ( )
        {
            foreach ( Profile<N> d in DiveProfiles )
            {
                d.Clear ( );
            }
            DiveProfiles.Clear ( );
            Profile<N>.ZeroProfileCounter ( );

            listFullDCSProfileIndicies.Clear ( );
            listMarginalDCSProfileIndicies.Clear ( );
            listNoDCSProfileIndicies.Clear ( );
            listPSIMildDCSProfileIndicies.Clear ( );
            listPSISeriousDCSProfileIndices.Clear ( );

            listFullDCSProfileIndicies      = null;
            listMarginalDCSProfileIndicies  = null;
            listNoDCSProfileIndicies        = null;
            listPSIMildDCSProfileIndicies   = null;
            listPSISeriousDCSProfileIndices = null;

        }

        #endregion Dive profile methods and properties

        #region Decompression sickness occurrence specific methods and properties

        /// <summary>
        /// Set/get fractional marginal DCS use flag
        /// </summary>
        public static bool FractionalMarginals { set { bFractionalMarginals = value; } get { return bFractionalMarginals; } }

        /// <summary>
        /// Get the number of full DCS cases in this data set
        /// </summary>
        /// <returns>number of full DCS cases</returns>
        public int FullDCS ( )
        {
#warning add a memory here
            int i = 0;
            foreach ( Profile<N> p in DiveProfiles )
            {
                if ( p.IsFullDCS )
                    i += ( int ) p.Divers;
            }
            return i;
        }

        /// <summary>
        /// Get the number of marginal DCS cases in this data set
        /// </summary>
        /// <returns>number of marginal DCS cases</returns>
        public int MarginalDCS ( )
        {
#warning add a memory here
            int i = 0;
            foreach ( Profile<N> p in DiveProfiles )
            {
                if ( p.IsMarginalDCS )
                    i += ( int ) p.Divers;
            }
            return i;
        }

        /// <summary>
        /// Reports the number of full DCS cases without T1, T2 times
        /// </summary>
        /// <returns>number of full DCS cases without symptom onset times</returns>
        public int FullDCSWithoutTimes ( )
        {
#warning add a memory here
            int i = 0;
            foreach ( Profile<N> p in DiveProfiles )
            {
                if ( p.IsFullDCS && !p.GoodTimes )
                    i += ( int ) p.Divers;
            }
            return i;
        }

        /// <summary>
        /// Reports the number of marginal DCS cases without T1, T2 times
        /// </summary>
        /// <returns>number of marginal DCS cases without symptom onset times</returns>
        public int MarginalDCSWithoutTimes ( )
        {
#warning add a memory here
            int i = 0;
            foreach ( Profile<N> p in DiveProfiles )
            {
                if ( p.IsMarginalDCS && !p.GoodTimes )
                    i += ( int ) p.Divers;
            }
            return i;
        }

        /// <summary>
        /// Reports the number of DCS cases of any severtiy level without T1, T2 times
        /// </summary>
        /// <returns>number of DCS cases without symptom onset times</returns>
        public int AllDCSWithoutTimes ( )
        {
#warning add a memory here
            int i = 0;
            foreach ( Profile<N> p in DiveProfiles )
            {
                if ( ( p.IsFullDCS || p.IsMarginalDCS ) && !p.GoodTimes )
                    i += ( int ) p.Divers;
            }
            return i;
        }

        /// <summary>
        /// Get the number of all DCS cases in this data set
        /// </summary>
        /// <returns>total number of DCS cases (marginal + full)</returns>
        public int AllDCS ( )
        {
#warning add a memory here
            int i = 0;
            foreach ( Profile<N> p in DiveProfiles )
            {
                if ( p.IsFullDCS || p.IsMarginalDCS )
                    i += ( int ) p.Divers;
            }
            return i;
        }

        /// <summary>
        /// Get the fraction of full DCS cases in this data set
        /// </summary>
        /// <returns>fraction of full DCS cases</returns>
        public double FractionFullDCS ( )
        {
            return ( ( double ) FullDCS ( ) / ( double ) Profiles );
        }

        /// <summary>
        /// Get the fraction of marginal DCS cases in this data set
        /// </summary>
        /// <returns>fraction of marginal DCS cases</returns>
        public double FractionMarginalDCS ( )
        {
            return ( ( double ) MarginalDCS ( ) / ( double ) Profiles );
        }

        /// <summary>
        /// Get the fraction of all DCS cases in this data set
        /// </summary>
        /// <returns>fraction of all (marginal + full) DCS cases</returns>
        public double FractionAllDCS ( )
        {
            return ( ( double ) AllDCS ( ) / ( double ) Profiles );
        }

        /// <summary>
        /// Get the percentage of full DCS cases in this data set
        /// </summary>
        /// <returns>percentage of full DCS cases</returns>
        public double PercentFullDCS ( )
        {
            return 100.0 * FractionFullDCS ( );
        }

        /// <summary>
        /// Get the percentage of marginal DCS cases in this data set
        /// </summary>
        /// <returns>percentage of marginal DCS cases</returns>
        public double PercentMarginalDCS ( )
        {
            return 100.0 * FractionMarginalDCS ( );
        }

        /// <summary>
        /// get the percentage of all DCS cases in this data set
        /// </summary>
        /// <returns>percentage of all (marginal + full) DCS cases</returns>
        public double PercentAllDCS ( )
        {
            return 100.0 * FractionAllDCS ( );
        }

        /// <summary>
        /// Get the value of marginal DCS cases in the entire data set
        /// </summary>
        /// <param name="d">marginal DCS value</param>
        public void SetMarginalDCS ( double d )
        {
            if ( Math.Abs ( d ) <= 1.0e-6 || Math.Abs ( d - 1.0 ) <= 1.0e-6 )
                DiveData<P, N>.FractionalMarginals = false;
            else
                DiveData<P, N>.FractionalMarginals = true;

            foreach ( Profile<N> p in DiveProfiles )
                p.SetMarginalDCS ( d );

            listFullDCSProfileIndicies = null;
            listMarginalDCSProfileIndicies = null;
            listNoDCSProfileIndicies = null;
        }

        /// <summary>
        /// Get the total DCS value for the data set
        /// </summary>
        /// <returns>total DCS</returns>
        public double TotalDCS ( )
        {
#warning add a memory here
            double d = 0.0;
            foreach ( Profile<N> p in this.DiveProfiles )
            {
                d += p.DCS * p.Divers;
            }
            return d;
        }

        /// <summary>
        /// Resets all marginal DCS cases to no-DCS cases
        /// </summary>
        public void ResetMarginalsToNoDCS ( )
        {
            DiveData<P, N>.FractionalMarginals = false;
            foreach ( Profile<N> p in this.DiveProfiles )
                if ( p.IsMarginalDCS )
                    p.SetMarginalToNoDCS ( );

            listFullDCSProfileIndicies = null;
            listMarginalDCSProfileIndicies = null;
            listNoDCSProfileIndicies = null;
        }

        /// <summary>
        /// Resets all marginal DCS cases to full DCS cases
        /// </summary>
        public void ResetMarginalsToFullDCS ( )
        {
            DiveData<P, N>.FractionalMarginals = false;
            foreach ( Profile<N> p in this.DiveProfiles )
                if ( p.IsMarginalDCS )
                    p.SetMarginalToFullDCS ( );

            listFullDCSProfileIndicies = null;
            listMarginalDCSProfileIndicies = null;
            listNoDCSProfileIndicies = null;
        }

        /// <summary>
        /// Resets full DCS cases to no-DCS cases
        /// </summary>
        public void ResetFullToNoDCS ( )
        {
            foreach ( Profile<N> p in this.DiveProfiles )
                if ( p.IsFullDCS )
                    p.SetFullToNoDCS ( );

            listFullDCSProfileIndicies = null;
            listMarginalDCSProfileIndicies = null;
            listNoDCSProfileIndicies = null;
        }

        /// <summary>
        /// Resets full DCS exposures to serious PSI status
        /// </summary>
        public void ResetFullToSeriousDCS ( )
        {
            foreach ( Profile<N> p in this.DiveProfiles )
                if ( p.IsFullDCS )
                    p.SetFullToSeriousDCS ( );

            listFullDCSProfileIndicies = null;
            listMarginalDCSProfileIndicies = null;
            listNoDCSProfileIndicies = null;
        }

        /// <summary>
        /// Resets marginal DCS exposures to mild PSI status
        /// </summary>
        public void ResetMarginalsToMildDCS ( )
        {
            DiveData<P, N>.FractionalMarginals = false;
            foreach ( Profile<N> p in this.DiveProfiles )
                if ( p.IsMarginalDCS )
                    p.SetMarginalToMildDCS ( );

            listFullDCSProfileIndicies = null;
            listMarginalDCSProfileIndicies = null;
            listNoDCSProfileIndicies = null;
        }

        /// <summary>
        /// Deletes full DCS profiles
        /// </summary>
        public void RemoveFullDCS ( )
        {
            for ( int i = DiveProfiles.Length - 1; i >= 0; i-- )
                if ( DiveProfiles [ i ].IsFullDCS )
                    DiveProfiles.RemoveAt ( i );

            listFullDCSProfileIndicies     = null;
            listMarginalDCSProfileIndicies = null;
            listNoDCSProfileIndicies       = null;
        }

        /// <summary>
        /// Delete mild DCS according to PSI code.
        /// </summary>
        public void RemoveMildDCS ( )
        {

            for ( int i = DiveProfiles.Length - 1 ; i >= 0 ; i-- )
                if ( DiveProfiles [ i ].PSIMildDCS )
                    DiveProfiles.RemoveAt ( i );

            listFullDCSProfileIndicies     = null;
            listMarginalDCSProfileIndicies = null;
            listNoDCSProfileIndicies       = null;
            
        }

        /// <summary>
        /// Delete serious DCS according to PSI classification.
        /// </summary>
        public void RemoveSeriousDCS ( )
        {

            for ( int i = DiveProfiles.Length - 1 ; i >= 0 ; i-- )
                if ( DiveProfiles [ i ].PSISeriousDCS )
                    DiveProfiles.RemoveAt ( i );

            listFullDCSProfileIndicies     = null;
            listMarginalDCSProfileIndicies = null;
            listNoDCSProfileIndicies       = null;


        }

        /// <summary>
        /// Delete DCS cases with certian PSI codes.
        /// </summary>
        /// <param name="codes">1D array of PSI enumeratoin codes to remove.</param>
        public void RemoveDCSByPSICode ( PSI [] codes)
        {

            for ( int i = DiveProfiles.Length - 1 ; i >= 0 ; i-- )
            {

                var containsCode = false;

                for ( int j = 0 ; j < codes.Length ; j++ )
                    if ( DiveProfiles [ i ].PSIIndex == codes [ j ] )
                        containsCode = true;

                if (containsCode)
                    DiveProfiles.RemoveAt ( i );

            }

            listFullDCSProfileIndicies = null;
            listMarginalDCSProfileIndicies = null;
            listNoDCSProfileIndicies = null;
            
        }     

        /// <summary>
        /// Deletes marginal DCS profiles
        /// </summary>
        public void RemoveMarginalDCS ( )
        {
            DiveData<P, N>.FractionalMarginals = false;
            for ( int i = DiveProfiles.Length - 1; i >= 0; i-- )
                if ( DiveProfiles [ i ].IsMarginalDCS )
                    DiveProfiles.RemoveAt ( i );

            listFullDCSProfileIndicies = null;
            listMarginalDCSProfileIndicies = null;
            listNoDCSProfileIndicies = null;
        }

        /// <summary>
        /// Deletes profiles of any DCS type if the profiles do not have T1, T2 times
        /// </summary>
        public void RemoveAnyDCSWithoutOnsetTimes ( )
        {
            for ( int i = DiveProfiles.Length - 1; i >= 0; i-- )
                if ( DiveProfiles [ i ].IsAnyDCS && !DiveProfiles [ i ].GoodTimes )
                    DiveProfiles.RemoveAt ( i );

            listFullDCSProfileIndicies = null;
            listMarginalDCSProfileIndicies = null;
            listNoDCSProfileIndicies = null;
        }

        public void RemoveNoDCS ( )
        {
            for ( int i = DiveProfiles.Length - 1; i >= 0; i-- )
                if ( !DiveProfiles [ i ].IsAnyDCS )
                    DiveProfiles.RemoveAt ( i );

            listFullDCSProfileIndicies = null;
            listMarginalDCSProfileIndicies = null;
            listNoDCSProfileIndicies = null;
        }

        public List<int> FullDCSProfileIndicies
        {
            get
            {
                if ( listFullDCSProfileIndicies == null )
                {
                    listFullDCSProfileIndicies = new List<int> ( );
                    for ( int i = 0; i < this.Profiles; i++ )
                        if ( DiveProfiles [ i ].IsFullDCS )
                            listFullDCSProfileIndicies.Add ( i );
                }

                return listFullDCSProfileIndicies;
            }
        }

        public List<int> MarginalDCSProfileIndicies
        {
            get
            {
                if ( listMarginalDCSProfileIndicies == null )
                {
                    listMarginalDCSProfileIndicies = new List<int> ( );
                    for ( int i = 0; i < this.Profiles; i++ )
                        if ( DiveProfiles [ i ].IsMarginalDCS )
                            listMarginalDCSProfileIndicies.Add ( i );
                }

                return listMarginalDCSProfileIndicies;
            }
        }

        public List<int> NoDCSProfileIndicies
        {

            get
            {

                //var rwlock = new ReaderWriterLockSlim ( );

                //rwlock.EnterUpgradeableReadLock ( );

                if ( listNoDCSProfileIndicies == null )
                {

                    //rwlock.EnterWriteLock ( );

                    listNoDCSProfileIndicies = new List<int> ( );
                    for ( int i = 0; i < this.Profiles; i++ )
                        if ( !DiveProfiles [ i ].IsAnyDCS )
                            listNoDCSProfileIndicies.Add ( i );

                    //rwlock.ExitWriteLock ( );

                }

                //rwlock.ExitUpgradeableReadLock ( );

                return listNoDCSProfileIndicies;

            }

        }

        #endregion Decompression sickness occurrence specific methods and properties

        #region PSI methods and properties

        /// <summary>
        /// Reports the number of exposures with mild PSI DCS
        /// </summary>
        /// <returns>number of mild DCS exposures</returns>
        public int PSIMildDCS ( )
        {
            if ( !Profile<N>.UsePSI )
            {
                throw new DCSException ( "You attempted to use PSI data when none exists in Decompression.DiveData.PSIMildDCS" );
            }

            int i = 0;
            foreach ( Profile<N> p in DiveProfiles )
            {
                if ( p.PSIMildDCS )
                    i += ( int ) p.Divers;
            }
            return i;
        }

        /// <summary>
        /// Reports the number of exposures with PSI serious DCS
        /// </summary>
        /// <returns>number of serious DCS exposures</returns>
        public int PSISeriousDCS ( )
        {
            if ( !Profile<N>.UsePSI )
                throw new DCSException ( "You attempted to use PSI data when none exists in Decompression.DiveData.PSISeriousDCS" );

            int i = 0;
            foreach ( Profile<N> p in DiveProfiles )
            {
                if ( p.PSISeriousDCS )
                    i += ( int ) p.Divers;
            }
            return i;
        }

        /// <summary>
        /// Reports the number of exposures with Full DCS
        /// </summary>
        /// <returns>number of full DCS exposures</returns>
        public int PSIFullDCS ( )
        {
            if ( !Profile<N>.UsePSI )
                throw new DCSException ( "You attempted to use PSI data when none exists in Decompression.DiveData.PSISeriousDCS" );

            int i = 0;
            foreach ( Profile<N> p in DiveProfiles )
            {
                if ( p.IsFullDCS )
                    i += ( int ) p.Divers;
            }
            return i;
        }

        public List<int> PSIMildDCSProfileIndicies
        {
            get
            {

                if ( !Profile<N>.UsePSI )
                    throw new DCSException ( "Attempt to call Decompression.DiveData<N,P>.PSIMildDCSProfileIndicies without setting Profile<N>.UsePSI" );

                //var rwlock = new ReaderWriterLockSlim ( );

                //rwlock.EnterUpgradeableReadLock ( );

                if ( listPSIMildDCSProfileIndicies == null )
                {

                    //rwlock.EnterWriteLock ( );

                    listPSIMildDCSProfileIndicies = new List<int> ( );
                    for ( int i = 0; i < this.Profiles; i++ )
                        if ( DiveProfiles [ i ].PSIMildDCS )
                            listPSIMildDCSProfileIndicies.Add ( i );

                    //rwlock.ExitWriteLock ( );

                }

                //rwlock.ExitUpgradeableReadLock ( );

                return listPSIMildDCSProfileIndicies;

            }
        }

        public List<int> PSISeriousDCSProfileIndicies
        {
            get
            {

                if ( !Profile<N>.UsePSI )
                    throw new DCSException ( "Attempt to call Decompression.DiveData<N,P>.PSISeriousDCSProfileIndicies without setting Profile<N>.UsePSI" );

                //var rwlock = new ReaderWriterLockSlim ( );

                //rwlock.EnterUpgradeableReadLock ( );

                if ( listPSISeriousDCSProfileIndices == null )
                {

                    //rwlock.EnterWriteLock ( );

                    listPSISeriousDCSProfileIndices = new List<int> ( );
                    for ( int i = 0; i < this.Profiles; i++ )
                        if ( DiveProfiles [ i ].PSISeriousDCS )
                            listPSISeriousDCSProfileIndices.Add ( i );

                    //rwlock.ExitWriteLock ( );

                }

                //rwlock.ExitUpgradeableReadLock ( );

                return listPSISeriousDCSProfileIndices;

            }
        }

        #endregion PSI methods and properties

        #region Data specific and profile specific methods and properties

        public void RemoveShallowerThan (double depth )
        {

            for ( int i = DiveProfiles.Length - 1; i >= 0; i-- )
                if ( DiveProfiles [ i ].MaximumDepth < depth)
                    DiveProfiles.RemoveAt ( i );
            
        }

        public void RemoveDeeperThan ( double depth )
        {

            for ( int i = DiveProfiles.Length - 1; i >= 0; i-- )
                if ( DiveProfiles [ i ].MaximumDepth > depth )
                    DiveProfiles.RemoveAt ( i );

        }

        public void KeepDepthsBetween ( double shallow, double deep )
        {

            RemoveShallowerThan ( shallow );
            RemoveDeeperThan ( deep );

        }

        public void PrintProfilesWithDepthsBetween ( double shallow, double deep )
        {

            var of = new SaveFileDialog ( );
            of.Title = @"Save ANMRI Format Profiles";
            if ( of.ShowDialog ( ) != DialogResult.OK )
                return;

            var fs = new FileStream ( of.FileName, FileMode.Create );
            var sw = new StreamWriter ( fs );

            KeepDepthsBetween(shallow, deep);

            foreach ( var p in DiveProfiles )
            {
                var s = p.GetANMRIProfile ( );
                sw.Write ( s );
            }

            sw.Close ( );
            fs.Close ( );

        }
        
        /// <summary>
        /// Get the total number of exposures
        /// </summary>
        /// <returns>number of exposures</returns>
        public int Exposures ( )
        {
            int i = 0;
            foreach ( Profile<N> p in DiveProfiles )
            {
                i += ( int ) p.Divers;
            }
            return i;
        }

        /// <summary>
        /// Get the file name
        /// </summary>
        public string FileName { set { sFileName = value; } get { return sFileName; } }

        /// <summary>
        /// Get the data directory
        /// </summary>
        public string Directory { set { sDirectory = value; } get { return sFileName.Substring ( 0, sFileName.LastIndexOf ( "\\" ) ); } }

        /// <summary>
        /// Header column print method for PSI work
        /// </summary>
        public void PrintFlags ( )
        {
            // create an open file dialog
            FileStream fs = new FileStream ( "flags.csv", FileMode.Create );
            StreamWriter sw = new StreamWriter ( fs );
            sw.WriteLine ( @"DCS flag, PSI code, PSI meaning, Severity code, Severity meaning, Exposures" );
            foreach ( Profile<N> p in this.DiveProfiles )
            {
                string s = p.FlagsString ( );
                sw.WriteLine ( s );
            }
            sw.Close ( );
            fs.Close ( );
        }

        /// <summary>
        /// For debugging - check for profiles with an end node below the surface
        /// </summary>
        /// <returns>number of profiles with non-surface ending nodes</returns>
        public int NumberOfNonSurfaceDives ( )
        {
            int n = new int ( );
            for ( int i = 0; i < this.Profiles; i++ )
            {
                Profile<N> p = DiveProfiles [ i ];
                if ( ( p.Node ( p.Nodes - 1 ) as Node ).Depth != 0.0 )
                {
                    n++;
                    string s = "File = "
                        + p.FileName
                        + ", profile = "
                        + p.ProfileNumber.ToString ( )
                        + ", depth = "
                        + p.MaximumDepth.ToString ( )
                        + ", dcs = "
                        + p.DCS.ToString ( )
                        + ", divers = "
                        + p.Divers.ToString ( )
                        + ", t1 = "
                        + p.Time1.ToString ( )
                        + ", t2 = "
                        + p.Time2.ToString ( )
                        + ", last time = "
                        + p.LastSurfaceTime.ToString ( );
                    System.Windows.Forms.MessageBox.Show ( s, "Information" );
                }
            }

            return n;
        }

        /// <summary>
        /// Fixer for Big292 profiles with last surface time exceptions
        /// </summary>
        /// <param name="p">profile</param>
        /// <returns>last surface time</returns>
        public double LastSurfaceTime ( Profile<N> p )
        {
            if ( p.FileName == "asatnsm.dat" && p.ProfileNumber == 34 )
                return 2882.9; // inferred from onset times - checked 05/09/2007
            if ( p.FileName == "asatnsm.dat" && p.ProfileNumber == 36 )
                return 2882.0; // inferred from onset times - checked 05/09/2007
            if ( p.FileName == "asatnsm.dat" && p.ProfileNumber == 37 )
                return 6528.0; // inferred from onset times - checked 05/09/2007
            if ( p.FileName == "edu184.dat" && p.ProfileNumber == 21 )
                return 92.9;   // inferred from onset times - checked 05/09/2007
            if ( p.FileName == "edu184.dat" && p.ProfileNumber == 35 )
                return 351.3;  // inferred from onset times - checked 05/09/2007
            if ( p.FileName == "edu885a.dat" && p.ProfileNumber == 13 )
                return 354.5;  // cannot be inferred from onset times - use edu885a.dat profile number 14 - checked 05/09/2007
            if ( p.FileName == "edu885ar.dat" && p.ProfileNumber == 2 )
                return 471.9;  // inferred from onset times - checked 05/09/2007

            double dLastTime = ( p.Node ( p.Nodes - 1 ) as Node ).Time;

            if ( ( p.Node ( p.Nodes - 1 ) as Node ).Depth > 0.1 )
                return dLastTime;

            for ( int j = p.Nodes - 1; j >= 0; j-- )
            {
                if ( ( p.Node ( j ) as Node ).Depth < 0.1 )
                    dLastTime = ( p.Node ( j ) as Node ).Time;
                else
                    break;
            }
            return dLastTime;
        }

        /// <summary>
        /// Reports the last time the diver goes shallower than the indicated depth in the profile
        /// </summary>
        /// <param name="i">profile index</param>
        /// <param name="dDepth">depth of interest</param>
        /// <returns>pass time</returns>
        public double LastSurfaceDepthTime ( int i, double dDepth )
        {
            // find the last surface time
            Profile<N> p = DiveProfiles [ i ];
            double dLastTime = ( p.Node ( p.Nodes - 1 ) as Node ).Time;
            for ( int j = p.Nodes - 1; j >= 0; j-- )
            {
                if ( ( p.Node ( j ) as Node ).Depth < dDepth )
                    dLastTime = ( p.Node ( j ) as Node ).Time;
                else
                    break;
            }
            return dLastTime;
        }

        /// <summary>
        /// Reports the first surface time in the profile
        /// </summary>
        /// <param name="i">profile index</param>
        /// <returns>time of first surface</returns>
        public double FirstSurfaceTime ( int i )
        {
            // find the first surface time
            Profile<N> p = DiveProfiles [ i ];
            double dFirstTime = ( p.Node ( 0 ) as Node ).Time;
            double dMaxDepth = ( p.Node ( 0 ) as Node ).Depth;
            for ( int j = 1; j < p.Nodes - 1; j++ )
            {
                Node n = p.Node ( j ) as Node;
                if ( n.Depth >= dMaxDepth )
                {
                    dMaxDepth = n.Depth;
                    dFirstTime = n.Time;
                }
                else if ( n.Depth > 0.1 )
                    dFirstTime = n.Time;
                else if ( n.Depth < 0.1 && dMaxDepth > 10.0 )
                {
                    dFirstTime = n.Time;
                    return dFirstTime;
                }
                else
                {
                    throw new DCSException ( "Unknown state in Decompression.DiveData.FirstSurfaceTime" );
                }
            }

            return dFirstTime;
        }

        /// <summary>
        /// Reports the ending integration time limit - last surface time + 24(48)hrs for bounce(saturation) dives
        /// </summary>
        /// <param name="i">profile index</param>
        /// <returns>time of integtration limit</returns>
        public double IntegrationLimit ( int i )
        {
            Profile<N> p = DiveProfiles [ i ];
            double dLastTime = p.LastSurfaceTime;

            if ( p.FileName.Equals ( "asatedu.dat" )
                || p.FileName.Equals ( "asatnsm.dat" )
                || p.FileName.Equals ( "asatnmr.dat" ) )
            {
                // this is a saturation dive
                dLastTime += 48.0 * 60.0;
            }
            else
            {
                // this is a bounce dive
                dLastTime += 24.0 * 60.0;
            }
            return dLastTime;
        }

        /// <summary>
        /// For debugging - throws a DCSException if a T1 time is before the first decompression start time
        /// </summary>
        public void CheckForBadIntegrationStartTime ( )
        {
            for ( int i = 0; i < this.Profiles; i++ )
            {
                Profile<N> p = DiveProfiles [ i ];
                if ( p.GoodTimes )
                {
                    if ( p.Time1 < p.FirstDecompressionStartTime )
                        throw new DCSException ( "Bad integration limit found in Decompression.DiveData.CheckForBadIntegrationStartTime" );
                }
            }
            System.Windows.Forms.MessageBox.Show ( "No bad integratoin limits found" );
        }

        /// <summary>
        /// For debugging - throws a DCSException if there are DCS profiles with more than one diver
        /// </summary>
        public void CheckNumberOfDivers ( )
        {
            // check the full dcs cases
            for ( int i = 0; i < this.Profiles; i++ )
            {
                Profile<N> p = DiveProfiles [ i ];
                if ( p.IsFullDCS )
                {
                    if ( p.Divers != 1 )
                    {
                        double dcs = p.DCS;
                        double dvr = p.Divers;
                        double t0  = p.FirstDecompressionStartTime;
                        bool bt    = p.GoodTimes;
                        double t1  = p.Time1;
                        double t2  = p.Time2;
                        string s   = p.FileName;
                        int n      = p.ProfileNumber;
                        throw new DCSException ( "More than one diver with full DCS found in a single profle" );
                    }
                }
            }

            // check the marginal dcs cases
            for ( int i = 0; i < this.Profiles; i++ )
            {
                Profile<N> p = DiveProfiles [ i ];
                if ( p.IsMarginalDCS )
                {
                    if ( p.Divers != 1 )
                    {
                        double dcs = p.DCS;
                        double dvr = p.Divers;
                        double t0  = p.FirstDecompressionStartTime;
                        bool bt    = p.GoodTimes;
                        double t1  = p.Time1;
                        double t2  = p.Time2;
                        string s   = p.FileName;
                        int n      = p.ProfileNumber;
                        throw new DCSException ( "More than one diver with marginal DCS found in a single profile" );
                    }
                }
            }

            System.Windows.Forms.MessageBox.Show ( "Number of divers check completed" );
        }

        /// <summary>
        /// Reports the start time of the last decompression in the profile
        /// </summary>
        /// <param name="i">profile index</param>
        /// <returns>time of last decompression start</returns>
        public double LastDecompressionStartTime ( int i )
        {
            // find that last surface time and depth

            Profile<N> p = DiveProfiles [ i ];

            double dLastTime = p.Node ( p.Nodes - 1 ).Time;
            double dMaxDepth = p.Node ( p.Nodes - 1 ).Depth;

            for ( int j = p.Nodes - 1; j >= 0; j-- )
            {
                Node n = p.Node ( j );

                if ( n.Depth > dMaxDepth + 2.0 )
                {
                    dLastTime = n.Time;
                    dMaxDepth = n.Depth;
                }
                if ( n.Depth < dMaxDepth - 20.0 )
                    break;
            }
            return dLastTime;
        }

        /// <summary>
        /// Reports the decompression start time for the most recent decompression before the indicated time
        /// </summary>
        /// <param name="i">profile index</param>
        /// <param name="dTime">time of interest</param>
        /// <returns>time of decompression start</returns>
        public double MostRecentDecompressionStartTime ( int i, double dTime )
        {
            // find that most recent surface time and depth

            Profile<N> p = DiveProfiles [ i ];

            double dLastTime = p.Node ( p.Nodes - 1 ).Time;
            double dMaxDepth = p.Node ( p.Nodes - 1 ).Depth;

            for ( int j = p.Nodes - 1; j >= 0; j-- )
            {
                Node n = p.Node ( j ) as Node;

                if ( n.Time > dTime )
                    continue;
                if ( n.Depth > dMaxDepth + 2.0 )
                {
                    dLastTime = n.Time;
                    dMaxDepth = n.Depth;
                }
                if ( n.Depth < dMaxDepth - 20.0 )
                    break;
            }
            return dLastTime;
        }

        /// <summary>
        /// Gets the maximum pressure for all profiles in the collection
        /// </summary>
        public double MaximumPressure
        {
            get
            {
                double dMaximumPressure = new double ( );
                for ( int i = 0; i < this.Profiles; i++ )
                {
                    if ( DiveProfiles [ i ].MaximumPressure > dMaximumPressure )
                        dMaximumPressure = DiveProfiles [ i ].MaximumPressure;
                }
                return dMaximumPressure;
            }
        }

        /// <summary>
        /// Gets the maximum depth for all profiles in the collection
        /// </summary>
        public double MaximumDepth
        {
            get
            {
                double dMaximumDepth = new double ( );
                for ( int i = 0; i < this.Profiles; i++ )
                {
                    if ( DiveProfiles [ i ].MaximumDepth > dMaximumDepth )
                        dMaximumDepth = DiveProfiles [ i ].MaximumDepth;
                }
                return dMaximumDepth;
            }
        }

        /// <summary>
        /// Generates an Occurance Density Function for the dive trial data.
        /// The histogram boundaries are available in public static double[] ODFHistogramBounds.
        /// </summary>
        /// <returns>1D array of the histogram values</returns>
        public double [ ] ODFObservaton ( )
        {

            if ( ODFHistogramBins == null )
                throw new DCSException ( "Histogram bounds not set in Decompression.DiveData<P,N>.ODFObservation" );

            Histogram histObservation = new Histogram ( ODFHistogramBins );
            histObservation.Clear ( );

            for ( int i=0; i < this.Profiles; i++ )
            {

                Profile<N> p = DiveProfiles [ i ];

                if ( p.IsAnyDCS )
                {
                    double dOffsetTime = p.LastSurfaceTime;
                    if ( p.GoodTimes )
                    {
                        double dT1 = p.Time1 - dOffsetTime;
                        double dT2 = p.Time2 - dOffsetTime;
                        ODFObservationIncrement ( histObservation, p, dT1, dT2, dOffsetTime );
                    }
                    else
                    {
                        double dT1 = p.FirstDecompressionStartTime - dOffsetTime;
                        double dT2 = p.FinalNode.Time - dOffsetTime;
                        ODFObservationIncrement ( histObservation, p, dT1, dT2, dOffsetTime );
                    }
                }
            }

            double[] dvObservation = new double [ ODFHistogramBins.Length - 1 ];
            for ( int i = 0; i < ODFHistogramBins.Length - 1; i++ )
            {
                dvObservation [ i ] = histObservation.Bins [ i ].Value;
            }

            return dvObservation;

        }

        private double ModeWeight ( double binleft, double binright, double dT1, double dT2)
        {

            // window is before the mode
            if ( dT2 <= binleft + DCSUtilities.Constants.Epsilon )
                return 0.0;

            // window is after the mode
            if ( dT1 >= binright - DCSUtilities.Constants.Epsilon )
                return 0.0;

            // the window is contained withing the mode
            if ( dT1 >= ( binleft - DCSUtilities.Constants.Epsilon ) && dT2 <= ( binright + DCSUtilities.Constants.Epsilon ) )
                return 1.0;

            // the window is wider than the mode
            if ( dT1 <= ( binleft + DCSUtilities.Constants.Epsilon ) && dT2 >= ( binright - DCSUtilities.Constants.Epsilon ) )
                return ( binright - binleft ) / ( dT2 - dT1 );

            // the window straddles the left boundary
            if ( dT1 <= ( binleft + DCSUtilities.Constants.Epsilon ) && dT2 <= ( binright + DCSUtilities.Constants.Epsilon ) )
                return ( dT2 - binleft ) / ( dT2 - dT1 );

            // the window straddles the right boundary
            if ( dT1 >= ( binleft + DCSUtilities.Constants.Epsilon ) && dT2 >= ( binright - DCSUtilities.Constants.Epsilon ) )
                return ( binright - dT1 ) / ( dT2 - dT1 );


            return -1.0;

        }

        private void ODFObservationIncrement ( Histogram histObservation, Profile<N> p, double dT1, double dT2, double surfacetime )
        {

            var modes = new double[4];
            modes [ 0 ] = ModeWeight ( -60.0 , 60.0 , dT1 , dT2 );
            modes [ 1 ] = ModeWeight ( 60.0 , 180.0 , dT1 , dT2 );
            modes [ 2 ] = dT1;
            modes [ 3 ] = dT2;
            p.Tag = modes as Object;
               
            if ( histObservation.FindBin ( dT1 ).LowerBound == histObservation.FindBin ( dT2 ).LowerBound )
            {

                double dWeight = p.DCS * p.Divers; // ( histObservation.FindBin ( dT1 ).Width / 60.0 );
                histObservation.Increment ( dT1, dWeight );

                //var lowerBound = histObservation.FindBin ( dT1 ).LowerBound;

                //if ( lowerBound >= -60.0 - DCSUtilities.Constants.Epsilon &&
                //    lowerBound <=   60.0 + DCSUtilities.Constants.Epsilon )
                //    modes[0] = dWeight/p.Divers;

                //if ( lowerBound >=  60.0 - DCSUtilities.Constants.Epsilon &&
                //    lowerBound <=   180.0 + DCSUtilities.Constants.Epsilon )
                //    modes [ 1 ] = dWeight / p.Divers;

                //p.Tag = modes as Object;
                
            }
            else
            {

                // how many bins does this stride?
                // int iBins = histObservation.FindBin ( dT2 ).Index - histObservation.FindBin ( dT1 ).Index;
                // increment the first partial bin
                double dFraction = ( histObservation.FindBin ( dT1 ).UpperBound - dT1 ) / ( dT2 - dT1 );
                double dWeight   = p.DCS * p.Divers * dFraction; // ( histObservation.FindBin ( dT1 ).Width / 60.0 );
                histObservation.Increment ( dT1, dWeight );

                //var lowerBound = histObservation.FindBin ( dT1 ).LowerBound;

                //if ( lowerBound >= -60.0 - DCSUtilities.Constants.Epsilon &&
                //    lowerBound <= 60.0 + DCSUtilities.Constants.Epsilon )
                //    modes [ 0 ] = dWeight / p.Divers;

                //if ( lowerBound >= 60.0 - DCSUtilities.Constants.Epsilon &&
                //    lowerBound <= 180.0 + DCSUtilities.Constants.Epsilon )
                //    modes [ 1 ] = dWeight / p.Divers;

                //p.Tag = modes as Object;


                // increment the full bins
                for ( int b=histObservation.FindBin ( dT1 ).Index + 1; b < histObservation.FindBin ( dT2 ).Index; b++ )
                {
                    HistogramBin bin = histObservation.Bins [ b ];
                    dFraction = ( bin.UpperBound - bin.LowerBound ) / ( dT2 - dT1 );
                    dWeight = p.DCS * p.Divers * dFraction; // ( bin.Width / 60.0 );
                    histObservation.Increment ( bin.LowerBound + 0.5 * ( bin.UpperBound - bin.LowerBound ), dWeight );

                    // var lowerBound = histObservation.FindBin ( dT1 ).LowerBound;

                    //if ( lowerBound >= -60.0 - DCSUtilities.Constants.Epsilon &&
                    //    lowerBound <= 60.0 + DCSUtilities.Constants.Epsilon )
                    //    modes [ 0 ] = dWeight / p.Divers;

                    //if ( lowerBound >= 60.0 - DCSUtilities.Constants.Epsilon &&
                    //    lowerBound <= 180.0 + DCSUtilities.Constants.Epsilon )
                    //    modes [ 1 ] = dWeight / p.Divers;

                    //p.Tag = modes as Object;
                    
                }
                // increment the last partial bin
                dFraction = ( dT2 - histObservation.FindBin ( dT2 ).LowerBound ) / ( dT2 - dT1 );
                dWeight = p.DCS * p.Divers * dFraction; // ( histObservation.FindBin ( dT2 ).Width / 60 );
                histObservation.Increment ( dT2, dWeight );

                // var lowerBound = histObservation.FindBin ( dT1 ).LowerBound;

                //if ( lowerBound >= -60.0 - DCSUtilities.Constants.Epsilon &&
                //    lowerBound <= 60.0 + DCSUtilities.Constants.Epsilon )
                //    modes [ 0 ] = dWeight / p.Divers;

                //if ( lowerBound >= 60.0 - DCSUtilities.Constants.Epsilon &&
                //    lowerBound <= 180.0 + DCSUtilities.Constants.Epsilon )
                //    modes [ 1 ] = dWeight / p.Divers;

                //p.Tag = modes as Object;

            }
        }

        /// <summary>
        /// Set the bins to use for generateing an ODF of the dive data
        /// </summary>
        /// <param name="w">Bin width in minutes</param>
        public void ODFObservationSetHistogramBounds ( double w )
        {

            ODFHistogramStride = w;

            var bounds = new List<double> ( );

            for ( var n = ODFHistogramLowerBound ; n <= ODFHistogramUpperBound + DCSUtilities.Constants.Epsilon ; n += ODFHistogramStride )
                bounds.Add ( n );

            ODFHistogramBins = bounds.ToArray ( );

        }

        public void GenerateNodeDump ( int i )
        {

            if ( i >= DiveProfiles.Length )
                throw new DCSException("Array size exceeded in Decompression.DiveData.GenerateNodeDump");

            // Open a node dump file.
            var sf1 = new SaveFileDialog ( );
            sf1.Title = @"Save Node Dump";
            if ( sf1.ShowDialog ( ) != DialogResult.OK )
                return;

            var fs1 = new FileStream ( sf1.FileName, FileMode.Create );
            var sw1 = new StreamWriter ( fs1 );

            var p = DiveProfiles [ i ];
            sw1.WriteLine ( NodeTissue.HeaderString ( ) );
            for ( var n = 0; n < p.Nodes; n++ )
                sw1.WriteLine ( ( p.Node ( n ) as NodeTissue ).ToString ( ) );

            sw1.Close ( );
            fs1.Close ( );
            
        }

        #endregion Data specific and profile specific methods and properties

    }

}