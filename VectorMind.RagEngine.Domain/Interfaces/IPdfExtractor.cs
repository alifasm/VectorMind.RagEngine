using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorMind.RagEngine.Domain.Interfaces;

public interface IPdfExtractor
{
    Task<string> ExtractTextAsync(Stream pdfStream);
}
