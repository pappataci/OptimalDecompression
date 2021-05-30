using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNumerics;
using Newtonsoft.Json;
using System.IO;
using static ILNumerics.ILMath;

namespace FuncApprox
{
    public class ArrayTool
    {
        public static void ToDisk(Array<double> X, string fileName)
        {
            var convertedString = JsonConvert.SerializeObject(X);
            File.WriteAllText(fileName , convertedString);
        }

        public static Array<double> ArrayFromDisk(string fileName)
        {
            var stringContent = File.ReadAllText(fileName);
            var systemArray = JsonConvert.DeserializeObject<double[]>(stringContent);
            Array<double> output = vector(systemArray);
            return output;
        }
    }
}
