using System.Collections.Generic;
using System.Threading.Tasks;
using VectorMind.RagEngine.Domain.Models;

namespace VectorMind.RagEngine.Domain.Interfaces;

public interface IVectorStore
{
    Task InitializeCollectionAsync();

    Task SaveChunksAsync(List<DocumentChunk> chunks);

    Task<List<DocumentChunk>> SearchSimilarAsync(float[] queryVector, int limit = 3);
}