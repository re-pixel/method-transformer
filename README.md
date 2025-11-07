# Method Transformer

A C# code transformation tool that automatically duplicates single-parameter method declarations and generates intelligent parameter names using machine learning-powered semantic analysis and vector similarity search.

## Overview

This tool analyzes C# source files using Roslyn and automatically transforms methods with exactly one parameter by:
- Duplicating the single parameter
- Generating contextually-aware parameter names using either:
  - **Heuristic approach**: Simple rule-based naming (fast, no dependencies)
  - **Semantic approach**: ML-powered embeddings and Pinecone vector search 
- Ensuring uniqueness by avoiding naming collisions with existing identifiers
- Optionally cloning statements that use the original parameter

The semantic mode leverages **AllMiniLmL6V2** sentence transformer and **Pinecone** vector database to suggest semantically appropriate parameter names based on similar code patterns from a training dataset. The heuristic mode provides a lightweight alternative that works immediately without any setup.

## Features

- **Automatic Parameter Duplication**: Identifies and transforms methods with single parameters
- **Dual Suggestion Modes**: 
  - **Heuristic Mode**
  - **Semantic Mode**
- **ML-Powered Name Suggestions**: Uses semantic search to suggest contextually appropriate parameter names (semantic mode)
- **Collision Avoidance**: Automatically avoids naming conflicts with existing identifiers in method scope
- **Documentation-Aware**: Extracts and uses XML documentation comments for better context understanding (semantic mode)
- **Type-Aware Suggestions**: Filters suggestions based on parameter type for improved accuracy (semantic mode)
- **Statement Cloning**: Duplicates statements that reference the original parameter
- **Roslyn-Based**: Built on Microsoft's Roslyn compiler platform for accurate code analysis

## Project Structure

```
CodeAnalysisTool/
├── Core/
│   └── Models/
│       ├── FileProcessingOptions.cs      
│       └── ParameterDuplicationResult.cs 
├── ContextExtraction/
│   └── ContextExtractor.cs              # Utility for extracting training data from codebases
├── Infrastructure/
│   ├── CompilationService.cs            # Roslyn compilation and semantic model creation
│   ├── ICompilationService.cs           
│   ├── ConfigurationService.cs          
│   ├── IConfigurationService.cs         
│   ├── FileService.cs                   
│   └── IFileService.cs                  
├── NameSuggestion/
│   ├── HeuristicNameSuggester.cs        
│   ├── INameSuggester.cs                
│   ├── LocalEmbeddingSuggester.cs       # Embedding generation and Pinecone integration
│   └── MLNameSuggester.cs               
├── Rewriters/
│   ├── DocumentationExtractor.cs        # XML documentation extraction
│   ├── DuplicateSingleParameterRewriter.cs  # Main code transformation logic
│   ├── ParameterReplacer.cs             # Parameter name replacement in statements
│   └── SymbolCollector.cs               # Identifier collection for collision detection
├── Services/
│   ├── CodeTransformationService.cs     # Orchestrates transformation pipeline
│   ├── ICodeTransformationService.cs    
│   ├── MethodAnalysisService.cs         # Method metadata extraction and analysis
│   └── IMethodAnalysisService.cs        
├── CodeAnalysisTool.csproj              
├── CodeAnalysisTool.sln                 
└── Program.cs                           # Application entry point
```

## Prerequisites

- **.NET 8.0 SDK** or later
- **For Semantic Suggester (optional)**:
  - **Pinecone API Key**: Required for vector similarity search
  - **ONNX Model**: The `model/model.onnx` file containing AllMiniLmL6V2 sentence transformer
  - **Pinecone Index**: A Pinecone index named "code-contexts" populated with training data

## Installation

1. Clone the repository:
```bash
git clone https://github.com/re-pixel/method-transformer.git
cd method-transformer
```

2. Restore dependencies:
```bash
dotnet restore
```

3. (Optional) Configure environment variables for semantic suggester:
   Create a `.env` file in the project root:
```
PINECONE_API_KEY=your_pinecone_api_key_here
```

   **Note**: The heuristic suggester works without any configuration or external dependencies. Pinecone setup is only required if you want to use the semantic suggester.

4. (Optional) Ensure the embedding model is present (for semantic suggester):
   - Install `model.onnx`, `tokenizer.json`, and `vocab.txt` from https://huggingface.co/onnx-models/all-MiniLM-L6-v2-onnx/tree/main and place them in the `model/` directory

5. (Optional) Populate Pinecone index (first-time setup for semantic suggester):
   - Place training context files (`contexts_*.json`) in the `contexts/` directory
   - Use `LocalEmbeddingSuggester.PopulateVectorBaseAsync()` to upload embeddings to Pinecone

## Usage

### Command-Line Arguments

```
CodeAnalysisTool <input-file.cs> [output-file.cs] [--suggester=heuristic|semantic]
```

- `<input-file.cs>`: The input C# source file (required)
- `<output-file.cs>`: The output file path (optional). If not specified, the input file will be overwritten.
- `--suggester`: Choose the name suggestion algorithm (default: `semantic`):
  - `heuristic`: Simple rule-based naming (e.g., appends "2", "Copy", "_copy" to original parameter name)
  - `semantic`: ML-powered semantic search using embeddings and Pinecone (requires Pinecone setup)

### Basic Usage

Transform a C# file using semantic suggester (default, overwrites input file):
```bash
dotnet run -- <input-file.cs>
```

Transform a C# file with output to a new file using semantic suggester:
```bash
dotnet run -- <input-file.cs> <output-file.cs>
```

Use heuristic suggester (no Pinecone required):
```bash
dotnet run -- <input-file.cs> <output-file.cs> --suggester=heuristic
```

Use semantic suggester explicitly:
```bash
dotnet run -- <input-file.cs> <output-file.cs> --suggester=semantic
```

Arguments can be provided in any order:
```bash
dotnet run -- <input-file.cs> --suggester=heuristic <output-file.cs>
```

### Examples

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
**Note**: MLNameSuggester recognizes context.

**Input:**
```csharp
public void ProcessData(string name)
{
    // method body
}
```

**Output:**
```csharp
public void ProcessData(string name, string name2)
{
    // method body
}
```

**Input:**
```csharp
public void ProcessData(string name)
{
    int name2 = "";
    foreach(char c in name)
      Console.WriteLine(c);
}
```

**Output:**
```csharp
public void ProcessData(string name, string nameCopy)
{
    int name2 = "";
    foreach(char c in name)
      Console.WriteLine(c);
    foreach(char c in nameCopy)
      Console.WriteLine(c);
}
```
**Note**: Program recognizes potential collision if it decides to name the new parameter name2 so it goes for other options.

### Building Training Data

The semantic suggester in this project was trained on the [dotnet/runtime](https://github.com/dotnet/runtime) repository, which provides a rich source of well-written C# code patterns and naming conventions.

To create training contexts from a codebase:

```csharp
var extractor = new ContextExtractor(@"C:\path\to\codebase");
await extractor.RunExtraction();
```

This scans all `.cs` files, extracts method parameters with their contexts, and saves them as JSON files in the `contexts/` directory. The tool then generates embeddings for these contexts and uploads them to Pinecone for semantic similarity search.

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

The tool supports two suggestion modes:

**Heuristic Mode**:
- Applies simple naming patterns (e.g., `name2`, `nameCopy`, `name_copy`)
- Checks for collisions with existing identifiers
- Fast and requires no external dependencies

**Semantic Mode**:
- Generates embeddings for the method context using AllMiniLmL6V2
- Queries Pinecone vector database for similar parameter contexts
- Filters suggestions by parameter type and existing names
- Returns the most appropriate name suggestion based on cosine similarity

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
- **HeuristicNameSuggester**: Rule-based naming with collision avoidance
- **LocalEmbeddingSuggester**: Handles embedding generation and Pinecone queries (semantic mode)
- **MLNameSuggester**: Filters and ranks name suggestions from semantic search (semantic mode)

## Configuration

### Environment Variables (Semantic Suggester Only)
- `PINECONE_API_KEY`: Required for semantic suggester. Your Pinecone API key for vector database access.

### Pinecone Setup (Semantic Suggester Only)
1. Create a Pinecone index named `code-contexts`
2. Configure the index with appropriate dimensions (384 for AllMiniLmL6V2)
3. Populate the index using context JSON files

**Note**: The heuristic suggester requires no configuration and works immediately.

## Dependencies

- **Microsoft.CodeAnalysis.CSharp** (4.14.0): Roslyn compiler platform
- **AllMiniLmL6V2Sharp** (0.0.3): ONNX embedding model wrapper
- **Pinecone.Client** (4.0.2): Pinecone vector database client
- **DotNetEnv** (3.1.1): Environment variable management

## Limitations

- Only processes methods with exactly one parameter
- Semantic suggester requires pre-populated Pinecone index with training data
- Semantic suggestions depend on quality and relevance of training contexts
- Currently limited to C# language only
- Statement cloning is limited to first parameter usage
- Heuristic suggester provides simpler naming patterns compared to semantic approach

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

## Acknowledgments

- Built with [Roslyn](https://github.com/dotnet/roslyn)
- Uses [AllMiniLmL6V2](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2) sentence transformer
- Powered by [Pinecone](https://www.pinecone.io/) vector database
- Training data extracted from [dotnet/runtime](https://github.com/dotnet/runtime) repository
