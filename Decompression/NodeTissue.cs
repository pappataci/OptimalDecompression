namespace Decompression
{
    /// <summary>
    /// NodeTissue class. Inherits from Node class.  Adds tissue tension values for N2, O2, and He.
    /// </summary>
    public class NodeTissue : Node
    {
        /// <summary>
        /// Number of parallel tissues
        /// </summary>
        public static int NumberOfTissues = 3;

        public static double [ ] N2TissueRateUpperBoundary = { 0.640938503 , 0.033355716 , 0.003997487 };
        public static double [ ] N2TissueRateLowerBoundary = { 0.022877914 , 0.000946790 , 0.000002070 };

        private double [ ] dvN2TissueRate = new double [ NodeTissue.NumberOfTissues ];
        private double [ ] dvO2TissueRate = new double [ NodeTissue.NumberOfTissues ];
        private double [ ] dvHeTissueRate = new double [ NodeTissue.NumberOfTissues ];

        private double [ ] dvN2Tension = new double [ NumberOfTissues ];
        private double [ ] dvO2Tension = new double [ NumberOfTissues ];
        private double [ ] dvHeTension = new double [ NumberOfTissues ];

        private double [ ] dvInstantaneousRisk = new double [ NumberOfTissues ];
        private double [ ] dvIntegratedRisk = new double [ NumberOfTissues ];

        #region constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public NodeTissue ( )
            : base ( )
        {
        }

        /// <summary>
        /// Two argument assignment constructor
        /// </summary>
        /// <param name="time">node time (min)</param>
        /// <param name="depth">node depth (fsw)</param>
        public NodeTissue ( double time, double depth )
            : base ( time, depth )
        {
        }

        /// <summary>
        /// Four argument constructor
        /// </summary>
        /// <param name="time">node time (min)</param>
        /// <param name="depth">node depth (fsw)</param>
        /// <param name="gas">new gas</param>
        /// <param name="swtime">gas switching time (min)</param>
        public NodeTissue ( double time, double depth, double gas, double swtime )
            : base ( time, depth, gas, swtime )
        {
        }

        /// <summary>
        /// Five argument constructor
        /// </summary>
        /// <param name="time">node time (min)</param>
        /// <param name="depth">node depth (fsw)</param>
        /// <param name="gas">new gas</param>
        /// <param name="swtime">gas switching time (min)</param>
        /// <param name="exercise">exercise</param>
        public NodeTissue ( double time, double depth, double gas, double swtime, double exercise )
            : base ( time, depth, gas, swtime, exercise )
        {
        }

        #endregion constructors

        #region tissue rate properties and methods

        /// <summary>
        /// Set/get a vector of N2 tissue rates
        /// </summary>
        public double [ ] N2TissueRate { set { dvN2TissueRate = value; } get { return dvN2TissueRate; } }

        /// <summary>
        /// Set/get a vector of O2 tissue rates
        /// </summary>
        public double [ ] O2TissueRate { set { dvO2TissueRate = value; } get { return dvO2TissueRate; } }

        /// <summary>
        /// Set/get a vector of He tissue rates
        /// </summary>
        public double [ ] HeTissueRate { set { dvHeTissueRate = value; } get { return dvHeTissueRate; } }

        /// <summary>
        /// Get a single N2 tissue rate
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <returns>tissue rate</returns>
        public double GetSingleN2Rate ( int i )
        {
            return dvN2TissueRate [ i ];
        }

        /// <summary>
        /// Get a single O2 tissue rate
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <returns>tissue rate</returns>
        public double GetSingleO2Rate ( int i )
        {
            return dvO2TissueRate [ i ];
        }

        /// <summary>
        /// Get a single He tissue rate
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <returns>tissue rate</returns>
        public double GetSingleHeRate ( int i )
        {
            return dvHeTissueRate [ i ];
        }

        /// <summary>
        /// Set a single N2 tissue rate
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <param name="_r">tissue rate</param>
        public void SetSingleN2Rate ( int i, double _r )
        {
            dvN2TissueRate [ i ] = _r;
        }

        /// <summary>
        /// Set a single O2 tissue rate
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <param name="_r">tissue rate</param>
        public void SetSingleO2Rate ( int i, double _r )
        {
            dvO2TissueRate [ i ] = _r;
        }

        /// <summary>
        /// Set a single He tissue rate
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <param name="_r">tissue rate</param>
        public void SetSingleHeRate ( int i, double _r )
        {
            dvHeTissueRate [ i ] = _r;
        }

        #endregion tissue rate properties and methods

        #region tissue tension properties and methods

        /// <summary>
        /// Set/get vector of nitrogen tissue tensions
        /// </summary>
        public double [ ] N2Tension { set { dvN2Tension = value; } get { return dvN2Tension; } }

        /// <summary>
        /// Set single N2 tissue tension
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <param name="_t">N2 tissue tension</param>
        public void SetSingleN2Tension ( int i, double _t )
        {
            dvN2Tension [ i ] = _t;
        }

        /// <summary>
        /// Get single N2 tissue tension
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <returns>N2 tissueTension</returns>
        public double GetSingleN2Tension ( int i )
        {
            return dvN2Tension [ i ];
        }

        /// <summary>
        /// Set/get vector of oxygen tissue tensions
        /// </summary>
        public double [ ] O2Tension { set { dvO2Tension = value; } get { return dvO2Tension; } }

        /// <summary>
        /// Set single O2 tissue tension
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <param name="_t">O2 tissue tension</param>
        public void SetSingleO2Tension ( int i, double _t )
        {
            dvO2Tension [ i ] = _t;
        }

        /// <summary>
        /// Get single O2 tissue tension
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <returns>O2 tissueTension</returns>
        public double GetSingleO2Tension ( int i )
        {
            return dvO2Tension [ i ];
        }

        /// <summary>
        /// Set/get vector of helium tensions
        /// </summary>
        public double [ ] HeTension { set { dvHeTension = value; } get { return dvHeTension; } }

        /// <summary>
        /// Set single He tissue tension
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <param name="_t">He tissue tension</param>
        public void SetSingleHeTension ( int i, double _t )
        {
            dvHeTension [ i ] = _t;
        }

        /// <summary>
        /// Get single He tissue tension
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <returns>He tissueTension</returns>
        public double GetSingleHeTension ( int i )
        {
            return dvHeTension [ i ];
        }

        #endregion tissue tension properties and methods

        #region risk properties and methods

        /// <summary>
        /// Set/get vector of instantaneous risk values
        /// </summary>
        public double [ ] InstantaneousRisk { set { dvInstantaneousRisk = value; } get { return dvInstantaneousRisk; } }

        /// <summary>
        /// Set a single instantaneous risk value
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <param name="_r">instantaneous risk value</param>
        public void SetSingleInstantaneousRisk ( int i, double _r )
        {
            dvInstantaneousRisk [ i ] = _r;
        }

        /// <summary>
        /// Get a single instataneous risk value
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <returns>instantaneous risk value</returns>
        public double GetSingleInstantaneousRisk ( int i )
        {
            return dvInstantaneousRisk [ i ];
        }

        /// <summary>
        /// Set/get vector of integrated risk values
        /// </summary>
        public double [ ] IntegratedRisk { set { dvIntegratedRisk = value; } get { return dvIntegratedRisk; } }

        /// <summary>
        /// Set a single integrated risk value
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <param name="_r">integrated risk value</param>
        public void SetSingleIntegratedRisk ( int i, double _r )
        {
            dvIntegratedRisk [ i ] = _r;
        }

        /// <summary>
        /// Get a single integrated risk value
        /// </summary>
        /// <param name="i">tissue inted</param>
        /// <returns>integrated risk value</returns>
        public double GetSingleIntegratedRisk ( int i )
        {
            return dvIntegratedRisk [ i ];
        }

        #endregion risk properties and methods

        #region reporting methods

        /// <summary>
        /// Reports the N2, O2, and He tissue tensions
        /// </summary>
        /// <returns>string containing N2, O2, and He tensions</returns>
        public override string ToString ( )
        {
            string s = base.ToString ( );

            foreach ( double d in dvN2TissueRate )
                s += "," + d.ToString ( );

            foreach ( double d in dvO2TissueRate )
                s += "," + d.ToString ( );

            foreach ( double d in dvHeTissueRate )
                s += "," + d.ToString ( );

            foreach ( double d in dvN2Tension )
                s += "," + d.ToString ( );

            foreach ( double d in dvO2Tension )
                s += "," + d.ToString ( );

            foreach ( double d in dvHeTension )
                s += "," + d.ToString ( );

            foreach ( double d in dvInstantaneousRisk )
                s += "," + d.ToString ( );

            foreach ( double d in dvIntegratedRisk )
                s += "," + d.ToString ( );

            return s;
        }

        /// <summary>
        /// Reports the column header information for string returned by ToString.
        /// </summary>
        /// <returns>String containing header information</returns>
        new public static string HeaderString ( )
        {
            string s = Node.HeaderString ( );

            for ( int i = 0; i < NumberOfTissues; i++ )
                s += ",N2 Rate [" + i.ToString ( ) + "]";

            for ( int i = 0; i < NumberOfTissues; i++ )
                s += ",O2 Rate [" + i.ToString ( ) + "]";

            for ( int i = 0; i < NumberOfTissues; i++ )
                s += ",He Rate [" + i.ToString ( ) + "]";

            for ( int i = 0; i < NumberOfTissues; i++ )
                s += ",N2 Tension [" + i.ToString ( ) + "]";

            for ( int i = 0; i < NumberOfTissues; i++ )
                s += ",O2 Tension [" + i.ToString ( ) + "]";

            for ( int i = 0; i < NumberOfTissues; i++ )
                s += ",He Tension [" + i.ToString ( ) + "]";

            for ( int i = 0; i < NumberOfTissues; i++ )
                s += ",Instantaneous Risk [" + i.ToString ( ) + "]";

            for ( int i = 0; i < NumberOfTissues; i++ )
                s += ",Integrated Risk [" + i.ToString ( ) + "]";

            return s;
        }

        #endregion reporting methods
    }
}