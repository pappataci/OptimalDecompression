using System;
namespace Decompression
{


    /// <summary>
    /// NodeBubble class. Inherits from NodeTissue class. Adds bubble radius.
    /// </summary>
    public class NodeBubble : NodeCondition
    {

        private double [ ] dvBubbleRadius   = new double [ NumberOfTissues ];
        private double [ ] dvBubblePressure = new double [ NumberOfTissues ];

        /// <summary>
        /// Blank constructor
        /// </summary>
        public NodeBubble ( )
            : base ( )
        {

        }

        /// <summary>
        /// Two argument assignment constructor
        /// </summary>
        /// <param name="time">node time (min)</param>
        /// <param name="depth">node depth (fsw)</param>
        public NodeBubble ( double time, double depth )
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
        public NodeBubble ( double time, double depth, double gas, double swtime )
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
        public NodeBubble ( double time, double depth, double gas, double swtime, double exercise )
            : base ( time, depth, gas, swtime, exercise )
        {

        }

        /// <summary>
        /// Reports the N2, O2, and He tissue tensions
        /// </summary>
        /// <returns>string containing N2, O2, and He tensions</returns>
        public override string ToString ( )
        {
            string s = base.ToString ( );
            return s;
        }

        /// <summary>
        /// Reports the column header information for string returned by ToString.
        /// </summary>
        /// <returns>String containing header information</returns>
        new public static string HeaderString ( )
        {
            string s = NodeTissue.HeaderString ( );
            return s;
        }

        public double [ ] BubbleRadius { get { return dvBubbleRadius; } set { dvBubbleRadius = value; } }

        public void SetSingleBubbleRadius ( int _i, double _r )
        {

            dvBubbleRadius [ _i ] = _r;

        }

        public double GetSingleBubbleRadius ( int _i )
        {

            return dvBubbleRadius [ _i ];

        }

        public double GetSingleBubbleVolume ( int _i )
        {

            return 4.188790204786390 * Math.Pow ( dvBubbleRadius [ _i ], 3.0 );

        }

        public double [ ] BubbleVolume 
        { 

            get 
            {
                var bv = new double [ NodeTissue.NumberOfTissues ];

                for ( int i=0; i < NodeTissue.NumberOfTissues; i++ )
                    bv [ i ] = GetSingleBubbleVolume ( i );
                return bv;
            } 

        }

        public double [] BubblePressure
        {

            get
            {
                return dvBubblePressure;
            }

            set
            {
                dvBubblePressure = value;
            }
            
        }
        
    }
    
}
