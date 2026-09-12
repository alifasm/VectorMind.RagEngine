using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using VectorMind.RagEngine.Domain.Interfaces;

namespace VectorMind.RagEngine.Infrastructure.Services
{
    public class GeminiEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string Model = "gemini-embedding-2";
        private const int OutputDimensions = 768;

        public GeminiEmbeddingService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:embedContent?key={_apiKey}";

            var requestBody = new
            {
                model = $"models/{Model}",
                content = new
                {
                    parts = new[] { new { text } }
                },
                outputDimensionality = OutputDimensions
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiEmbeddingResponse>();

            if (result?.Embedding?.Values == null)
                throw new InvalidOperationException("Gemini API returned no embedding values.");

            return result.Embedding.Values;
        }

        private class GeminiEmbeddingResponse
        {
            [JsonPropertyName("embedding")]
            public EmbeddingValues? Embedding { get; set; }
        }

        private class EmbeddingValues
        {
            [JsonPropertyName("values")]
            public float[]? Values { get; set; }
        }
    }
}