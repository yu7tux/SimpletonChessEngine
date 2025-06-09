namespace SimpletonChessEngine
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			IChessEngine engine = EngineFactory.CreateEngine("SIMPLETON");

			// Proveri argumente
			if (args.Length > 0)
			{
				switch (args[0].ToLower())
				{
					case "--lichess":
						//var lichessBot = new LichessBotHandler(engine);
						//await lichessBot.Run();
						Console.WriteLine("Lichess bot not implemented yet");
						break;
					case "--winboard":
					case "--xboard":
						//var winboardHandler = new WinBoardHandler(engine);
						//winboardHandler.Run();
						Console.WriteLine("WinBoard not implemented yet");
						break;
					case "--uci":
					default:
						var uciHandler = new UCIHandler(engine);
						uciHandler.Run();
						break;
				}
			}
			else
			{
				// Default: UCI
				var uciHandler = new UCIHandler(engine);
				uciHandler.Run();
			}
		}
	}
}
