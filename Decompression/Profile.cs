using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DCSUtilities;

namespace Decompression
{
    /// <summary>
    /// dive profile class
    /// </summary>
    /// <typeparam name="N">generic node class</typeparam>
    public class Profile<N>
        where N : Node
    {
        /// <summary>
        /// Floating point tolerance for testing whether to nodes occur at the same time
        /// </summary>
        public const double TimeTolerance        = 1.0e-3;
        public const double LargeSurfaceInterval = 360.0;

        public static PSISEVERITYSPLITTING SplittingType = PSISEVERITYSPLITTING.UNKNOWN;

        /// <summary>
        /// Generic node collection
        /// </summary>
        protected NodeList<N> NodeNet = new NodeList<N> ( );

        private double dMaxDepth           = new double ( );
        private double dBottomTime         = new double ( );
        private double dAscentTime         = new double ( );
        private string sSetName            = new string ( string.Empty.ToCharArray ( ) );
        private double dOriginatingGas     = new double ( );
        private double dNumDivers          = new double ( );
        private double dDCS                = new double ( );
        private double dTimeOne            = new double ( );
        private double dTimeTwo            = new double ( );
        private double dFinalPDCS          = new double ( );
        private double dEndRiskTime        = new double ( );
        private double [ , ] dmPDCS        = null;
        private bool bFullDCS              = new bool ( );
        private bool bMarginalDCS          = new bool ( );
        private bool bGoodT12              = new bool ( );
        private string sLongFileName       = new string ( string.Empty.ToCharArray ( ) );
        private string sShortFileName      = new string ( string.Empty.ToCharArray ( ) );
        private PSI bPSI                   = PSI.UNKNOWN;
        private static int iProfileCounter = 0;
        private static bool bUsePSI        = new bool ( );
        private static bool bTetranomial   = new bool ( );
        private int iProfileNumber         = new int ( );
        private int iT1NodeIndex           = new int ( );
        private int iT2NodeIndex           = new int ( );
        public object Tag                  = null;
        private StringCollection ANMRIProf = new StringCollection ( );

        #region Profile constructors and profile generation methods and properties

        /// <summary>
        /// default constructor used in catching exceptions
        /// </summary>
        public Profile ( )
        {
            dMaxDepth       = 0.0;
            dBottomTime     = 0.0;
            dAscentTime     = 0.0;
            sSetName        = string.Empty;
            dOriginatingGas = 0.0;
            dNumDivers      = 0.0;
            dDCS            = 0.0;
            dTimeOne        = 0.0;
            dTimeTwo        = 0.0;
            bFullDCS        = false;
            bMarginalDCS    = false;
            bGoodT12        = false;
            bUsePSI         = false;
            bTetranomial    = false;
            sLongFileName   = string.Empty;
            sShortFileName  = string.Empty;
            bPSI            = PSI.UNKNOWN;
            iProfileNumber  = 0;
        }

        /// <summary>
        /// dive profile constructor
        /// </summary>
        /// <param name="s">first header string from NMRI dive profile</param>
        /// <param name="sFile">data file name</param>
        public Profile ( string s, string sFile )
        {
            ANMRIProf.Add ( s + Environment.NewLine );
            ANMRIProf.Add ( "-9999.0" + Environment.NewLine );

            // store the file name
            if ( sFile == string.Empty )
            {
                sLongFileName = string.Empty;
                sShortFileName = string.Empty;
            }
            else
            {
                sLongFileName = sFile;
                sShortFileName = sLongFileName.Substring ( sLongFileName.LastIndexOf ( char.Parse ( @"\" ) ) + 1,
                    sLongFileName.Length - sLongFileName.LastIndexOf ( char.Parse ( @"\" ) ) - 1 );
            }

            // parse the header1 string
            string [ ] sList = s.Split ( new char [ ] { ',' } );

            // assign the private data members
            if ( sList.Length > 3 )
            {
                dMaxDepth = double.Parse ( sList [ 0 ].Trim ( ) );
                dBottomTime = double.Parse ( sList [ 1 ].Trim ( ) );
                try
                {
                    dAscentTime = double.Parse ( sList [ 2 ].Trim ( ) );
                }
                catch ( Exception e )
                {
#warning temporary change to get NMRI 98 files working
                    dAscentTime = 0.0;

                    // string str = "Exception in Decompression.Profile.Profile (" + e.ToString () + ")";
                    // throw new DCSException ( str );
                }
                sSetName        = sList [ 3 ];
                dOriginatingGas = 1.0;
                dNumDivers      = 0.0;
                dDCS            = 0.0;
                dTimeOne        = 0.0;
                dTimeTwo        = 0.0;
                bFullDCS        = false;
                bMarginalDCS    = false;
                bGoodT12        = false;
                bPSI            = PSI.UNKNOWN;
                iProfileNumber  = ++iProfileCounter;
            }
            else
            {
                dMaxDepth       = 0.0;
                dBottomTime     = 0.0;
                dAscentTime     = 0.0;
                sSetName        = string.Empty;
                dOriginatingGas = 1.0;
                dNumDivers      = 0.0;
                dDCS            = 0.0;
                dTimeOne        = 0.0;
                dTimeTwo        = 0.0;
                bFullDCS        = false;
                bMarginalDCS    = false;
                bGoodT12        = false;
                bPSI            = PSI.UNKNOWN;

                // throw new DCSException("Attempt to create a DiveProfile object with a bad argument in Decompression.DiveProfile.DiveProfile");
            }
        }

        /// <summary>
        /// clear any preexisting dive nodes
        /// </summary>
        public void Clear ( )
        {
            iProfileCounter = 0;
            NodeNet.Clear ( );
        }

        /// <summary>
        /// add the header information
        /// </summary>
        /// <param name="s">string containing header (second line) information</param>
        public void AddHeader ( string s )
        {
            string [ ] sList = s.Split ( new char [ ] { ',' } );

            ANMRIProf.Insert ( ANMRIProf.Count - 1, s + Environment.NewLine );

            dOriginatingGas             = double.Parse ( sList [ 0 ].Trim ( ) );
            Decompression.Node.dLastGas = dOriginatingGas;
            dNumDivers                  = double.Parse ( sList [ 1 ].Trim ( ) );
            dDCS                        = double.Parse ( sList [ 2 ].Trim ( ) );

            if (  dDCS > 1.0 || dDCS < 0.0 )
                throw new DCSException ( "DCS > 1.0 or DCS < 0.0 found in Decompression.DiveProfile.AddHeader" );

            if ( dDCS > 0.01 )
            {
                if ( bUsePSI )
                {
                    if ( sList.Length == 3 )
                    {
                        throw new DCSException ( "Too few arguments when using PSI in Decompression.DiveProfile.AddHeater" );
                    }
                    else if ( sList.Length == 4 )
                    {
                        // dcs with psi, no times
                        dTimeOne = 0.0;
                        dTimeTwo = 0.0;
                        bPSI = PSIType ( sList [ 3 ].Trim ( ) );
                        bGoodT12 = false;
                    }
                    else
                    {
                        // dcs with times and psi
                        dTimeOne = double.Parse ( sList [ 3 ].Trim ( ) );
                        dTimeTwo = double.Parse ( sList [ 4 ].Trim ( ) );
                        bPSI = PSIType ( sList [ 5 ].Trim ( ) );
                        if ( Math.Abs ( dTimeTwo - dTimeOne ) > DCSUtilities.Constants.Epsilon )
                            bGoodT12 = true;
                    }
                }
                else
                {
                    if ( sList.Length > 4 )
                    {
                        dTimeOne = double.Parse ( sList [ 3 ].Trim ( ) );
                        dTimeTwo = double.Parse ( sList [ 4 ].Trim ( ) );
                        if ( Math.Abs ( dTimeTwo - dTimeOne ) > DCSUtilities.Constants.Epsilon )
                            bGoodT12 = true;
                    }
                    else
                    {
                        dTimeOne = 0.0;
                        dTimeTwo = 0.0;
                        bGoodT12 = false;
                    }
                }

                // set the dcs severity code
                if ( dDCS < 0.6 )
                {
                    bMarginalDCS = true;
                    bFullDCS = false;
                }
                else
                {
                    bMarginalDCS = false;
                    bFullDCS = true;
                }
            }
            else // DCS = 0.0
            {
                dTimeOne = 0.0;
                dTimeTwo = 0.0;
                bFullDCS = false;
                bMarginalDCS = false;
                bGoodT12 = false;
                bPSI = PSI.NONE;
            }
        }

        /// <summary>
        /// Add a new node to the collection
        /// </summary>
        /// <param name="s">header string from NMRI format file</param>
        public void AddNode ( string s )
        {
            string [ ] sList = s.Split ( new char [ ] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );

            ANMRIProf.Insert ( ANMRIProf.Count - 1, s + Environment.NewLine );

            if ( sList.Length == 1 && sList [ 0 ].Contains ( '!' ) )
            {
                // this is a comment line - disregard
            }
            else if ( sList.Length == 2 || ( sList.Length == 3 && string.IsNullOrWhiteSpace ( sList [ 2 ] ) ) )
            {
                double dTime = double.Parse ( sList [ 0 ].Trim ( ) );
                double dDepth = double.Parse ( sList [ 1 ].Trim ( ) );

                Type t = typeof ( N );
                System.Reflection.ConstructorInfo c = t.GetConstructor ( new Type [ ] { typeof ( double ), typeof ( double ) } );
                NodeNet.Add ( ( N ) c.Invoke ( new object [ ] { dTime, dDepth } ) );
            }
            else if ( sList.Length == 4 || ( sList.Length == 5 && string.IsNullOrWhiteSpace ( sList [ 4 ] ) ) )
            {
                double dTime = double.Parse ( sList [ 0 ].Trim ( ) );
                double dDepth = double.Parse ( sList [ 1 ].Trim ( ) );
                double dNewGas = double.Parse ( sList [ 2 ].Trim ( ) );
                double dSwitchTime = double.Parse ( sList [ 3 ].Trim ( ) );

                Type t = typeof ( N );
                System.Reflection.ConstructorInfo c = t.GetConstructor ( new Type [ ] { typeof ( double ), typeof ( double ), typeof ( double ), typeof ( double ) } );
                NodeNet.Add ( ( N ) c.Invoke ( new object [ ] { dTime, dDepth, dNewGas, dSwitchTime } ) );
            }
            else if ( sList.Length == 5 )
            {
                double dTime       = double.Parse ( sList [ 0 ].Trim ( ) );
                double dDepth      = double.Parse ( sList [ 1 ].Trim ( ) );
                double dNewGas     = double.Parse ( sList [ 2 ].Trim ( ) );
                double dSwitchTime = double.Parse ( sList [ 3 ].Trim ( ) );
                double dExercise   = double.Parse ( sList [ 4 ].Trim ( ) );

                Type t = typeof ( N );
                System.Reflection.ConstructorInfo c = t.GetConstructor ( new Type [ ] { typeof ( double ), typeof ( double ), typeof ( double ), typeof ( double ), typeof ( double ) } );
                NodeNet.Add ( ( N ) c.Invoke ( new object [ ] { dTime, dDepth, dNewGas, dSwitchTime, dExercise } ) );
            }
            else
            {
                throw new DCSException ( "Invalid number of string arguments passed to Decompression.DiveProfile.AddNode" );
            }
        }

        /// <summary>
        /// Inserts a new node in the dive profile
        /// </summary>
        /// <param name="i">insertion index</param>
        /// <param name="time">new node time</param>
        /// <param name="depth">new node depth</param>
        public void InsertNode ( int i, double time, double depth )
        {
            Type t = typeof ( N );
            System.Reflection.ConstructorInfo c = t.GetConstructor ( new Type [ ] { typeof ( double ), typeof ( double ) } );
            NodeNet.Insert ( i, ( N ) c.Invoke ( new object [ ] { time, depth } ) );
        }

        /// <summary>
        /// set the profile counter to zero
        /// </summary>
        static public void ZeroProfileCounter ( )
        {
            iProfileCounter = 0;
        }

        /// <summary>
        /// return generic node type by index
        /// </summary>
        /// <param name="i">node index</param>
        /// <returns>generic node</returns>
        public N Node ( int i )
        {
            return NodeNet [ i ];
        }

        #endregion Profile constructors and profile generation methods and properties

        #region Profile information methods and properties

        public double [ , ] DCSProbabilityFunction { get { return dmPDCS; } set { dmPDCS = value; } }

        /// <summary>
        /// get/set the final PDCS
        /// </summary>
        public double FinalDCSProbability { get { return dFinalPDCS; } set { dFinalPDCS = value; } }

        /// <summary>
        /// get/set the time of final risk accumulation
        /// </summary>
        public double TimeOfFinalDCSProbability { get { return dEndRiskTime; } set { dEndRiskTime = value; } }

        /// <summary>
        /// get the number of dive nodes in this dive profile
        /// </summary>
        public int Nodes { get { return NodeNet.Length; } }

        /// <summary>
        /// get the maximum depth of this dive profile
        /// </summary>
        public double MaxDepth { get { return dMaxDepth; } }

        /// <summary>
        /// get the bottom time of this dive profile
        /// </summary>
        public double BottomTime { get { return dBottomTime; } }

        /// <summary>
        /// get the ascent time of this dive profile
        /// </summary>
        public double AscentTime { get { return dAscentTime; } }

        /// <summary>
        /// get the dive profile data set name
        /// </summary>
        public string SetName { get { return sSetName; } }

        /// <summary>
        /// get the originating gas for this dive profile
        /// </summary>
        public double OriginatingGas { get { return dOriginatingGas; } }

        /// <summary>
        /// get the number of divers in the this profile
        /// </summary>
        public double Divers { get { return dNumDivers; } }

        /// <summary>
        /// calculates the depth at an arbitraty time - must be overridden by an inheriting class
        /// </summary>
        /// <param name="dTime">ending time (min)</param>
        /// <returns>depth (fsw)</returns>
        public double Depth ( double dTime )
        {
            int c = NodeNet.Length - 1;

            // if the time is negative, return the first pressure
            if ( dTime < 0.0 )
                return ( NodeNet [ 0 ] as Node ).Depth;

            // if the time is greater than the greatest time in the data set, return the last pressure
            if ( dTime >= ( NodeNet [ c ] as Node ).Time )
                return ( NodeNet [ c ] as Node ).Depth;

            // find the most recent node
            for ( int i = NodeNet.Length - 1; i >= 0; i-- )
            {
                c = i;
                if ( ( NodeNet [ i ] as Node ).Time <= dTime )
                    break;
            }

            double dLastTime = ( NodeNet [ c ] as Node ).Time;
            double dNextTime = ( NodeNet [ c + 1 ] as Node ).Time;
            double dLastDepth = ( NodeNet [ c ] as Node ).Depth;
            double dNextDepth = ( NodeNet [ c + 1 ] as Node ).Depth;

            double Depth = dLastDepth
                + ( dTime - dLastTime ) * ( dNextDepth - dLastDepth ) / ( dNextTime - dLastTime );

            return Depth;
        }

        /// <summary>
        /// get the full file name including directory path
        /// </summary>
        public string FullFileName { get { return sLongFileName; } }

        /// <summary>
        /// get the file name
        /// </summary>
        public string FileName { get { return sShortFileName; } }

        /// <summary>
        /// Returns true if this is a saturation dive, false otherwise
        /// </summary>
        public bool IsSaturationDive
        {
            get
            {
                if ( FileName.ToLower ( ).Contains ( "asatare" ) ||   // saturation
                    FileName.ToLower ( ).Contains ( "asatedu" ) ||
                    FileName.ToLower ( ).Contains ( "asatnsm" ) ||
                    FileName.ToLower ( ).Contains ( "asatnmr" ) ||
                    FileName.ToLower ( ).Contains ( "asatdc" ) ||
                    FileName.ToLower ( ).Contains ( "asatfr85" ) ||
                    FileName.ToLower ( ).Contains ( "eduas45" ) ||
                    FileName.ToLower ( ).Contains ( "nmr9209" ) ||
                    FileName.ToLower ( ).Contains ( "edu849s2" ) ||  // sub saturation
                    FileName.ToLower ( ).Contains ( "nsm6hr" ) ||
                    FileName.ToLower ( ).Contains ( "rnplx50" ) ||
                    FileName.ToLower ( ).Contains ( "surexmcorrected" )
                    )
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// get the profile number
        /// </summary>
        public int ProfileNumber { get { return iProfileNumber; } }

        /// <summary>
        /// Search the profile for maximum depth
        /// </summary>
        public double MaximumDepth
        {
            get
            {
                double dMaximumDepth = new double ( );
                for ( int i = 0; i < this.Nodes; i++ )
                {
                    if ( ( NodeNet [ i ] as Node ).Depth > dMaximumDepth )
                        dMaximumDepth = ( NodeNet [ i ] as Node ).Depth;
                }
                return dMaximumDepth;
            }
        }

        /// <summary>
        /// Search the profile for the maximum pressure
        /// </summary>
        public double MaximumPressure
        {
            get
            {
                double dMaximumPressure = new double ( );
                for ( int i = 0; i < this.Nodes; i++ )
                {
                    if ( NodeNet [ i ].Pressure > dMaximumPressure )
                        dMaximumPressure = NodeNet [ i ].Pressure;
                }
                return dMaximumPressure;
            }
        }

        /// <summary>
        /// Find the index of the node just before the specified time
        /// </summary>
        /// <param name="dTime">time of interest</param>
        /// <returns>node index of closest node not more recent that dTime</returns>
        public int MostRecentNodeIndex ( double dTime )
        {
            int iLast = new int ( );
            for ( int i = 0; i < NodeNet.Length; i++ )
            {
                if ( dTime - NodeNet [ i ].Time < -TimeTolerance )
                    break;
                iLast = i;
            }
            return iLast;
        }

        /// <summary>
        /// Get the first decompression start time
        /// </summary>
        public double FirstDecompressionStartTime
        {
            get
            {
                double dFirstTime = NodeNet [ 0 ].Time;
                double dMaxDepth = NodeNet [ 0 ].Depth;

                for ( int j = 1; j < this.Nodes; j++ )
                {
                    Node n = NodeNet [ j ];

                    if ( n.Depth > dMaxDepth - 2.0 )
                    {
                        dFirstTime = n.Time;
                        dMaxDepth = n.Depth;
                    }
                    if ( n.Depth < dMaxDepth - 20.0 )
                        break;
                }
                return dFirstTime;
            }
        }

        /// <summary>
        /// Get the node at the beginning of the first decompression
        /// </summary>
        public N FirstDecompressionNode
        {
            get
            {
                N n = NodeNet [ 0 ];
                double dFirstTime = NodeNet [ 0 ].Time;
                double dMaxDepth = NodeNet [ 0 ].Depth;

                for ( int i = 1; i < this.Nodes; i++ )
                {
                    n = NodeNet [ i ];
                    if ( n.Depth > MaxDepth - 2.0 )
                    {
                        dFirstTime = n.Time;
                        dMaxDepth = n.Depth;
                    }
                    if ( n.Depth < dMaxDepth - 20.0 )
                        break;
                }

                return n;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public N FirstSurfaceNode
        {
            get
            {
                bool bLeaveSurface = new bool ( );

                N n = NodeNet [ 0 ];

                for ( int i = 1; i < this.Nodes; i++ )
                {
                    n = NodeNet [ i ];

                    if ( !bLeaveSurface && n.Depth > 10.0 )
                        bLeaveSurface = true;

                    if ( bLeaveSurface && n.Depth < 1.0 )
                        break;
                }

                return n;
            }
        }

        /// <summary>
        /// get the second bottom arrive node
        /// </summary>
        public N SecondBottomNode
        {
            get
            {
                bool bLeaveSurface = new bool ( );
                bool bReachSurface = new bool ( );

                N n = NodeNet [ 0 ];

                for ( int i = 1; i < this.Nodes - 2; i++ )
                {
                    n = NodeNet [ i ];

                    if ( !bLeaveSurface && n.Depth > 10.0 )
                        bLeaveSurface = true;

                    if ( bLeaveSurface && !bReachSurface && n.Depth < 1.0 )
                        bReachSurface = true;

                    if ( bReachSurface && n.Depth > 10.0 && ( n.Depth >= NodeNet [ i + 1 ].Depth ) )
                        break;
                }

                return n;
            }
        }

        /// <summary>
        /// get the last surface arrive node
        /// </summary>
        public N LastSurfaceNode
        {
            get
            {
                N n = NodeNet [ this.Nodes - 1 ];

                for ( int i = this.Nodes - 1; i > 0; i-- )
                {
                    n = NodeNet [ i ];

                    if ( NodeNet [ i - 1 ].Depth > 1.0 )
                        break;
                }

                return n;
            }
        }

        /// <summary>
        /// get the last leave bottom node
        /// </summary>
        public N LastLeaveBottomNode
        {
            get
            {
                N n = NodeNet [ this.Nodes - 1 ];

                for ( int i = this.Nodes - 1; i > 0; i-- )
                {
                    n = NodeNet [ i ];

                    if ( NodeNet [ i - 1 ].Depth > 1.0 )
                    {
                        n = NodeNet [ i - 1 ];
                        break;
                    }
                }

                return n;
            }
        }

        /// <summary>
        /// get the T1 node
        /// </summary>
        public N Time1Node { get { return NodeNet [ iT1NodeIndex ]; } }

        /// <summary>
        /// get the T2 node
        /// </summary>
        public N Time2Node { get { return NodeNet [ iT2NodeIndex ]; } }

        /// <summary>
        /// get the final node
        /// </summary>
        public N FinalNode { get { return NodeNet [ Nodes - 1 ]; } }

        /// <summary>
        /// Get the last surface time
        /// </summary>
        public double LastSurfaceTime
        {
            get
            {
                if ( this.FileName == "asatnsm.dat" && this.ProfileNumber == 34 )
                    return 2882.9; // inferred from onset times - checked 05/09/2007
                if ( this.FileName == "asatnsm.dat" && this.ProfileNumber == 36 )
                    return 2882.0; // inferred from onset times - checked 05/09/2007
                if ( this.FileName == "asatnsm.dat" && this.ProfileNumber == 37 )
                    return 6528.0; // inferred from onset times - checked 05/09/2007
                if ( this.FileName == "edu184.dat" && this.ProfileNumber == 21 )
                    return 92.9;   // inferred from onset times - checked 05/09/2007
                if ( this.FileName == "edu184.dat" && this.ProfileNumber == 35 )
                    return 351.3;  // inferred from onset times - checked 05/09/2007
                if ( this.FileName == "edu885a.dat" && this.ProfileNumber == 13 )
                    return 354.5;  // cannot be inferred from onset times - use edu885a.dat profile number 14 - checked 05/09/2007
                if ( this.FileName == "edu885ar.dat" && this.ProfileNumber == 2 )
                    return 471.9;  // inferred from onset times - checked 05/09/2007

                double dLastTime = NodeNet [ Nodes - 1 ].Time;

                if ( NodeNet [ Nodes - 1 ].Depth > 0.1 )
                    return dLastTime;

                for ( int j = Nodes - 1; j >= 0; j-- )
                {
                    if ( NodeNet [ j ].Depth < 0.1 )
                        dLastTime = NodeNet [ j ].Time;
                    else
                        break;
                }
                return dLastTime;
            }
        }

        /// <summary>
        /// Calculates the ambient pressure at the specified time
        /// </summary>
        /// <param name="dTime">time (min) at which to calculate the ambient pressure</param>
        /// <returns>ambient pressure (fsw)</returns>
        public double Pressure ( double dTime )
        {
            int c = MostRecentNodeIndex ( dTime );

            if ( c == this.Nodes - 1 )
                return NodeNet [ Nodes - 1 ].Pressure;

            double dLastTime = NodeNet [ c ].Time;
            double dNextTime = NodeNet [ c + 1 ].Time;
            double dLastPressure = NodeNet [ c ].Pressure;
            double dNextPressure = NodeNet [ c + 1 ].Pressure;

            double Pressure = dLastPressure
                + ( dTime - dLastTime ) * ( dNextPressure - dLastPressure ) / ( dNextTime - dLastTime );

            return Pressure;
        }

        /// <summary>
        /// Calculates the N2 pressure at the specified time
        /// </summary>
        /// <param name="dTime">time (min) at which to calculate the N2 pressure</param>
        /// <returns>N2 pressure (fsw)</returns>
        public double N2Pressure ( double dTime )
        {

            if ( dTime <= 0.0 )
                return NodeNet [ 0 ].N2Pressure;

            int c = MostRecentNodeIndex ( dTime );

            if ( c == this.Nodes - 1 )
                return NodeNet [ Nodes - 1 ].N2Pressure;

            double dLastTime       = NodeNet [ c ].Time;
            double dNextTime       = NodeNet [ c + 1 ].Time;
            double dLastN2Pressure = NodeNet [ c ].N2Pressure;
            double dNextN2Pressure = NodeNet [ c + 1 ].N2Pressure;

            double N2Pressure = dLastN2Pressure
                + ( dTime - dLastTime ) * ( dNextN2Pressure - dLastN2Pressure ) / ( dNextTime - dLastTime );

            return N2Pressure;
        }

        /// <summary>
        /// Calculates the O2 pressure at the specified time
        /// </summary>
        /// <param name="dTime">time (min) at which to calculate the O2 pressure</param>
        /// <returns>O2 pressure (fsw)</returns>
        public double O2Pressure ( double dTime )
        {
            int c = MostRecentNodeIndex ( dTime );

            if ( c == this.Nodes - 1 )
                return NodeNet [ Nodes - 1 ].O2Pressure;

            double dLastTime = NodeNet [ c ].Time;
            double dNextTime = NodeNet [ c + 1 ].Time;
            double dLastO2Pressure = NodeNet [ c ].O2Pressure;
            double dNextO2Pressure = NodeNet [ c + 1 ].O2Pressure;

            double O2Pressure = dLastO2Pressure
                + ( dTime - dLastTime ) * ( dNextO2Pressure - dLastO2Pressure ) / ( dNextTime - dLastTime );

            return O2Pressure;
        }

        /// <summary>
        /// Calculates the He pressure at the specified time
        /// </summary>
        /// <param name="dTime">time (min) at which to calculate the He pressure</param>
        /// <returns>He pressure (fsw)</returns>
        public double HePressure ( double dTime )
        {
            int c = MostRecentNodeIndex ( dTime );

            if ( c == this.Nodes - 1 )
                return NodeNet [ Nodes - 1 ].HePressure;

            double dLastTime = NodeNet [ c ].Time;
            double dNextTime = NodeNet [ c + 1 ].Time;
            double dLastHePressure = NodeNet [ c ].HePressure;
            double dNextHePressure = NodeNet [ c + 1 ].HePressure;

            double HePressure = dLastHePressure
                + ( dTime - dLastTime ) * ( dNextHePressure - dLastHePressure ) / ( dNextTime - dLastTime );

            return HePressure;
        }

        /// <summary>
        /// Calculates the pressure rate at the specified time
        /// </summary>
        /// <param name="dTime"></param>
        /// <returns></returns>
        public double N2PressureRate ( double dTime )
        {
            return NodeNet [ MostRecentNodeIndex ( dTime ) ].N2PressureRate;
        }

        public double O2PressureRate ( double dTime )
        {
            return NodeNet [ MostRecentNodeIndex ( dTime ) ].O2PressureRate;
        }

        public double HePressureRate ( double dTime )
        {
            return NodeNet [ MostRecentNodeIndex ( dTime ) ].HePressureRate;
        }

        public int NumberOfDives ( double dDepthThreshold )
        {
            var nDives = 0;

            var lastDepth = 0.0;

            foreach ( Node n in NodeNet )
            {
                if ( lastDepth == 0.0 && n.Depth > lastDepth )
                    nDives++;

                lastDepth = n.Depth;
            }

            return nDives;
        }

        public bool IsNoStopDive ( int iDiveNumber )
        {
            throw new NotImplementedException ( "Decompression.Profile.IsNoStopDive is not yet implemented." );
        }

        #endregion Profile information methods and properties

        #region DCS occurrence methods and properties

        /// <summary>
        /// get the DCS flag for this dive profile
        /// </summary>
        public double DCS
        {
            get
            {
                return dDCS;
            }
            set
            {
                dDCS = value;
                if ( dDCS == 0.0 )
                {
                    bFullDCS = false;
                    bMarginalDCS = false;
                }
                else if ( dDCS == 1.0 )
                {
                    bFullDCS = true;
                    bMarginalDCS = false;
                }
                else
                {
                    bFullDCS = false;
                    bMarginalDCS = true;
                }
            }
        }

        /// <summary>
        /// change marginal DCS events to non events
        /// </summary>
        public void SetMarginalToNoDCS ( )
        {
            this.dDCS = 0.0;
            this.bFullDCS = false;
            this.bMarginalDCS = false;
            this.bGoodT12 = false;
            this.bPSI = PSI.NONE;
        }

        /// <summary>
        /// change full DCS events to non events
        /// </summary>
        public void SetFullToNoDCS ( )
        {
            this.dDCS = 0.0;
            this.bFullDCS = false;
            this.bMarginalDCS = false;
            this.bGoodT12 = false;
            this.bPSI = PSI.NONE;
        }

        /// <summary>
        /// change marginal DCS events to full events
        /// </summary>
        public void SetMarginalToFullDCS ( )
        {
            this.dDCS = 1.0;
            this.bFullDCS = true;
            this.bMarginalDCS = false;
        }

        /// <summary>
        /// changes full DCS events to serious events
        /// </summary>
        public void SetFullToSeriousDCS ( )
        {
            this.dDCS = 1.0;
            this.bFullDCS = true;
            this.bMarginalDCS = false;
            this.PSIIndex = PSI.NEUROLOGICALSERIOUS;
        }

        /// <summary>
        /// change marginal DCS events to mild events
        /// </summary>
        public void SetMarginalToMildDCS ( )
        {
            this.DCS = 1.0;
            this.bFullDCS = true;
            this.bMarginalDCS = false;
            this.PSIIndex = PSI.PAIN;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="d">DCS value </param>
        public void SetMarginalDCS ( double d )
        {
            if ( IsMarginalDCS )
                dDCS = d;
        }

        /// <summary>
        /// bool test for good DCS symptom onset times
        /// </summary>
        public bool GoodTimes { get { return bGoodT12; } }

        /// <summary>
        /// get the last well time for this dive profile - 0.0 if the diver was not bent
        /// </summary>
        public double Time1 { get { return dTimeOne; } set { dTimeOne = value; } }

        /// <summary>
        /// get the first sick time for this dive profile - returns 0.0 if the diver was not bent
        /// </summary>
        public double Time2 { get { return dTimeTwo; } set { dTimeTwo = value; } }

        /// <summary>
        /// get the DCS flag for this dive profile - true if the diver was bent
        /// </summary>
        public bool IsAnyDCS
        {
            get
            {
                if ( bFullDCS || bMarginalDCS )
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// bool test for full DCS
        /// </summary>
        public bool IsFullDCS { get { return bFullDCS; } }

        /// <summary>
        /// get the marginal DCS flag for this dive profile - true if the diver is marginally bent
        /// </summary>
        public bool IsMarginalDCS { get { return bMarginalDCS; } }

        /// <summary>
        /// Set/get the index of the T1 node
        /// </summary>
        public int T1NodeIndex { get { return iT1NodeIndex; } set { iT1NodeIndex = value; } }

        /// <summary>
        /// Set/get the index of the T2 node
        /// </summary>
        public int T2NodeIndex { get { return iT2NodeIndex; } set { iT2NodeIndex = value; } }

        #endregion DCS occurrence methods and properties

        #region PSI methods and properties

        /// <summary>
        /// bool to use perceived severity index
        /// </summary>
        public static bool UsePSI { get { return bUsePSI; } set { bUsePSI = value; } }

        private static PSI PSIType ( string s )
        {
#warning change this.
            if ( s == "PSIUNK" )
                return PSI.UNKNOWN; // this needs to be PAIN to get the number of cases to work out

            if ( s == "PSI1" )
                return PSI.NEUROLOGICALSERIOUS;

            if ( s == "PSI2" )
                return PSI.CARDIOPULMINARY;

            if ( s == "PSI3" )
                return PSI.NEUROLOGICALMILD;

            if ( s == "PSI4" )
                return PSI.PAIN;

            if ( s == "PSI5" )
                return PSI.SKIN;

            if ( s == "PSI6" )
                return PSI.CONSTITUTIONAL;

            return PSI.UNKNOWN;
        }

        /// <summary>
        /// get or set the perceived severity index
        /// </summary>
        public PSI PSIIndex { get { return bPSI; } set { bPSI = value; } }

        /// <summary>
        /// get the severity category (none/mild/serious)
        /// </summary>
        public PSISEVERITY Severity
        {

            get
            {

                if ( bPSI == PSI.NONE )
                    return PSISEVERITY.NONE;

                if ( this.IsMarginalDCS )
                    return PSISEVERITY.MARGINAL;

                switch ( Profile<Node>.SplittingType )
                {

                    case PSISEVERITYSPLITTING.SPLITTING_A_B:
                        if ( bPSI == PSI.UNKNOWN
                            || bPSI == PSI.CONSTITUTIONAL
                            || bPSI == PSI.SKIN
                            || bPSI == PSI.PAIN
                            || bPSI == PSI.NEUROLOGICALMILD )
                            return PSISEVERITY.MILD;

                        if ( bPSI == PSI.CARDIOPULMINARY
                            || bPSI == PSI.NEUROLOGICALSERIOUS )
                            return PSISEVERITY.SERIOUS;
                        break;

                    case PSISEVERITYSPLITTING.SPLITTING_I_II:
                        if ( bPSI == PSI.UNKNOWN
                            || bPSI == PSI.CONSTITUTIONAL
                            || bPSI == PSI.SKIN
                            || bPSI == PSI.PAIN )
                            return PSISEVERITY.MILD;

                        if ( bPSI == PSI.NEUROLOGICALMILD
                            || bPSI == PSI.CARDIOPULMINARY
                            || bPSI == PSI.NEUROLOGICALSERIOUS )
                            return PSISEVERITY.SERIOUS;
                        break;

                    case PSISEVERITYSPLITTING.SPLITTING_NONE:
                        return PSISEVERITY.FULL;

                    case PSISEVERITYSPLITTING.UNKNOWN:
                        throw new DCSException ( "Attempt to use Decompression.Profile.Severity property with SplittingType = PSISEVERITYSPLITTING.UNKNOWN" );

                    default:
                        break;

                }

                return PSISEVERITY.UNKNOWN;

            }

        }

        /// <summary>
        /// bool to test for mild DCS
        /// </summary>
        public bool PSIMildDCS
        {
            get
            {
                if ( Severity == PSISEVERITY.MILD )
                    return true;

                return false;
            }
        }

        /// <summary>
        /// bool to test for serious DCS
        /// </summary>
        public bool PSISeriousDCS
        {
            get
            {
                if ( Severity == PSISEVERITY.SERIOUS )
                    return true;

                return false;
            }
        }

        /// <summary>
        /// get/set tetranomial bool value
        /// </summary>
        public static bool Tetranomial { get { return bTetranomial; } set { bTetranomial = value; } }

        /// <summary>
        /// get a string containing perceived severity index flags
        /// </summary>
        /// <returns></returns>
        public string FlagsString ( )
        {
            string s = this.DCS.ToString ( )
                + ","
                + ( int ) this.PSIIndex
                + ","
                + this.PSIIndex.ToString ( )
                + ","
                + ( int ) this.Severity
                + ","
                + this.Severity.ToString ( )
                + ","
                + this.Divers.ToString ( );
            return s;
        }

        #endregion PSI methods and properties

        #region Reporting methods and properties

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString ( )
        {
            Type t = typeof ( N );
            System.Reflection.MethodInfo m = t.GetMethod ( "HeaderString" );
            string s = ( string ) m.Invoke ( null, new object [ 0 ] );
            s += Environment.NewLine;

            foreach ( N n in NodeNet )
            {
                s += n.ToString ( );
                s += Environment.NewLine;
            }

            return s;
        }

        /// <summary>
        /// Save a .csv file containing profile information
        /// </summary>
        public void SaveProfile ( )
        {
            // get an output file
            SaveFileDialog cFile = new SaveFileDialog ( );
            cFile.Title = @"Open profile output file";
            cFile.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            cFile.FilterIndex = 0;
            cFile.RestoreDirectory = true;
            cFile.OverwritePrompt = true;

            if ( cFile.ShowDialog ( ) != DialogResult.OK )
                return;

            // open the file for writing
            FileStream fs = new FileStream ( cFile.FileName, FileMode.Create, FileAccess.Write, FileShare.None );
            StreamWriter sw = new StreamWriter ( fs );

            // write profile information
            sw.WriteLine ( this.ToString ( ) );

            // close resources
            sw.Close ( );
            fs.Close ( );
        }

        /// <summary>
        /// Save the profile in ANMRI format
        /// </summary>
        public void SaveANMRIProfile ( )
        {
            // get an output file
            SaveFileDialog cFile = new SaveFileDialog ( );
            cFile.Title = @"Open profile output file";
            cFile.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            cFile.FilterIndex = 0;
            cFile.RestoreDirectory = true;
            cFile.OverwritePrompt = true;

            if ( cFile.ShowDialog ( ) != DialogResult.OK )
                return;

            // open the file for writing
            FileStream fs = new FileStream ( cFile.FileName, FileMode.Create, FileAccess.Write, FileShare.None );
            StreamWriter sw = new StreamWriter ( fs );

            // write profile information
            foreach ( string s in ANMRIProf )
                sw.Write ( s );

            // close resources
            sw.Close ( );
            fs.Close ( );
        }

        /// <summary>
        /// Display the ANMRI profile in a Message Box
        /// </summary>
        public void ShowANMRIProfile ( )
        {
            string s = string.Empty;
            foreach ( string _s in ANMRIProf )
                s += _s;

            MessageBox.Show ( s );
        }

        public string GetANMRIProfile ( )
        {

            string s = string.Empty;
            foreach ( string _s in ANMRIProf )
                s += _s;

            return s;

        }

        /// <summary>
        /// The internal format profile in a Message Box
        /// </summary>
        /// <param name="bShort">true for short format</param>
        public void ShowProfile ( bool bShort )
        {
            if ( bShort )
            {
                string s = ANMRIProf [ 0 ] + ANMRIProf [ 1 ];
                int i = new int ( );
                foreach ( Node n in NodeNet )
                {
                    s += n.Time.ToString ( "F2" ) + ", " + n.Depth.ToString ( "F2" );
                    if ( n.IsGasSwitchNode )
                        s += ", " + n.Gas.ToString ( "F2" ) + ", " + n.SwitchTime.ToString ( "F1" );
                    else
                        s += ", " + ", ";

                    if ( n.IsT1Node )
                        s += ", T1";
                    else
                        s += ", ";

                    if ( n.IsT2Node )
                        s += ", T2";
                    else
                        s += ", ";

                    if ( n.IsInsertedNode )
                        s += ", inserted";
                    else
                        s += ", ";

                    //if ( IsAnyDCS )
                    //{
                    //    if ( i == T1NodeIndex )
                    //        s += ", T1 index";
                    //    if ( i == T2NodeIndex )
                    //        s += ", T2 index";
                    //}

                    s += n.DCSProbability.ToString ( "F6" );

                    s += Environment.NewLine;

                    i++;
                }
                s += "-9999" + Environment.NewLine;

                MessageBox.Show ( s );
            }
            else
                MessageBox.Show ( this.ToString ( ) );
        }

        #endregion Reporting methods and properties
    }
}