using System;
using System.Collections.Generic;
using System.Text;

namespace Labb___Bygg_din_egen_lokala_AI_agent_i_C_ {
    internal class AgentConfig {

        // Parametrar som styr hur agenten beter sig. 
        // Set model
        public string Model { get; set; } = "llama3.2";

        // grund instruktioner / personlighet
        public string SystemPrompt { get; set; } = "Du är en hjälpsam assistent som svarar kort och tydligt på svenska";

        /* Styr det kreativa
        0.0 = mycket fokuserat / 1.0 = mer varierat
        */
        public double Temperature { get; set; } = 0.7;

        // Tokens - hur långt svaret få vara-ish
        public int MaxTokens { get; set; } = 500;

        // Styr urval av ord. lägre värde = mer begränsat
        public double TopP { get; set; } = 0.9;
    }
}
