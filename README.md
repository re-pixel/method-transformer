# Code Analysis Tool

A C# code transformation tool that automatically duplicates single-parameter method declarations and generates intelligent parameter names using machine learning-powered semantic analysis and vector similarity search.

## Overview

This tool analyzes C# source files using Roslyn and automatically transforms methods with exactly one parameter by:
- Duplicating the single parameter
- Generating contextually-aware parameter names using embeddings
- Ensuring uniqueness by avoiding naming collisions with existing identifiers
- Optionally cloning statements that use the original parameter

The tool leverages **AllMiniLmL6V2** embeddings and **Pinecone** vector database to suggest semantically appropriate parameter names based on similar code patterns from a training dataset.

## Features

- **Automatic Parameter Duplication**: Identifies and transforms methods with single parameters
- **ML-Powered Name Suggestions**: Uses semantic search to suggest contextually appropriate parameter names
- **Collision Avoidance**: Automatically avoids naming conflicts with existing identifiers in method scope
- **Documentation-Aware**: Extracts and uses XML documentation comments for better context understanding
- **Type-Aware Suggestions**: Filters suggestions based on parameter type for improved accuracy
- **Statement Cloning**: Can duplicate statements that reference the original parameter
- **Roslyn-Based**: Built on Microsoft's Roslyn compiler platform for accurate code analysis

## Project Structure

```
CodeAnalysisTool/
├── Core/
│   └── Models/
│       ├── FileProcessingOptions.cs      # Configuration for file processing
│       └── ParameterDuplicationResult.cs # Result of transformation operations
├── ContextExtraction/
│   └── ContextExtractor.cs               # Utility for extracting training data from codebases
├── Infrastructure/
│   ├── CompilationService.cs            # Roslyn compilation and semantic model creation
│   ├── ConfigurationService.cs          # Environment variable and API key management
│   ├── FileService.cs                   # File I/O operations
│   └── Interfaces/                      # Service abstractions
├── NameSuggestion/
│   ├── LocalEmbeddingSuggester.cs       # Embedding generation and Pinecone integration
│   ├── MLNameSuggester.cs               # ML-based name suggestion logic
│   └── INameSuggester.cs                # Name suggestion interface
├── Rewriters/
│   ├── DuplicateSingleParameterRewriter.cs  # Main code transformation logic
│   ├── DocumentationExtractor.cs            # XML documentation extraction
│   ├── ParameterReplacer.cs                 # Parameter name replacement in statements
│   └── SymbolCollector.cs                   # Identifier collection for collision detection
├── Services/
│   ├── CodeTransformationService.cs     # Orchestrates transformation pipeline
│   ├── MethodAnalysisService.cs         # Method metadata extraction and analysis
│   └── Interfaces/                      # Service abstractions
├── model/                               # ONNX embedding model files
├── contexts/                            # Training data (JSON context files)
└── Program.cs                           # Application entry point
```

## Prerequisites

- **.NET 8.0 SDK** or later
- **Pinecone API Key**: Required for vector similarity search
- **ONNX Model**: The `model/model.onnx` file (AllMiniLmL6V2 embeddings)
- **Pinecone Index**: A Pinecone index named "code-contexts" populated with training data

## Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/CodeAnalysisTool.git
cd CodeAnalysisTool
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Configure environment variables:
   Create a `.env` file in the project root:
   ```
   PINECONE_API_KEY=your_pinecone_api_key_here
   ```

4. Ensure the embedding model is present:
   - Install `model.onnx`, `tokenizer.json`, and `vocab.txt` from https://huggingface.co/onnx-models/all-MiniLM-L6-v2-onnx/tree/main and place them in the `model/` directory

5. Populate Pinecone index (first-time setup):
   - Place training context files (`contexts_*.json`) in the `contexts/` directory
   - Use `LocalEmbeddingSuggester.PopulateVectorBaseAsync()` to upload embeddings to Pinecone

## Usage

### Basic Usage

Transform a C# file (overwrites input file):
```bash
dotnet run -- <input-file.cs>
```

Transform a C# file with output to a new file:
```bash
dotnet run -- <input-file.cs> <output-file.cs>
```

### Example

**Input** (`TestFile.cs`):
```csharp
class TestClass
{
    /// <summary>
    /// This method calculates distance between two points.
    /// </summary>
    void Distance(int start)
    {
        start += 1;
    }
}
```

**Output** (after transformation):
```csharp
class TestClass
{
    /// <summary>
    /// This method calculates distance between two points.
    /// </summary>
    void Distance(int start, int end)
    {
        start += 1;
        end += 1;
    }
}
```

### Building Training Data

To create training contexts from a codebase:

```csharp
var extractor = new ContextExtractor(@"C:\path\to\codebase");
await extractor.RunExtraction();
```

This scans all `.cs` files, extracts method parameters with their contexts, and saves them as JSON files in the `contexts/` directory.

## How It Works

### 1. Code Analysis
- Uses Roslyn to parse and analyze C# source code
- Creates a semantic model for type information and symbol resolution
- Identifies methods with exactly one parameter

### 2. Context Extraction
For each target method, the tool extracts:
- XML documentation comments (summary)
- Method name
- Parameter type information
- Existing identifiers in method scope (to avoid collisions)

### 3. Name Suggestion
- Generates embeddings for the method context using AllMiniLmL6V2
- Queries Pinecone vector database for similar parameter contexts
- Filters suggestions by parameter type and existing names
- Returns the most appropriate name suggestion

### 4. Code Transformation
- Duplicates the parameter with the suggested name
- Uses syntax rewriters to clone statements that reference the original parameter
- Preserves code formatting and trivia

## Architecture

### Service Layer
- **CodeTransformationService**: Orchestrates the transformation pipeline
- **MethodAnalysisService**: Extracts method metadata and builds contexts
- **CompilationService**: Manages Roslyn compilation and semantic models

### Rewriters
- **DuplicateSingleParameterRewriter**: Main transformation logic using CSharpSyntaxRewriter
- **ParameterReplacer**: Replaces parameter references in statements
- **DocumentationExtractor**: Extracts XML documentation comments

### Name Suggestion
- **LocalEmbeddingSuggester**: Handles embedding generation and Pinecone queries
- **MLNameSuggester**: Filters and ranks name suggestions

## Configuration

### Environment Variables
- `PINECONE_API_KEY`: Required. Your Pinecone API key for vector database access.

### Pinecone Setup
1. Create a Pinecone index named `code-contexts`
2. Configure the index with appropriate dimensions (384 for AllMiniLmL6V2)
3. Populate the index using context JSON files

## Dependencies

- **Microsoft.CodeAnalysis.CSharp** (4.14.0): Roslyn compiler platform
- **AllMiniLmL6V2Sharp** (0.0.3): ONNX embedding model wrapper
- **Pinecone.Client** (4.0.2): Pinecone vector database client
- **DotNetEnv** (3.1.1): Environment variable management

## Limitations

- Only processes methods with exactly one parameter
- Requires pre-populated Pinecone index with training data
- Suggestions depend on quality and relevance of training contexts
- Currently limited to C# language only
- Statement cloning is limited to first parameter usage

## Future Improvements

### Short-term
- **Multi-parameter Support**: Extend to methods with multiple parameters
- **Configuration File**: Add JSON/YAML configuration for customization
- **Batch Processing**: Support for processing multiple files or directories
- **Progress Reporting**: Enhanced progress indicators for large codebases
- **Error Recovery**: Better error handling and recovery mechanisms

### Medium-term
- **Multiple Language Support**: Extend to other .NET languages (VB.NET, F#)
- **Custom Naming Strategies**: Allow users to define custom naming patterns
- **IDE Integration**: Visual Studio/VSCode extension for real-time suggestions
- **Dry-run Mode**: Preview changes without modifying files
- **Rollback Functionality**: Ability to undo transformations

### Long-term
- **Fine-tuning Capabilities**: Allow training on project-specific codebases
- **Quality Metrics**: Provide confidence scores for suggestions
- **Interactive Mode**: CLI interface for reviewing and selecting suggestions
- **Git Integration**: Automatic commit creation with transformation results
- **Performance Optimization**: Parallel processing and caching improvements
- **Additional Transformations**: Support for other code refactoring patterns
- **Unit Test Generation**: Generate tests for transformed methods
- **Documentation Updates**: Automatically update XML documentation

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Acknowledgments

- Built with [Roslyn](https://github.com/dotnet/roslyn)
- Uses [AllMiniLmL6V2](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2) embeddings
- Powered by [Pinecone](https://www.pinecone.io/) vector database
