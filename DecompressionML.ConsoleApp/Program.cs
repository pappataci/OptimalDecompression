// This file was auto-generated by ML.NET Model Builder. 

using System;
using DecompressionML.Model;

namespace DecompressionML.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create single instance of sample data from first line of dataset for model input
            ModelInput sampleData = new ModelInput()
            {
                P1 = 0.8F,
                P2 = 0.8F,
                P3 = 0.8F,
            };

            // Make a single prediction on the sample data and print results
            var predictionResult = ConsumeModel.Predict(sampleData);

            Console.WriteLine("Using model to make single prediction -- Comparing actual ResRisk with predicted ResRisk from sample data...\n\n");
            Console.WriteLine($"P1: {sampleData.P1}");
            Console.WriteLine($"P2: {sampleData.P2}");
            Console.WriteLine($"P3: {sampleData.P3}");
            Console.WriteLine($"\n\nPredicted ResRisk: {predictionResult.Score}\n\n");
            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }
    }
}
