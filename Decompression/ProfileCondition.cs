namespace Decompression
{
    /// <summary>
    /// ProfileCondition class - derives from ProfileTissue class
    /// </summary>
    /// <typeparam name="N">node with NodeCondition type</typeparam>
    public class ProfileCondition<N> : ProfileTissue<N>
        where N : NodeCondition
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ProfileCondition ( )
            : base ( )
        {
        }

        /// <summary>
        /// Two argument profile constructor
        /// </summary>
        /// <param name="s">first header string from NMRI dive profile</param>
        /// <param name="sFile">data file name</param>
        public ProfileCondition ( string s, string sFile )
            : base ( s, sFile )
        {
        }

        /// <summary>
        /// Sets the nitrogen tissue rates for indicated node
        /// </summary>
        /// <param name="i">node index</param>
        /// <param name="rate">vector of He tissue rates</param>
        public void SetN2TissueRate ( int i, double [ ] rate )
        {
            Node ( i ).N2TissueRate = rate;
        }

        /// <summary>
        /// Sets the oxygen tissue rates for indicated node
        /// </summary>
        /// <param name="i">node index</param>
        /// <param name="rate">vector of He tissue rates</param>
        public void SetO2TissueRate ( int i, double [ ] rate )
        {
            Node ( i ).O2TissueRate = rate;
        }

        /// <summary>
        /// Sets the helium tissue rates for indicated node
        /// </summary>
        /// <param name="i">node index</param>
        /// <param name="rate">vector of He tissue rates</param>
        public void SetHeTissueRate ( int i, double [ ] rate )
        {
            Node ( i ).HeTissueRate = rate;
        }

        /// <summary>
        /// Reports the nitrogen tissue rates for the inidcated node
        /// </summary>
        /// <param name="i">node index</param>
        /// <returns>vector of N2 tissue rates</returns>
        public double [ ] GetN2TissueRate ( int i )
        {
            return Node ( i ).N2TissueRate;
        }

        /// <summary>
        /// Reports the oxygen tissue rates for the inidcated node
        /// </summary>
        /// <param name="i">node index</param>
        /// <returns>vector of O2 tissue rates</returns>
        public double [ ] GetO2TissueRate ( int i )
        {
            return Node ( i ).O2TissueRate;
        }

        /// <summary>
        /// Reports the helium tissue rates for the inidcated node
        /// </summary>
        /// <param name="i">node index</param>
        /// <returns>vector of He tissue rates</returns>
        public double [ ] GetHeTissueRate ( int i )
        {
            return Node ( i ).HeTissueRate;
        }
    }
}