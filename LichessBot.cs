using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimpletonChessEngine
{
    public class LichessBot
    {
        private readonly SimpletonChessEngine engine;
        private readonly HttpClient httpClient;
        private readonly string apiToken;
        private string currentGameId;

        public void Dispose()
        {
            httpClient?.Dispose();
        }
        public LichessBot(SimpletonChessEngine engine)
        {
            this.engine = engine;
            this.httpClient = new HttpClient();

            // Učitaj API token iz environment variable ili config file
            this.apiToken = Environment.GetEnvironmentVariable("LICHESS_API_TOKEN")
                           ?? File.ReadAllText("lichess_token.txt").Trim();

            if (string.IsNullOrEmpty(apiToken))
            {
                throw new InvalidOperationException("Lichess API token not found. Set LICHESS_API_TOKEN environment variable or create lichess_token.txt file.");
            }

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}");
        }

        public async Task StartAsync()
        {
            Console.WriteLine("Starting Lichess Bot...");

            try
            {
                // Provjeri account status
                await CheckAccountStatus();

                // Upgrade to bot account if needed
                await UpgradeToBotAccount();

                // Start event stream
                await ListenForChallenges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting Lichess bot: {ex.Message}");
                throw;
            }
        }

        private async Task CheckAccountStatus()
        {
            var response = await httpClient.GetAsync("https://lichess.org/api/account");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var account = JObject.Parse(content);
                Console.WriteLine($"Logged in as: {account["username"]}");

                if (account["title"]?.ToString() != "BOT")
                {
                    Console.WriteLine("Account is not a bot account. Attempting to upgrade...");
                }
            }
            else
            {
                throw new InvalidOperationException("Failed to authenticate with Lichess API");
            }
        }

        private async Task UpgradeToBotAccount()
        {
            var response = await httpClient.PostAsync("https://lichess.org/api/bot/account/upgrade", null);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Successfully upgraded to bot account");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Console.WriteLine("Account is already a bot account");
            }
            else
            {
                Console.WriteLine($"Failed to upgrade account: {response.StatusCode}");
            }
        }

        private async Task ListenForChallenges()
        {
            Console.WriteLine("Listening for challenges and game events...");

            using var stream = await httpClient.GetStreamAsync("https://lichess.org/api/stream/event");
            using var reader = new StreamReader(stream);

            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var eventData = JObject.Parse(line);
                    await HandleEvent(eventData);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Failed to parse event: {ex.Message}");
                }
            }
        }

        private async Task HandleEvent(JObject eventData)
        {
            string eventType = eventData["type"]?.ToString();

            switch (eventType)
            {
                case "challenge":
                    await HandleChallenge(eventData["challenge"]);
                    break;

                case "gameStart":
                    currentGameId = eventData["game"]?["id"]?.ToString();
                    if (!string.IsNullOrEmpty(currentGameId))
                    {
                        Console.WriteLine($"Game started: {currentGameId}");
                        _ = Task.Run(() => PlayGame(currentGameId)); // Start game in background
                    }
                    break;

                case "gameFinish":
                    Console.WriteLine($"Game finished: {eventData["game"]?["id"]}");
                    break;
            }
        }

        private async Task HandleChallenge(JToken challenge)
        {
            string challengeId = challenge["id"]?.ToString();
            string challenger = challenge["challenger"]?["name"]?.ToString();
            string timeControl = challenge["timeControl"]?["type"]?.ToString();

            Console.WriteLine($"Challenge from {challenger} ({timeControl})");

            // Accept challenge (možeš dodati logiku za selective accepting)
            bool shouldAccept = ShouldAcceptChallenge(challenge);

            if (shouldAccept)
            {
                await AcceptChallenge(challengeId);
            }
            else
            {
                await DeclineChallenge(challengeId);
            }
        }

        private bool ShouldAcceptChallenge(JToken challenge)
        {
            // Implementiraj logiku za decision making
            string variant = challenge["variant"]?["key"]?.ToString();
            bool isRated = challenge["rated"]?.ToObject<bool>() ?? false;

            // Accept only standard chess, both rated and casual
            return variant == "standard";
        }

        private async Task AcceptChallenge(string challengeId)
        {
            var response = await httpClient.PostAsync($"https://lichess.org/api/challenge/{challengeId}/accept", null);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Accepted challenge: {challengeId}");
            }
            else
            {
                Console.WriteLine($"Failed to accept challenge: {response.StatusCode}");
            }
        }

        private async Task DeclineChallenge(string challengeId)
        {
            var response = await httpClient.PostAsync($"https://lichess.org/api/challenge/{challengeId}/decline", null);
            Console.WriteLine($"Declined challenge: {challengeId}");
        }

        private async Task PlayGame(string gameId)
        {
            try
            {
                engine.NewGame();

                using var stream = await httpClient.GetStreamAsync($"https://lichess.org/api/bot/game/stream/{gameId}");
                using var reader = new StreamReader(stream);

                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        var gameState = JObject.Parse(line);
                        await HandleGameState(gameId, gameState);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Failed to parse game state: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing game {gameId}: {ex.Message}");
            }
        }

        private async Task HandleGameState(string gameId, JObject gameState)
        {
            string type = gameState["type"]?.ToString();

            if (type == "gameFull" || type == "gameState")
            {
                string moves = gameState["moves"]?.ToString() ??
                              gameState["state"]?["moves"]?.ToString() ?? "";
                string status = gameState["status"]?.ToString() ??
                               gameState["state"]?["status"]?.ToString() ?? "";

                if (status == "started")
                {
                    // Apply all moves to engine
                    engine.NewGame();
                    if (!string.IsNullOrEmpty(moves))
                    {
                        foreach (string move in moves.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                        {
                            engine.MakeMove(move);
                        }
                    }

                    // Check if it's our turn
                    bool isWhite = gameState["white"]?["name"]?.ToString() == "YourBotName"; // Replace with actual bot name
                    int moveCount = string.IsNullOrEmpty(moves) ? 0 : moves.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                    bool ourTurn = (isWhite && moveCount % 2 == 0) || (!isWhite && moveCount % 2 == 1);

                    if (ourTurn && !engine.IsGameOver())
                    {
                        await MakeBotMove(gameId);
                    }
                }
            }
        }

        private async Task MakeBotMove(string gameId)
        {
            try
            {
                string bestMove = engine.GetBestMove();
                if (!string.IsNullOrEmpty(bestMove) && bestMove != "null")
                {
                    var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");
                    var response = await httpClient.PostAsync($"https://lichess.org/api/bot/game/{gameId}/move/{bestMove}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Made move: {bestMove}");
                        engine.MakeMove(bestMove);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to make move {bestMove}: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error making move: {ex.Message}");
            }
        }
    }

}
