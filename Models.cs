
namespace PartsCatalogBot;

public sealed class CatalogCategory
{
    public required string Id { get; init; }
    public required string Name { get; init; }
}

public sealed class CatalogPart
{
    public required string Id { get; init; }
    public required string CategoryId { get; init; }
    public required string Name { get; init; }

    public List<ParameterDefinition> Parameters { get; init; } = new();
}

public sealed class ParameterDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }

    // Text — ввод текстом.
    // Choice — выбор одного из вариантов.
    public string InputType { get; init; } = "Text";

    public List<string> Options { get; init; } = new();
}

public enum UserStep
{
    MainMenu,
    ChoosingPart,
    EnteringParameters,
    Completed
}
