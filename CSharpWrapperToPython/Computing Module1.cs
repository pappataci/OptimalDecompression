﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNumerics;
using static ILNumerics.Globals;
using static ILNumerics.ILMath;

namespace CSharpWrapperToPython
{

    /// <summary>
    /// Example ILNumerics Computing Module
    /// </summary>
    /// <remarks>This is an example showing how to write and use simple ILNumerics Computing Modules.
    /// The purpose of the code in here is demonstration only. You may alter, use or delete any parts of it as you will.</remarks>
    public class Computing_Module1
    {

        public static RetArray<double> SimpleMethod(InArray<double> A, InArray<double> B)
        {
            // add all input parameters to the local Scope!
            using (Scope.Enter(A, B))
            {

                // ... your algorithm here ... 
                // 
                // You can use all members of ILMath here directly without the 'ILMath.' prefix: 
                // rand(10,300); sin(A); A[full, r(end/2,end)] ...
                // 
                // visit the online documentation: http://ilnumerics.net

                Array<double> C = SpecialData.sinc(40, 50);
                C[1, 1] = 0.9;
                return C[0, full];
            }
        }

        public static RetArray<double> MethodWithOutputParameters(InArray<int> A, InArray<double> B, OutArray<float> C = null)
        {
            // add all input parameters to the local Scope
            using (Scope.Enter(A, B))
            {

                // 
                // ... your algorithm here ... 
                // 

                // How to handle output parameters: 
                // Output parameters should be optional. Users of the method can decide to request the parameter by providing an 
                // predefined array to it. Otherwise, they ommit the parameter. Ommited output parameters are set to null. Here, 
                // we check for it: 
                if (!isnull(C))
                {
                    // Always use .a property to assign to output arrays!
                    C.a = ones<float>(100, 200);
                }
                // keep in mind to declare all array variables explicitly! 
                // Do not use the 'var' keyword here! The compiler would fail to infer the type correctly. 
                Array<double> D = rand(10, 20);

                return D[A, full];  // you can return any array here directly - no casting is required
            }
        }

        /// <summary> 
        /// Example method showing the utilization of the computing module 
        /// </summary>
        public static void Main(params string[] parameter)
        {

            Array<double> A = 1;
            // call a method on the computing module: 
            A = Computing_Module1.SimpleMethod(A, A + 1);

            // call a method with output arguments: 
            Array<int> I = 0;
            Array<float> C = -1.4f;
            A = Computing_Module1.MethodWithOutputParameters(I, A, C);
            // C is output argument and was altered in the method
            // I is input argument and garanteed not to be altered by the method
            // ... do something with A...
        }

    }

}
