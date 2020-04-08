#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\CNTK.CPUOnly.2.7.0\lib\netstandard2.0\Cntk.Core.Managed-2.7.dll"

open CNTK

open System.Runtime.InteropServices
open System

System.Environment.CurrentDirectory <- @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\CNTK.CPUOnly.2.7.0\support\x64\Release"
module Kernel = 
     [<DllImport("Cntk.Core.CSBinding-2.7.dll", CharSet = CharSet.Auto, SetLastError = true)>]
       extern bool SetDllDirectory(string lpPathName);

Kernel.SetDllDirectory(@"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\CNTK.CPUOnly.2.7.0\support\x64\Release")
let cpu = DeviceDescriptor.UseDefaultDevice()