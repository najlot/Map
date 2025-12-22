# ![M](https://github.com/najlot/Map/blob/main/images/icon.png) Najlot.Map

**Najlot.Map** is a lightweight, high-performance object-to-object mapping library for .NET.
It combines the simplicity of a central mapping container with the performance of handwritten mapping code.

The library is designed to:

* avoid reflection at runtime,
* keep mappings explicit and readable,
* scale from handwritten mappings to compile-time generated mappings.

---

## Features

* Explicit, method-based mappings
* Central `IMap` container
* Zero magic at runtime
* Optional **Roslyn Source Generator**
* Mapping validation for test environments
* Collection and nested object mapping
* Custom converters
* Dependency injection support

---

## Packages

Najlot.Map is distributed as two NuGet packages:

| Package                      | Description                     |
| ---------------------------- | ------------------------------- |
| `Najlot.Map`                 | Core mapping infrastructure     |
| `Najlot.Map.SourceGenerator` | Compile-time mapping generation |

---

## Installation

### Core library

```bash
dotnet add package Najlot.Map
```

### Source generator (optional)

```bash
dotnet add package Najlot.Map.SourceGenerator
```

---

## Core Concepts

* **Mappings are methods**, not configuration.
* **Only public methods are registered**.
* `IMap` is the single entry point for all mappings.
* Mappings can be validated during testing.

---

## Manual Mapping Example

You can write mapping methods manually without the source generator.

```csharp
internal class UserMapMethods
{
    [MapIgnoreProperty(nameof(User.Password))]
    public void MapModelToUser(IMap map, UserModel from, User to)
    {
        to.Id = from.Id;
        to.Username = from.Username;
        to.Features = map
            .From<UserFeatureModel>(from.Features)
            .ToList<UserFeature>();
    }

    [MapIgnoreProperty(nameof(UserModel.Password))]
    public UserModel MapUserToModel(IMap map, User from) => new()
    {
        Id = from.Id,
        Username = from.Username,
        Features = map
            .From<UserFeature>(from.Features)
            .To<UserFeatureModel>()
    };

    [MapIgnoreMethod]
    public Guid IgnoredMethod(UserModel from) => from.Id;
}
```

---

## Registering Mappings

### Manual Registration

```csharp
var map = new Map()
    .Register<UserMapMethods>()
    .RegisterFactory(type => IocContainer.Resolve(type));
```

### Auto-Registration (Source Generator)

The source generator creates an extension method `Register{AssemblyName}Mappings()` that registers all classes marked with `[Mapping]`.

```csharp
var map = new Map()
    .RegisterMyProjectMappings() // Assuming assembly name is "MyProject"
    .RegisterFactory(type => IocContainer.Resolve(type));
```

### Mapping Objects

```csharp
var model = map.From(user).To<UserModel>();
```

### Mapping Collections

```csharp
var models = map.From<User>(users).To<UserModel>();
```

---

## Mapping Validation (Tests Only)

`Validate()` compares destination properties with used mappings and throws when mappings are incomplete.

```csharp
map.Validate();
```

> ⚠️ Recommended **only in unit tests**, not in production.

---

## Source Generator

The source generator creates mapping implementations at **compile time**.

### Example

```csharp
using Najlot.Map;
using Najlot.Map.Attributes;

[Mapping]
public partial class UserMappings
{
    public partial void MapUser(IMap map, UserModel from, User to);
    public partial void MapFeature(IMap map, FeatureModel from, Feature to);

    private partial void PartialMapAddress(
        IMap map,
        AddressModel from,
        AddressViewModel to);

    public void MapAddress(
        IMap map,
        AddressModel from,
        AddressViewModel to)
    {
        // Custom logic before
        PartialMapAddress(map, from, to);
        // Custom logic after
    }

    // Custom converter (auto-detected)
    public DateTime ConvertToUtc(DateTimeOffset offset)
        => offset.UtcDateTime;
}
```

### Usage

```csharp
var map = new Map()
    // Registers all [Mapping] classes in the assembly
    .RegisterMyProjectMappings() 
    .RegisterFactory(type => /* your factory */);

var user = map.From(userModel).To<User>();
```

---

## When to Use the Source Generator

Use it if you want:

* maximum performance
* zero runtime overhead
* compile-time safety
* reduced boilerplate

Manual mappings remain fully supported and interoperable.

---

## License

MIT License

---

