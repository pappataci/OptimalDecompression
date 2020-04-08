namespace Decompression
{
    /// <summary>
    /// NodeCondition class. Inherits from NodeTissue class. Adds tissue rates for N2, O2, and He.
    /// </summary>
    public class NodeCondition : NodeTissue
    {
        /// <summary>
        /// Blank constructor
        /// </summary>
        public NodeCondition ( )
            : base ( )
        {
        }

        /// <summary>
        /// Two argument assignment constructor
        /// </summary>
        /// <param name="time">node time (min)</param>
        /// <param name="depth">node depth (fsw)</param>
        public NodeCondition ( double time, double depth )
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
        public NodeCondition ( double time, double depth, double gas, double swtime )
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
        public NodeCondition ( double time, double depth, double gas, double swtime, double exercise )
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
    }
}