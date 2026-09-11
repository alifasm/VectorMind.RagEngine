using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VectorMind.RagEngine.Domain.Models;

namespace VectorMind.RagEngine.Domain.Interfaces;

public interface IVectorStore
{
    Task InitializeCollectionAsync();
    Task SaveChunksAsync(List chunks);
    Task> SearchSimilarAsync(float[] queryVector, int limit = 3);
}
