namespace SimpletonChessEngine
{
    internal class Program
    {
        //static void Main(string[] args)
        //{
        //    Console.WriteLine("Hello, World!");
        //}

        static async Task Main(string[] args)
        {
            var engine = new SimpletonChessEngine();

            // Proveri da li je pokrenuto sa posebnim argumentima
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "--lichess":
                        await engine.RunLichessBot();
                        break;
                    case "--winboard":
                    case "--xboard":
                        engine.RunWinBoard();
                        break;
                    case "--uci":
                        engine.RunUCI();
                        break;
                    default:
                        Console.WriteLine("Usage: ChessEngine [--lichess|--winboard|--uci]");
                        break;
                }
            }
            else
            {
                // Default: pokušaj da detektuješ protokol
                engine.RunAutoDetect();
            }
        }
    }
}
