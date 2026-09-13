using System.Collections.Generic;
using System.Threading.Tasks;
using VectorMind.RagEngine.Domain.Models;

namespace VectorMind.RagEngine.Domain.Interfaces
{
    public interface IRerankerService
    {
        Task<List<DocumentChunk>> RerankAsync(string question, List<DocumentChunk> candidates, int topN);
    }
}