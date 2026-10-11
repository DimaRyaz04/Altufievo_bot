
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace PartsCatalogBot;

public class BotMenu
{
    public InlineKeyboardMarkup MainMenu()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "🛠 Наш сервис", "service")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "🔎 Найти запчасть", "find_part")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "♻️ Купить Б/У технику", "used")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "📦 Старый каталог", "catalog")
            }
        });
    }

    public InlineKeyboardMarkup Equipment(bool used = false)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "🧺 Стиральные машины",
                    used ? "used_washing" : "washing")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "🍽 Посудомоечные машины",
                    used ? "used_dishwashers" : "dishwashers")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "🏠 Главная", "home")
            }
        });
    }

    public InlineKeyboardMarkup PartTypes()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("УБЛ", "parttype:УБЛ")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Люк", "parttype:Люк")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Модуль", "parttype:Модуль")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "Двигатель", "parttype:Двигатель")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "🏠 Главная", "home")
            }
        });
    }

    public InlineKeyboardMarkup BackToHome()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "🏠 Главная", "home")
            }
        });
    }

    public InlineKeyboardMarkup CreateKeyboard(
        IEnumerable<(string Text, string CallbackData)> buttons)
    {
        return new InlineKeyboardMarkup(
            buttons.Select(button => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    button.Text, button.CallbackData)
            }));
    }

    public InlineKeyboardMarkup Categories(
        IEnumerable<CatalogCategory> categories)
    {
        var rows = categories.Select(category => new[]
        {
            InlineKeyboardButton.WithCallbackData(
                category.Name, $"category:{category.Id}")
        }).ToList();

        rows.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData(
                "🏠 Главная", "home")
        });

        return new InlineKeyboardMarkup(rows);
    }

    public InlineKeyboardMarkup Parts(
        IEnumerable<CatalogPart> parts,
        string categoryId)
    {
        var rows = parts.Select(part => new[]
        {
            InlineKeyboardButton.WithCallbackData(
                part.Name, $"part:{part.Id}")
        }).ToList();

        rows.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData(
                "⬅️ Назад", $"category:{categoryId}")
        });

        rows.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData(
                "🏠 Главная", "home")
        });

        return new InlineKeyboardMarkup(rows);
    }
}
