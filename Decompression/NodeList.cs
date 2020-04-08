using System.Collections.Generic;

namespace Decompression
{
    /// <summary>
    /// NodeList array
    /// </summary>
    public class NodeList<N>
        where N : Node
    {
        private List<N> Nodes = new List<N> ( );

        /// <summary>
        /// Node network constructor
        /// </summary>
        public NodeList ( )
        {
            Nodes.Clear ( );
        }

        /// <summary>
        /// Add a node to the network
        /// </summary>
        /// <param name="node"></param>
        public void Add ( N node )
        {
            Nodes.Add ( node );
        }

        /// <summary>
        /// Inset a node into node list.
        /// </summary>
        /// <param name="i">insertion index</param>
        /// <param name="node">insertion node</param>
        public void Insert ( int i, N node )
        {
            Nodes.Insert ( i, node );
        }

        /// <summary>
        /// length property - get the number of dive nodes in this dive profile
        /// </summary>
        public int Length { get { return Nodes.Count; } }

        /// <summary>
        /// Indexer - return the indicated node
        /// </summary>
        /// <param name="index">node index (int)</param>
        /// <returns></returns>
        public virtual N this [ int index ]
        {
            get { return Nodes [ index ]; }
        }

        /// <summary>
        /// Generic Enumerator for use with foreach loops
        /// </summary>
        /// <returns></returns>
        public virtual System.Collections.Generic.IEnumerator<N> GetEnumerator ( )
        {
            for ( int i = 0; i < Nodes.Count; i++ )
                yield return Nodes [ i ];
        }

        /// <summary>
        /// Clear the node network
        /// </summary>
        public void Clear ( )
        {
            Nodes.Clear ( );
        }
    }
}