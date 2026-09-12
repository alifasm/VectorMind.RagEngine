using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using VectorMind.RagEngine.Domain.Interfaces;
using VectorMind.RagEngine.Domain.Models;

namespace VectorMind.RagEngine.Infrastructure.VectorDb
{
    public class QdrantVectorStore : IVectorStore
    {
        private readonly QdrantClient _client;
        private const string CollectionName = "pdf_documents";
        private const ulong VectorSize = 768;

        public QdrantVectorStore(string host, int port, string? apiKey = null)
        {
            _client = apiKey != null
                ? new QdrantClient(host, port, https: true, apiKey: apiKey)
                : new QdrantClient(host, port);
        }

        public async Task InitializeCollectionAsync()
        {
            var collections = await _client.ListCollectionsAsync();

            if (!collections.Contains(CollectionName))
            {
                await _client.CreateCollectionAsync(
                    CollectionName,
                    new VectorParams { Size = VectorSize, Distance = Distance.Cosine });
            }
        }

        public async Task SaveChunksAsync(List<DocumentChunk> chunks)
        {
            var points = chunks.Select(chunk => new PointStruct
            {
                Id = new PointId { Uuid = chunk.Id },
                Vectors = chunk.Embedding ?? throw new InvalidOperationException($"Chunk {chunk.Id} has no embedding."),
                Payload =
                {
                    ["documentName"] = chunk.DocumentName,
                    ["text"] = chunk.Text,
                    ["chunkIndex"] = chunk.ChunkIndex
                }
            }).ToList();

            await _client.UpsertAsync(CollectionName, points);
        }

        public async Task<List<DocumentChunk>> SearchSimilarAsync(float[] queryVector, int limit = 3)
        {
            var results = await _client.QueryAsync(
                CollectionName,
                queryVector,
                limit: (ulong)limit);

            return results.Select(r => new DocumentChunk(
                r.Id.Uuid,
                r.Payload["documentName"].StringValue,
                r.Payload["text"].StringValue,
                (int)r.Payload["chunkIndex"].IntegerValue,
                null
            )).ToList();
        }
    }
}