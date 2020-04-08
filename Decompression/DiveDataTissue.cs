namespace Decompression
{
    /// <summary>
    /// DiveDataTissue class - derives from DiveData class
    /// </summary>
    /// <typeparam name="P">profile type with ProfileTissue base</typeparam>
    /// <typeparam name="N">node type with NodeTissue base</typeparam>
    public class DiveDataTissue<P, N> : DiveData<P, N>
        where P : ProfileTissue<N>
        where N : NodeTissue
    {
        private double [ ] m_dvN2TissueRate = new double [ NodeTissue.NumberOfTissues ];
        private double [ ] m_dvO2TissueRate = new double [ NodeTissue.NumberOfTissues ];
        private double [ ] m_dvHeTissueRate = new double [ NodeTissue.NumberOfTissues ];

        /// <summary>
        /// Default constructor
        /// </summary>
        public DiveDataTissue ( )
            : base ( )
        {
            for ( int i = 0; i < NodeTissue.NumberOfTissues; i++ )
            {
                m_dvN2TissueRate [ i ] = 1.0;
                m_dvO2TissueRate [ i ] = 1.0;
                m_dvHeTissueRate [ i ] = 1.0;
            }
        }

        /// <summary>
        /// Set/get vector of N2 tissue rates
        /// </summary>
        public virtual double [ ] N2TissueRate { set { m_dvN2TissueRate = value; } get { return m_dvN2TissueRate; } }

        /// <summary>
        /// Set/get vector of O2 tissue rates
        /// </summary>
        public virtual double [ ] O2TissueRate { set { m_dvO2TissueRate = value; } get { return m_dvO2TissueRate; } }

        /// <summary>
        /// Set/get vector of He tissue rates
        /// </summary>
        public virtual double [ ] HeTissueRate { set { m_dvHeTissueRate = value; } get { return m_dvHeTissueRate; } }

        /// <summary>
        /// Get a single N2 tissue rate
        /// </summary>
        /// <param name="t">tissue index</param>
        /// <returns>N2 tissue rate</returns>
        public double GetSingleN2Rate ( int t )
        {
            return m_dvN2TissueRate [ t ];
        }

        /// <summary>
        /// Get a single O2 tissue rate
        /// </summary>
        /// <param name="t">tissue index</param>
        /// <returns>O2 tissue rate</returns>
        public double GetSingleO2Rate ( int t )
        {
            return m_dvO2TissueRate [ t ];
        }

        /// <summary>
        /// Get a single He tissue rate
        /// </summary>
        /// <param name="t">tissue index</param>
        /// <returns>He tissue rate</returns>
        public double GetSingleHeRate ( int t )
        {
            return m_dvHeTissueRate [ t ];
        }

        /// <summary>
        /// Set a single N2 tissue rate
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <param name="_r">tissue rate</param>
        public virtual void SetSingleN2Rate ( int i, double _r )
        {
            m_dvN2TissueRate [ i ] = _r;
        }

        /// <summary>
        /// Set a single O2 tissue rate
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <param name="_r">tissue rate</param>
        public virtual void SetSingleO2Rate ( int i, double _r )
        {
            m_dvO2TissueRate [ i ] = _r;
        }

        /// <summary>
        /// Set a single H2 tissue rate
        /// </summary>
        /// <param name="i">tissue index</param>
        /// <param name="_r">tissue rate</param>
        public virtual void SetSingleHeRate ( int i, double _r )
        {
            m_dvHeTissueRate [ i ] = _r;
        }
    }
}