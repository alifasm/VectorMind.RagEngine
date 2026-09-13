using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using VectorMind.RagEngine.Domain.Interfaces;
using VectorMind.RagEngine.Domain.Models;

namespace VectorMind.RagEngine.Infrastructure.Services
{
    public class GeminiChatService : IAnswerGenerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string Model = "gemini-flash-latest";
        public GeminiChatService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<string> GenerateAnswerAsync(string question, List<DocumentChunk> context)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={_apiKey}";

            // Build clearly-labeled context blocks so the model can distinguish sources
            var contextBlock = new StringBuilder();
            for (int i = 0; i < context.Count; i++)
            {
                contextBlock.AppendLine($"[Source {i + 1} — {context[i].DocumentName}, chunk {context[i].ChunkIndex}]");
                contextBlock.AppendLine(context[i].Text);
                contextBlock.AppendLine();
            }

            var prompt = $@"You are answering a question using ONLY the document excerpts below.

Instructions:
- Write one clear, well-organized answer in plain English (2–5 sentences unless the question needs a list).
- If the excerpts don't contain enough information to answer, say so directly — do not guess.
- Do not repeat the raw excerpts back; synthesize them into a single coherent answer.
- If you use a fact from a specific source, mention it as (Source N) inline.

Document excerpts:
{contextBlock}

Question: {question}

Answer:";

            var requestBody = new
            {
                contents = new[]
                {
                    new { role = "user", parts = new[] { new { text = prompt } } }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    maxOutputTokens = 512
                }
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiGenerateResponse>();

            var answer = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            return string.IsNullOrWhiteSpace(answer)
                ? "I couldn't generate an answer from the retrieved content."
                : answer.Trim();
        }

        private class GeminiGenerateResponse
        {
            [JsonPropertyName("candidates")]
            public List<Candidate>? Candidates { get; set; }
        }

        private class Candidate
        {
            [JsonPropertyName("content")]
            public ContentPart? Content { get; set; }
        }

        private class ContentPart
        {
            [JsonPropertyName("parts")]
            public List<TextPart>? Parts { get; set; }
        }

        private class TextPart
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }
    }
}