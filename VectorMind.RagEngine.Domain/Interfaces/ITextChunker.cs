using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VectorMind.RagEngine.Domain.Models;

namespace VectorMind.RagEngine.Domain.Interfaces;

public interface ITextChunker
{
    List CreateChunks(string fullText, string documentName, int chunkSize = 500, int overlap = 50);
}
