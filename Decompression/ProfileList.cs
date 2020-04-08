using System.Collections.Generic;

namespace Decompression
{
    /// <summary>
    /// ProfileList class
    /// </summary>
    /// <typeparam name="P">profile of type Pofile</typeparam>
    /// <typeparam name="N">node of type Node</typeparam>
    public class ProfileList<P, N>
        where P : Profile<N>
        where N : Node
    {
        private List<P> Profiles = new List<P> ( );

        /// <summary>
        /// Profile list constructor
        /// </summary>
        public ProfileList ( )
        {
            Profiles.Clear ( );
        }

        /// <summary>
        /// Adds a profile to the list
        /// </summary>
        /// <param name="profile">generic profile</param>
        public void Add ( P profile )
        {
            Profiles.Add ( profile );
        }

        /// <summary>
        /// Gets the number of profiles
        /// </summary>
        public int Length { get { return Profiles.Count; } }

        /// <summary>
        /// Generic Enumerator for use with foreach loops
        /// </summary>
        /// <returns>enumerator</returns>
        public System.Collections.Generic.IEnumerator<P> GetEnumerator ( )
        {
            for ( int i = 0; i < Profiles.Count; i++ )
                yield return Profiles [ i ];
        }

        /// <summary>
        /// Clear the profile list
        /// </summary>
        public void Clear ( )
        {
            Profiles.Clear ( );
        }

        /// <summary>
        /// Removes indicated profile from collection
        /// </summary>
        /// <param name="i">index of profile to remove</param>
        public void RemoveAt ( int i )
        {
            Profiles.RemoveAt ( i );
        }

        /// <summary>
        /// Profile indexer
        /// </summary>
        /// <param name="index">profile index</param>
        /// <returns>profile at specified index</returns>
        public P this [ int index ]
        {
            get
            {
                return Profiles [ index ];
            }
        }
    }
}