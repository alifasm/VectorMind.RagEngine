using System.IO;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using VectorMind.RagEngine.Domain.Interfaces;

namespace VectorMind.RagEngine.Infrastructure.Services
{
    public class PdfExtractor : IPdfExtractor
    {
        public Task<string> ExtractTextAsync(Stream pdfStream)
        {
            var sb = new StringBuilder();

            using (var document = PdfDocument.Open(pdfStream))
            {
                foreach (var page in document.GetPages())
                {
                    sb.AppendLine(page.Text);
                }
            }

            return Task.FromResult(sb.ToString());
        }
    }
}