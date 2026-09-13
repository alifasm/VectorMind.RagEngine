using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using VectorMind.RagEngine.Domain.Interfaces;
using VectorMind.RagEngine.Domain.Models;

namespace VectorMind.RagEngine.Infrastructure.Services
{
    public class GeminiRerankerService : IRerankerService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string Model = "gemini-flash-latest";

        public GeminiRerankerService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<List<DocumentChunk>> RerankAsync(string question, List<DocumentChunk> candidates, int topN)
        {
            // Nothing to rerank if we already have fewer candidates than requested.
            if (candidates.Count <= topN)
                return candidates;

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={_apiKey}";

            var candidateBlock = new StringBuilder();
            for (int i = 0; i < candidates.Count; i++)
            {
                candidateBlock.AppendLine($"[{i}] {candidates[i].Text}");
                candidateBlock.AppendLine();
            }

            var prompt = $@"You are scoring how relevant each numbered passage is to a question.

Question: {question}

Passages:
{candidateBlock}

Score each passage from 0 (irrelevant) to 10 (directly answers the question).
Respond with ONLY a JSON array, no explanation, no markdown formatting, in this exact shape:
[{{""index"": 0, ""score"": 7}}, {{""index"": 1, ""score"": 2}}]

You must include one entry per passage index shown above.";

            var requestBody = new
            {
                contents = new[] { new { role = "user", parts = new[] { new { text = prompt } } } },
                generationConfig = new { temperature = 0.0, maxOutputTokens = 512 }
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestBody);

            // If reranking fails for any reason, fail open: fall back to original vector-search order
            // rather than breaking the whole query.
            if (!response.IsSuccessStatusCode)
                return candidates.Take(topN).ToList();

            var result = await response.Content.ReadFromJsonAsync<GeminiGenerateResponse>();
            var rawText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(rawText))
                return candidates.Take(topN).ToList();

            try
            {
                var cleaned = rawText.Trim().Trim('`').Replace("json", "", StringComparison.OrdinalIgnoreCase).Trim();
                var scores = JsonSerializer.Deserialize<List<ScoreEntry>>(cleaned);

                if (scores == null || scores.Count == 0)
                    return candidates.Take(topN).ToList();

                return scores
                    .Where(s => s.Index >= 0 && s.Index < candidates.Count)
                    .OrderByDescending(s => s.Score)
                    .Take(topN)
                    .Select(s => candidates[s.Index])
                    .ToList();
            }
            catch (JsonException)
            {
                // Model didn't return clean JSON — fail open rather than crash the request.
                return candidates.Take(topN).ToList();
            }
        }

        private class ScoreEntry
        {
            [JsonPropertyName("index")]
            public int Index { get; set; }

            [JsonPropertyName("score")]
            public double Score { get; set; }
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