# Method Transformer

A minimal C# code transforming tool that uses Roslyn (Microsoft.CodeAnalysis) to automatically duplicate single-parameter methods in C# source files.

## What it does

This tool processes C# source files and finds all method declarations that have exactly one parameter. For each such method, it duplicates that parameter and renames the duplicate by appending "2" to the original parameter name.

## Usage

```
SyntaxTreeManualTraversal <input-file.cs> [output-file.cs]
```

- **input-file.cs**: The path to the input C# source file (required)
- **output-file.cs**: The path where the modified code will be written (optional). If not specified, the input file will be overwritten.

## Example

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

## Requirements

- .NET SDK
- Microsoft.CodeAnalysis NuGet packages (CSharp)
