using Python.Runtime;
using System.Runtime.InteropServices;
using static TestLibraryFramework.Calculate;
using System;

namespace CSharpWrapperToPython
{
    public static class Class1
    {
       
        public static double[] getAddressOfThisVec(double value , int numOfComponents  )
        {
            var myVector =   createyVectorInFSharp(value, numOfComponents);
            //var handle = GCHandle.Alloc(myVector);
            //var ptr = (IntPtr)handle;
            //return new PyLong(ptr);
            return myVector;
        }
     
    }
} 
