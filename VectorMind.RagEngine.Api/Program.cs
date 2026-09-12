using VectorMind.RagEngine.Domain.Interfaces;
using VectorMind.RagEngine.Infrastructure.Services;
using VectorMind.RagEngine.Infrastructure.VectorDb;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register HttpClient for Gemini API
builder.Services.AddHttpClient();

// Register Domain & Infrastructure Services (Clean Architecture DI)
builder.Services.AddSingleton<IPdfExtractor, PdfExtractor>();
builder.Services.AddSingleton<ITextChunker, TextChunker>();

builder.Services.AddSingleton<IEmbeddingService>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var apiKey = builder.Configuration["Gemini:ApiKey"]
        ?? throw new InvalidOperationException("Gemini:ApiKey is missing from configuration.");
    return new GeminiEmbeddingService(httpClient, apiKey);
});

// Register Qdrant Vector Store
var qdrantHost = builder.Configuration["Qdrant:Host"] ?? "localhost";
var qdrantPort = int.Parse(builder.Configuration["Qdrant:Port"] ?? "6334");
builder.Services.AddSingleton<IVectorStore>(sp => new QdrantVectorStore(qdrantHost, qdrantPort));

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure Qdrant collection exists on startup
using (var scope = app.Services.CreateScope())
{
    var vectorStore = scope.ServiceProvider.GetRequiredService<IVectorStore>();
    await vectorStore.InitializeCollectionAsync();
}

app.Run();