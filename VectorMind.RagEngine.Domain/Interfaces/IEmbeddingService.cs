using System.Threading.Tasks;

namespace VectorMind.RagEngine.Domain.Interfaces;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text);
}