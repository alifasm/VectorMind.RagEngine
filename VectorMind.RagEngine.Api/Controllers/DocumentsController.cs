using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VectorMind.RagEngine.Domain.Interfaces;
using VectorMind.RagEngine.Domain.Models;

namespace VectorMind.RagEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IPdfExtractor _pdfExtractor;
    private readonly ITextChunker _textChunker;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;

    public DocumentsController(
        IPdfExtractor pdfExtractor,
        ITextChunker textChunker,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore)
    {
        _pdfExtractor = pdfExtractor;
        _textChunker = textChunker;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadPdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Please upload a valid PDF file.");

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only PDF files are supported.");

        using var stream = file.OpenReadStream();

        // 1. Extract plain text from PDF
        var rawText = await _pdfExtractor.ExtractTextAsync(stream);

        // 2. Slice text into overlapping chunks
        var chunks = _textChunker.CreateChunks(rawText, file.FileName);

        // 3. Generate embeddings for each chunk via Gemini API
        var embeddedChunks = new List<DocumentChunk>();
        foreach (var chunk in chunks)
        {
            var vector = await _embeddingService.GenerateEmbeddingAsync(chunk.Text);
            embeddedChunks.Add(chunk with { Embedding = vector });
        }

        // 4. Save vectors & payloads to Qdrant Vector DB
        await _vectorStore.SaveChunksAsync(embeddedChunks);

        return Ok(new
        {
            Message = "PDF processed and indexed successfully.",
            DocumentName = file.FileName,
            TotalChunksIndexed = embeddedChunks.Count
        });
    }

    [HttpGet("query")]
    public async Task<IActionResult> QueryDocument([FromQuery] string question)
    {
        if (string.IsNullOrWhiteSpace(question))
            return BadRequest("Query question cannot be empty.");

        // 1. Convert question into a 768-dim vector
        var queryVector = await _embeddingService.GenerateEmbeddingAsync(question);

        // 2. Execute similarity search in Qdrant
        var results = await _vectorStore.SearchSimilarAsync(queryVector, limit: 3);

        return Ok(results);
    }
}