using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Python.Included;
using Python.Runtime;


namespace PythonInteropFromCSharp
{
    class Program
    {
        static void Main(string[] _)
        {
            using (Py.GIL())
            {

                dynamic np = Py.Import("numpy");
                
                dynamic trc = Py.Import("torch");
                
                double al = np.cos(np.pi / 2); 
                Console.WriteLine(al) ;
                Console.Read();

            }
        }
    }
}
