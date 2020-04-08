using System;
using System.Windows.Forms;

namespace Decompression
{

    /// <summary>
    /// ProfileTissue class - derives from Profile class
    /// </summary>
    /// <typeparam name="N">node with base NodeTissue</typeparam>
    public class ProfileTissue<N> : Profile<N>
        where N : NodeTissue
    {

        private bool bFirstPass = true;

        private double [ ] dvMaxInstantaneousRisk = new double [ NodeTissue.NumberOfTissues ];

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProfileTissue ( )
            : base ( )
        {
        }

        /// <summary>
        /// Two argument profile constructor
        /// </summary>
        /// <param name="s">first header string from NMRI dive profile</param>
        /// <param name="sFile">data file name</param>
        public ProfileTissue ( string s, string sFile )
            : base ( s, sFile )
        {
        }

        /// <summary>
        /// Set/get first pass flag
        /// </summary>
        public bool FirstPass { set { bFirstPass = value; } get { return bFirstPass; } }

        /// <summary>
        /// Sets the nitrogen tissue tension for the indicated node
        /// </summary>
        /// <param name="i">node index</param>
        /// <param name="tension">vector of N2 tissue tensions</param>
        public void SetN2Tension ( int i, double [ ] tension )
        {
            Node ( i ).N2Tension = tension;
        }

        /// <summary>
        /// Sets oxygen tissue tension for the indicated node
        /// </summary>
        /// <param name="i">node index</param>
        /// <param name="tension">vector of O2 tissue tensions</param>
        public void SetO2Tension ( int i, double [ ] tension )
        {
            Node ( i ).O2Tension = tension;
        }

        /// <summary>
        /// Sets helium tissue tension for the indicated node
        /// </summary>
        /// <param name="i">node index</param>
        /// <param name="tension">vector of He tissue tensions</param>
        public void SetHeTension ( int i, double [ ] tension )
        {
            Node ( i ).HeTension = tension;
        }

        /// <summary>
        /// Reports nitrogen tissue tension for the indicated node
        /// </summary>
        /// <param name="i">node index</param>
        /// <returns>vector of N2 tissue tensions</returns>
        public double [ ] GetN2Tension ( int i )
        {
            return Node ( i ).N2Tension;
        }

        /// <summary>
        /// Reports oxygen tissue tension for the indicated node
        /// </summary>
        /// <param name="i">node index</param>
        /// <returns>vector of O2 tissue tensions</returns>
        public double [ ] GetO2Tension ( int i )
        {
            return Node ( i ).O2Tension;
        }

        /// <summary>
        /// Reports helium tissue tension for the indicated node
        /// </summary>
        /// <param name="i">node index</param>
        /// <returns>vector of He tissue tensions</returns>
        public double [ ] GetHeTension ( int i )
        {
            return Node ( i ).HeTension;
        }

        /// <summary>
        /// Set the N2 tissue rates for all tissues
        /// </summary>
        public double [ ] N2TissueRate
        {
            set { foreach ( N n in NodeNet ) N2TissueRate = value; }
        }

        /// <summary>
        /// Set the O2 tissue rates for all tissues
        /// </summary>
        public double [ ] O2TissueRate
        {
            set { foreach ( N n in NodeNet ) O2TissueRate = value; }
        }

        /// <summary>
        /// Set the He tissue rates for all tissues
        /// </summary>
        public double [ ] HeTissueRate
        {
            set { foreach ( N n in NodeNet ) HeTissueRate = value; }
        }

        /// <summary>
        /// Get the max instantaneous risk vector
        /// </summary>
        /// <returns>max vector</returns>
        //public double [ ] GetMaxInstantaneousRisk ( )
        //{
        //    return dvMaxInstantaneousRisk;
        //}

        /// <summary>
        /// Reset the max instantaneous risk vector
        /// </summary>
        public void ResetMaxInstantaneousRisk ( )
        {
            for ( int i = 0 ; i < dvMaxInstantaneousRisk.Length ; i++ )
                dvMaxInstantaneousRisk [ i ] = -1000.0;
        }

        /// <summary>
        /// Set the max instantaneous risk vector
        /// </summary>
        /// <param name="_vec">max vector</param>
        public void SetMaxInstantaneousRisk ( double [ ] _vec )
        {
            for ( int i = 0 ; i < _vec.Length ; i++ )
                dvMaxInstantaneousRisk [ i ] = Math.Max ( dvMaxInstantaneousRisk [ i ] , _vec [ i ] );
        }

        /// <summary>
        /// Get/Set the max instantaneous risk vector
        /// </summary>
        public double[] MaxInstantanouseRisk
        {
            get { return dvMaxInstantaneousRisk; }
            // set { dvMaxInstantaneousRisk = value; }
        }

        /// <summary>
        /// Get the maximum component of the max instantaneous risk vector
        /// </summary>
        /// <returns>largest component of max vector</returns>
        //public double LargestMaxInstantaneousRisk ()
        //{
        //    // TODO we need to only count over risk bearing compartments
        //    var max = dvMaxInstantaneousRisk[0];
        //    for ( int i = 1 ; i < dvMaxInstantaneousRisk.Length ; i++ )
        //        max = Math.Max ( max , dvMaxInstantaneousRisk [ i ] );

        //    return max;

        //}

        /// <summary>
        /// Get the minimum component of the max instantaneous risk vector
        /// </summary>
        /// <returns>smallest component of max vector</returns>
        //public double SmallestMaxInstantaneousRisk ( )
        //{

        //    var min = dvMaxInstantaneousRisk[0];
        //    for ( int i = 1 ; i < dvMaxInstantaneousRisk.Length ; i++ )
        //        min = Math.Min ( min , dvMaxInstantaneousRisk [ i ] );

        //    return min;

        //}

        /// <summary>
        /// Set a single component of the max instantaneous risk vector
        /// </summary>
        /// <param name="i">index of component to set</param>
        /// <param name="d">value of component to set</param>
        //public void SetSingleMaxInstantaneousRisk ( int i , double d )
        //{
        //    dvMaxInstantaneousRisk [ i ] = d;
        //}

        /// <summary>
        /// Get a single component of the max instantaneous risk vector
        /// </summary>
        /// <param name="i">index of component to get</param>
        /// <returns></returns>
        //public double GetSingleMaxInstantaneousRisk ( int i )
        //{
        //    return dvMaxInstantaneousRisk [ i ];
        //}
        
    }

}