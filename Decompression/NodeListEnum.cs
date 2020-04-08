using System;
using System.Collections.Generic;
using DCSUtilities;

namespace Decompression
{
    /// <summary>
    /// NodeListEnum class
    /// </summary>
    /// <typeparam name="N"></typeparam>
    public class NodeListEnum<N> : IEnumerator<N>
        where N : Node
    {
        private NodeList<N> _list;
        private int position = -1;

        /// <summary>
        ///
        /// </summary>
        /// <param name="list"></param>
        public NodeListEnum ( NodeList<N> list )
        {
            this._list = list;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool MoveNext ( )
        {
            position++;
            return ( position < _list.Length );
        }

        /// <summary>
        ///
        /// </summary>
        public void Reset ( )
        {
            position = -1;
        }

        /// <summary>
        ///
        /// </summary>
        Object System.Collections.IEnumerator.Current
        {
            get
            {
                try
                {
                    return _list [ position ];
                }
                catch ( IndexOutOfRangeException )
                {
                    throw new DCSException ( "Index out of range in Decomplession.NodeListEnum<N>.Current" );
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose ( )
        {
        }

        /// <summary>
        ///
        /// </summary>
        public N Current
        {
            get
            {
                try
                {
                    return _list [ position ];
                }
                catch ( IndexOutOfRangeException )
                {
                    throw new DCSException ( "Index out of range in Decomplession.NodeListEnum<N>.Current" );
                }
            }
        }
    }
}