using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorMind.RagEngine.Domain.Models;

public record DocumentChunk(
    string Id,
    string DocumentName,
    string Text,
    int ChunkIndex,
    float[]? Embedding = null
);
