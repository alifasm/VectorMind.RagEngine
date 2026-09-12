using System.Collections.Generic;
using VectorMind.RagEngine.Domain.Models;

namespace VectorMind.RagEngine.Domain.Interfaces;

public interface ITextChunker
{
    List<DocumentChunk> CreateChunks(string fullText, string documentName, int chunkSize = 500, int overlap = 50);
}