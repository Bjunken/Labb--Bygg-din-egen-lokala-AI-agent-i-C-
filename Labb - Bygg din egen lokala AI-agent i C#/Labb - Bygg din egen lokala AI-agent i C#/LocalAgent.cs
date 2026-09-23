using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Xml.Schema;
using System.Security.Principal;

namespace Labb___Bygg_din_egen_lokala_AI_agent_i_C_ {
    internal class LocalAgent {
        // Pratar med ollama-server via http och skickar med parametrar
        private readonly HttpClient _httpClient;
        private readonly AgentConfig _config;

        // Ollama adressen
        private const string OllamaEndpoint = "http://localhost:11434/api/chat";

        public LocalAgent(AgentConfig config) {
            _config = config;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
        }
        // Skickar frågan och retunerar svar
        public async Task<string> AskAsync(string userMessage) {
            var request = new OllamaRequest
            {
                Model = _config.Model,
                Stream = false,
                Messages = new List<ChatMessage>
                {
                    new() {Role = "system", Content = _config.SystemPrompt },
                    new() {Role = "user", Content = userMessage}
                },
                Options = new OllamaOptions
                {
                    Temperature = _config.Temperature,
                    TopP = _config.TopP,
                    NumPredict = _config.MaxTokens
                }
            };

            try {
                var response = await _httpClient.PostAsJsonAsync(OllamaEndpoint, request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
                return result?.Message?.Content ?? "(Inget svar mottogs från modellen.)";
            } catch (HttpRequestException ex) {
                return $"Kunde inte nå Ollama. Kontrollera att Ollama körs. Felmeddelande: {ex.Message}";
            } catch (TaskCanceledException) {
                return "Förfrågan tog för låg tid (timeout). Prova en mindre modell eller lägre MaxTokens.";
            }
        }
    }
    // Klasser som beskriver format
    public class OllamaRequest {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "";
        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = new();
        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
        [JsonPropertyName("options")]
        public OllamaOptions Options { get; set; } = new();
    }
    public class ChatMessage {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "";
        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
        }
    public class OllamaOptions {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }
        [JsonPropertyName("top_p")]
        public double TopP { get; set; }
        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; }
    }
    public class OllamaResponse {
        [JsonPropertyName("message")]
        public ChatMessage? Message { get; set; }
    }
}
