using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decompression
{

    public class ODFTimes : IComparable<ODFTimes>
    {

        public double T1;
        public double T2;
        public double PDCS;

        public ODFTimes ( double t1, double t2, double pdcs )
        {

            T1   = Math.Round ( t1 );
            T2   = Math.Round ( t2 );
            PDCS = pdcs;

        }
        
        public int CompareTo ( ODFTimes other )
        {

            if ( this.T1 > other.T1 )
                return 1;

            if ( this.T1 == other.T1 )
            {

                if ( this.T2 == other.T2 )
                    return 0;

                if ( this.T2 < other.T2 )
                    return 1;

            }

            return -1;

        }

        public override string ToString ( )
        {

            return String.Format ( "{ 0},{ 1}" , T1 , T2 );

        }

    }

}
