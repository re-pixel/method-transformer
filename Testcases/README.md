# Test Files

This directory contains illustrative test files for the Code Analysis Tool. These files are designed to test various scenarios that the tool might encounter when processing real-world C# code, particularly code similar to patterns found in the `dotnet/runtime` repository.

## Test Files

### CollectionsTest.cs
Tests collection-related operations:
- `Contains`, `Add`, `Remove` methods
- Index operations
- Array copying and clearing

### StringOperationsTest.cs
Tests string manipulation methods:
- Comparison operations (`CompareTo`, `StartsWith`, `EndsWith`)
- Transformation operations (`Replace`, `Trim`, `Split`)
- Encoding operations

### ArrayOperationsTest.cs
Tests array-specific operations:
- Searching and sorting
- Reversing and cloning
- Resizing and clearing

### StreamIOTest.cs
Tests stream and I/O operations:
- Reading and writing bytes
- Async operations
- Position and flushing

### NumericOperationsTest.cs
Tests mathematical operations:
- Absolute value, min/max
- Rounding and square root
- Trigonometric functions

### ValidationTest.cs
Tests validation and argument checking:
- Null checks
- Range validation
- Array validation
- Enum validation

### ConversionTest.cs
Tests type conversion methods:
- String parsing
- Type conversion
- Base64 encoding/decoding
- DateTime parsing

### ComparisonTest.cs
Tests comparison and equality:
- Equality checking
- Comparison operations
- Sequence equality
- Collection queries

### EdgeCasesTest.cs
Tests edge cases and special scenarios:
- Empty methods
- Async methods
- Generic methods
- Nullable parameters
- Methods with attributes
- Multiple statements using the parameter
- Lambda expressions

## Usage

To test the tool on these files:

```bash
# Test on a single file
dotnet run -- test/CollectionsTest.cs test/CollectionsTest.Output.cs

# Test with heuristic suggester
dotnet run -- test/StringOperationsTest.cs test/StringOperationsTest.Output.cs --suggester=heuristic

# Test with semantic suggester (default)
dotnet run -- test/ArrayOperationsTest.cs test/ArrayOperationsTest.Output.cs --suggester=semantic
```

## Expected Behavior

Each file contains methods with exactly one parameter. After transformation:
1. Each method should have its parameter duplicated
2. The duplicate should have a suggested name (heuristic: `name2`, `nameCopy`, etc., or semantic: contextually appropriate names)
3. Statements using the original parameter should be cloned to use the new parameter (if applicable)

## Notes

- The semantic suggester should provide more contextually appropriate names based on training data from `dotnet/runtime`.
- The heuristic suggester will provide simpler names like `value2`, `itemCopy`, etc.

