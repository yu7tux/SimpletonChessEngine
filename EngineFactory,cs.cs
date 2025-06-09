using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpletonChessEngine
{
    public static class EngineFactory
    {
        public static IChessEngine CreateEngine(string engineType)
        {
            return engineType.ToUpper() switch
            {
                "SIMPLETON" => new SimpletonChessEngine(),
                //"MATH_V1" => new MathBasedEngine(), // kad napraviš
                //"NEURAL" => new NeuralNetworkEngine(), // kad napraviš
                _ => new SimpletonChessEngine() // fallback
            };
        }

        // Lista dostupnih engine-a
        public static string[] GetAvailableEngines()
        {
            return new[] { "SIMPLETON", "MATH_V1", "NEURAL" };
        }
    }
}
