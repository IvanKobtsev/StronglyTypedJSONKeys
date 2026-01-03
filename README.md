# Strongly Typed JSON Keys

**Strongly Typed JSON Keys** is a .NET Roslyn source generator that reads JSON files at build time and generates static classes containing `const string` fields for each JSON key (including nested).

The goal is to eliminate hard-coded string keys when working with JSON-based data such as configuration files, localization resources, or structured payloads.

## Why use this?

Hard-coding JSON keys as strings can lead to:
- Typos that only fail at runtime;
- Painful refactors when keys change;
- Poor discoverability and IntelliSense support.

This source generator solves those problems by:
- Generating compile-time constants for JSON keys;
- Making key usage refactor-safe;
- Providing IntelliSense and compile-time checking.

## What it does

Given one or more JSON files, the generator:
- Parses their keys at compile time;
- Generates static classes with `const string` fields each containing the full key path;
- Keeps your code in sync with your JSON structure.

## How do I use it

Just include your JSON files in your project as `AdditionalFiles` like this:

```xml
<!-- ExampleProject.csproj -->

  <ItemGroup>
    <AdditionalFiles Include="json_files/example.json">
      <GenerateKeys>true</GenerateKeys>
      <ClassName>ExampleKeys</ClassName>
    </AdditionalFiles>
  </ItemGroup>
```

The **GenerateKeys tag is required** for the generator to process your JSON files.

### Options

You can configure the generation of each file to your liking with these options:

| Name                     | Description                                                                  | Type     |
|--------------------------|------------------------------------------------------------------------------|----------|
| `RootKeyPath`            | Limits key generation to a nested JSON object instead of the entire file.    | `string` |
| `KeysPrefix`             | Prefixes all generated keys with a specified value.                          | `string` |
| `ClassName`              | The name of the generated class. Defaults to the JSON file name.             | `string` |
| `CapitalizeGeneratedKeys`| Capitalizes generated field names to avoid conflicts with C# keywords.       | `bool`   |

License: MIT
