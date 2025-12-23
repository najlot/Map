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

## Expression-based mappings (LINQ projections)

When mapping inside `IQueryable<T>` (for example with Entity Framework Core), you often need an `Expression<Func<TFrom, TTo>>` so the provider can translate the projection to SQL.

Najlot.Map supports registering and using **expression mappings** in addition to delegate-based mappings.

### Example

```csharp
using System;
using System.Linq;
using System.Linq.Expressions;
using Najlot.Map.Attributes;

public sealed class UserModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class UserListItem
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int AgeDays { get; set; }
}

[Mapping]
public partial class UserMappings
{
    // Generated as an Expression<Func<UserModel, UserListItem>>
    // Suitable for IQueryable projections.
    public static partial Expression<Func<UserModel, UserListItem>> ToListItem();
}
```

Usage with `IQueryable<T>`:

```csharp
var map = new Map()
    .RegisterMyProjectMappings();

// EF Core example: projection happens in SQL.
var query = map.From(db.Users).To<UserListItem>();

var items = await query.ToListAsync();
```

### Notes and limitations

- `Expression` mappings are intended for **queryable projections** (e.g. EF Core). For in-memory objects, delegate mappings (`void Map(IMap map, TFrom from, TTo to)`) are usually the better default.
- `Map.Validate()` **does not validate expression mappings**. Validation focuses on delegate-based mappings where the library can reliably track which properties are assigned.
- For complex projections, expression trees can become harder to generate correctly.
  In those cases it’s often best to **hand-write the expression** even if you use generated mappings elsewhere.

  Practical reasons you might hand-write an expression:
  - provider translation quirks (what EF Core can/can’t translate)
  - computed values that need to stay server-side
  - conditional projections, joins, groupings, or subqueries
  - performance tuning of the generated SQL

---

## License

MIT License

---

