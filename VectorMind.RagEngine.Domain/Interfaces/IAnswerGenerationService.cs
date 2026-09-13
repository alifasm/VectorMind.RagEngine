using System.Collections.Generic;
using System.Threading.Tasks;
using VectorMind.RagEngine.Domain.Models;

namespace VectorMind.RagEngine.Domain.Interfaces
{
    public interface IAnswerGenerationService
    {
        Task<string> GenerateAnswerAsync(string question, List<DocumentChunk> context);
    }
}