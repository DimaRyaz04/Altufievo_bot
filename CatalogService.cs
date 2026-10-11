
namespace PartsCatalogBot;

public sealed class CatalogService
{
    private readonly List<CatalogCategory> _categories = new()
    {
        new CatalogCategory
        {
            Id = "category_1",
            Name = "Категория 1"
        },
        new CatalogCategory
        {
            Id = "category_2",
            Name = "Категория 2"
        }
    };

    private readonly List<CatalogPart> _parts = new()
    {
        new CatalogPart
        {
            Id = "part_1",
            CategoryId = "category_1",
            Name = "Запчасть 1",
            Parameters = new List<ParameterDefinition>
            {
                new ParameterDefinition
                {
                    Id = "manufacturer",
                    Name = "Производитель",
                    InputType = "Choice",
                    Options = new List<string>
                    {
                        "Производитель A",
                        "Производитель B",
                        "Другой"
                    }
                },
                new ParameterDefinition
                {
                    Id = "model",
                    Name = "Модель",
                    InputType = "Text"
                }
            }
        },
        new CatalogPart
        {
            Id = "part_2",
            CategoryId = "category_1",
            Name = "Запчасть 2",
            Parameters = new List<ParameterDefinition>
            {
                new ParameterDefinition
                {
                    Id = "size",
                    Name = "Размер",
                    InputType = "Text"
                }
            }
        },
        new CatalogPart
        {
            Id = "part_3",
            CategoryId = "category_2",
            Name = "Запчасть 3"
        }
    };

    public IReadOnlyList<CatalogCategory> GetCategories()
    {
        return _categories;
    }

    public IReadOnlyList<CatalogPart> GetParts(string categoryId)
    {
        return _parts
            .Where(p => p.CategoryId == categoryId)
            .ToList();
    }

    public CatalogPart? GetPart(string partId)
    {
        return _parts.FirstOrDefault(p => p.Id == partId);
    }

    public CatalogCategory? GetCategory(string categoryId)
    {
        return _categories.FirstOrDefault(c => c.Id == categoryId);
    }
}
