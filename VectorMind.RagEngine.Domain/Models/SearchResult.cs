using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorMind.RagEngine.Domain.Models;

public record SearchResult(
    string DocumentName,
    string Text,
    float Score
);
