# Dynamic Method Serializer

> A C# code snippet that enables dynamic method invocation through serializable expressions.

## Overview

This code example allows capturing C# method calls as serializable DTOs that can be stored or transmitted, then executed later. It works by deconstructing lambda expressions into a JSON format containing the interface type, method name, and parameters.

## Features

- ✨ Serialize method calls to JSON
- 🔄 Support both void and value-returning methods
- 📦 Handle array parameters
- 🏭 Extensible factory pattern for implementations
- 🔍 Type-safe execution

## Installation

```bash
git clone https://github.com/yourusername/DynamicMethodSerializer.git
cd DynamicMethodSerializer
dotnet build
```

## Usage

Basic example:

```csharp
// Execute method with return value
var result = DynamicExecutor.Execute<IExample, int>(x => x.Add(5, 3));

// Execute void method
DynamicExecutor.Execute<IExample>(x => x.Print(\"Hello, World!\"));
```

Serialized output:

```json
{
  \"interfaceType\": \"IExample\",
  \"methodName\": \"Add\",
  \"arguments\": [
    {
      \"typeName\": \"System.Int32\",
      \"value\": 5
    },
    {
      \"typeName\": \"System.Int32\", 
      \"value\": 3
    }
  ]
}
```

## How It Works

1. Expression tree deconstruction
2. Serialization to DTO format
3. JSON serialization/deserialization
4. Dynamic method invocation via reflection

## Development

Requirements:
- .NET 6.0+
- Visual Studio 2022 or VS Code

## Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

## License

Distributed under the MIT License. See `LICENSE` for more information.

## Acknowledgements

This project was developed with significant assistance from GitHub Copilot. Most of the code and the content of this README were generated using Copilot, an AI programming assistant that provides code completions and suggestions. Leveraging Copilot accelerated the development process and showcases the potential of AI in software engineering.

## Contact

Your Name - [@dieron](https://github.com/dieron)

Project Link: [https://github.com/dieron/DynamicMethodSerializer](https://github.com/dieron/DynamicMethodSerializer)
```