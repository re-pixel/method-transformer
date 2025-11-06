using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using AllMiniLmL6V2Sharp;
using Pinecone;

namespace CodeAnalysisTool.NameSuggestion
{
   /// <summary>
   /// Helper class for deserializing context records from the JSON files.
   /// </summary>
   internal class JsonContextRecord
   {
       public string? transformerContext { get; set; }
       public string? paramType { get; set; }
       public string? paramName { get; set; }
   }

   internal class LocalEmbeddingSuggester : IDisposable
   {
       private readonly AllMiniLmL6V2Embedder _embedder;
       private readonly PineconeClient _pineconeClient;
       private readonly string _indexName;
       private const string NamespaceName = "code-contexts"; // Pinecone namespace for grouping

       /// <summary>
       /// Initializes the suggester and its embedding model.
       /// </summary>
       /// <param name="modelPath">Path to the ONNX embedding model.</param>
       /// <param name="pineconeApiKey">Your Pinecone API Key.</param>
       /// <param name="indexName">The name of the Pinecone index to use.</param>
       public LocalEmbeddingSuggester(string modelPath, string pineconeApiKey, string indexName)
       {
           _embedder = new AllMiniLmL6V2Embedder(modelPath: modelPath);
           _pineconeClient = new PineconeClient(pineconeApiKey);
           _indexName = indexName;
       }

       /// <summary>
       /// Checks if the Pinecone index is populated and loads data if it is not.
       /// </summary>
       /// <param name="contextsDirectory">The directory containing your "contexts*.json" files.</param>
       public async Task PopulateVectorBaseAsync(string contextsDirectory)
       {
           if (!Directory.Exists(contextsDirectory))
           {
               Console.WriteLine($"Error: Directory '{contextsDirectory}' does not exist.");
               return;
           }

           var index = _pineconeClient.Index(_indexName);

           Console.WriteLine($"Populating Pinecone index '{_indexName}'...");

           var contextFiles = Directory.EnumerateFiles(contextsDirectory, "contexts*.json", SearchOption.TopDirectoryOnly)
               .OrderBy(f => f)
               .ToList();

           if (!contextFiles.Any())
           {
               Console.WriteLine($"No context files found in '{contextsDirectory}' matching pattern 'contexts*.json'.");
               return;
           }

           Console.WriteLine($"Found {contextFiles.Count} context file(s) to process.");

           var allRecords = new List<JsonContextRecord>();
           var jsonOptions = new JsonSerializerOptions
           {
               PropertyNameCaseInsensitive = true,
               AllowTrailingCommas = true
           };

           foreach (var file in contextFiles)
           {
               try
               {
                   Console.WriteLine($"Reading {Path.GetFileName(file)}...");
                   var json = await File.ReadAllTextAsync(file);
                   var records = JsonSerializer.Deserialize<List<JsonContextRecord>>(json, jsonOptions);
                   if (records != null)
                   {
                       allRecords.AddRange(records);
                       Console.WriteLine($"  Loaded {records.Count} records from {Path.GetFileName(file)}");
                   }
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Failed to read or deserialize {file}: {ex.Message}");
               }
           }

           if (!allRecords.Any())
           {
               Console.WriteLine("No context records found to populate. Ensure files start with 'contexts' and are valid JSON.");
               return;
           }

           Console.WriteLine($"Total records loaded: {allRecords.Count}. Generating embeddings and preparing vectors...");

           var vectorsToUpsert = new List<Vector>();
           int processedCount = 0;
           int skippedCount = 0;

           foreach (var record in allRecords)
           {
               if (string.IsNullOrWhiteSpace(record.transformerContext))
               {
                   skippedCount++;
                   continue;
               }

               try
               {
                   float[] embedding = EmbedText(record.transformerContext);

                   var metadata = new Metadata
                   {
                       ["paramType"] = record.paramType ?? string.Empty,
                       ["paramName"] = record.paramName ?? string.Empty
                   };

                   vectorsToUpsert.Add(new Vector
                   {
                       Id = Guid.NewGuid().ToString(),
                       Values = new ReadOnlyMemory<float>(embedding),
                       Metadata = metadata
                   });

                   processedCount++;
                   if (processedCount % 1000 == 0)
                   {
                       Console.WriteLine($"  Processed {processedCount} records...");
                   }
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"  Error processing record: {ex.Message}");
                   skippedCount++;
               }
           }

           if (skippedCount > 0)
           {
               Console.WriteLine($"Skipped {skippedCount} records due to errors or empty contexts.");
           }

           if (!vectorsToUpsert.Any())
           {
               Console.WriteLine("No valid vectors to upsert.");
               return;
           }

           Console.WriteLine($"Prepared {vectorsToUpsert.Count} vectors. Upserting to Pinecone in batches...");

           const int batchSize = 100;
           int totalBatches = (int)Math.Ceiling((double)vectorsToUpsert.Count / batchSize);
           int batchNumber = 0;

           for (int i = 0; i < vectorsToUpsert.Count; i += batchSize)
           {
               var batch = vectorsToUpsert.Skip(i).Take(batchSize).ToArray();
               var upsertRequest = new UpsertRequest
               {
                   Vectors = batch,
                   Namespace = NamespaceName
               };

               try
               {
                   await index.UpsertAsync(upsertRequest);
                   batchNumber++;
                   Console.WriteLine($"  Upserted batch {batchNumber}/{totalBatches} ({batch.Length} vectors)");
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"  Error upserting batch {batchNumber + 1}: {ex.Message}");
               }
           }

           Console.WriteLine($"Successfully added {vectorsToUpsert.Count} embeddings to Pinecone index '{_indexName}' in namespace '{NamespaceName}'.");
       }

       /// <summary>
       /// Gets name suggestions by finding similar contexts in Pinecone, filtered by type.
       /// </summary>
       public async Task<List<string>> GetNameSuggestions(string context, string type, int count)
       {
           var index = _pineconeClient.Index(_indexName);

           float[] contextVec = EmbedText(context);

           var whereFilter = new Metadata
           {
               ["paramType"] = type
           };

           var queryRequest = new QueryRequest
           {
               Vector = new ReadOnlyMemory<float>(contextVec),
               TopK = (uint)count,
               Namespace = NamespaceName,
               IncludeMetadata = true,
               Filter = whereFilter
           };

           var queryResponse = await index.QueryAsync(queryRequest);

           if (queryResponse?.Matches == null)
           {
               return new List<string>();
           }

           return queryResponse.Matches
               .Where(m => m.Metadata != null && m.Metadata.ContainsKey("paramName"))
               .Select(m =>
               {
                   var metadata = m.Metadata;
                   if (metadata == null) return string.Empty;
                   var value = metadata["paramName"];
                   return value?.ToString() ?? string.Empty;
               })
               .Where(name => !string.IsNullOrEmpty(name))
               .Distinct()
               .ToList();
       }

       private float[] EmbedText(string text)
       {
           return (float[])_embedder.GenerateEmbedding(text);
       }

       public void Dispose() => _embedder.Dispose();
   }
}