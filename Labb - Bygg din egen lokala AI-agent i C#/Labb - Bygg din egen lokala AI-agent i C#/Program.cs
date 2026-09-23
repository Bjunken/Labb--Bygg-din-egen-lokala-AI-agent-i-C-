namespace Labb___Bygg_din_egen_lokala_AI_agent_i_C_ {
    internal class Program {
        static async Task Main(string[] args) {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Din lokala AI-agent ===");
            Console.WriteLine("Skriv en fråga och tryck enter. Skriv 'avsluta för att stänga programmet.\n");

            // init parametrar
            var config = new AgentConfig
            {
                Model = "llama3.2",
                SystemPrompt = "Du är en hjälpsam assistent som svarar kort och tydligt på svenska.",
                Temperature = 0.7,
                MaxTokens = 300,
                TopP = 0.7
            };

            var agent = new LocalAgent(config);

            while (true) {
                Console.Write("Du: ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) {
                    continue;
                }

                if (input.Trim().Equals("avsluta", StringComparison.OrdinalIgnoreCase)) {
                    Console.WriteLine("Agenten avslutas. Hej då!");
                    break;
                }

                Console.WriteLine("Agenten tänker...");
                string svar = await agent.AskAsync(input);

                Console.WriteLine($"Agent: {svar}\n");
            }
        }
    }
}
