namespace DCSUtilities
{
    public static class GAS
    {

#warning changes here to get USN93 to match USN numbers.

        //  To match the Navy models, use
        // static private bool bThalmannError = true;
        // public const double dPACO2  = 0.0460526315789;
        // public const double dPH2O   = 6.184210526315789E-02;

        static private bool bThalmannError = false;
        //static private bool bMetabolicGas  = true;

        public const double dFO2Air = 0.2100;
        public const double dPACO2  = 0.0460;
        public const double dPVO2   = 0.0605;
        public const double dPVCO2  = 0.0696;
        public const double dPH2O   = 0.0617;
        public const double dPFVG   = 0.1917;
        public const double dPFVG2  = 0.19210526315789;
        public const double dPTMG   = 0.153947368421053;

        /// <summary>
        /// Calculate the arterial inert gas partial pressure for STP Air, equation A11 from Thalmann et al.
        /// </summary>
        /// <param name="dPamb">ambient pressure (ata)</param>
        /// <returns></returns>
        static public double N2PressureAir ( double dPamb )
        { // checked 08/03/2010, Thalmann error added 10/03/2016
            if ( bThalmannError )
            {
                return ( dPamb - GAS.dPH2O - GAS.dPACO2 ) * ( 1.0 - GAS.dFO2Air );
            }
            else
            {
                return ( dPamb - GAS.dPH2O ) * ( 1.0 - GAS.dFO2Air );
            }
        }

        static public bool ThalmannError { set { bThalmannError = value; } get { return bThalmannError; } }

        /// <summary>
        /// Calculate the arterial inert gas partial pressure for a given fixed O2 fraction, equation A13 from Thalmann et al.
        /// </summary>
        /// <param name="dPamb">ambient pressure (ata)</param>
        /// <param name="dFO2">02 fraction</param>
        /// <returns></returns>
        static public double N2PressureFO2 ( double dPamb, double dFO2 )
        { // checked 08/03/2010, Thalmann error added 10/03/2016
            if ( bThalmannError )
            {
                return ( dPamb - GAS.dPH2O - GAS.dPACO2 ) * ( 1.0 - dFO2 );
            }
            else
            {
                return ( dPamb - GAS.dPH2O ) * ( 1.0 - dFO2 );
            }
        }

        /// <summary>
        /// Calculate the ambient pressure given the depth.
        /// </summary>
        /// <param name="depth">depth (fsw)</param>
        /// <returns>pressure (ata)</returns>
        static public double Pressure ( double depth )
        {
            return 1.0 + depth / 33.066;
        }
    }
}