using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VectorMind.RagEngine.Domain.Interfaces;
using VectorMind.RagEngine.Domain.Models;

namespace VectorMind.RagEngine.Infrastructure.Services
{
    public class TextChunker : ITextChunker
    {
        public List<DocumentChunk> CreateChunks(string fullText, string documentName, int chunkSize = 500, int overlap = 50)
        {
            var chunks = new List<DocumentChunk>();

            if (string.IsNullOrWhiteSpace(fullText))
                return chunks;

            // Collapse repeated whitespace/newlines
            var normalized = Regex.Replace(fullText, @"\s+", " ").Trim();

            int step = chunkSize - overlap;
            if (step <= 0) step = chunkSize; // guard against overlap >= chunkSize

            int position = 0;
            int chunkIndex = 0;

            while (position < normalized.Length)
            {
                int length = Math.Min(chunkSize, normalized.Length - position);
                string chunkText = normalized.Substring(position, length);

                chunks.Add(new DocumentChunk(
                    Guid.NewGuid().ToString(),
                    documentName,
                    chunkText,
                    chunkIndex,
                    null // Embedding filled in later by GeminiEmbeddingService
                ));

                chunkIndex++;
                position += step;
            }

            return chunks;
        }
    }
}