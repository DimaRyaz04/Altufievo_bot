

using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PartsCatalogBot;

public class BotService
{
    private readonly CatalogService _catalog;
    private readonly BotMenu _menu;

    private readonly ConcurrentDictionary<long, FlowState> _flows = new();

    private sealed class FlowState
    {
        public string Step { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Model { get; set; } = "";
    }

    public BotService(CatalogService catalog, BotMenu menu)
    {
        _catalog = catalog;
        _menu = menu;
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message &&
            update.Message is { Text: not null } message)
        {
            long chatId = message.Chat.Id;
            string input = message.Text.Trim();

            if (input == "/start" || input == "/menu")
            {
                _flows.TryRemove(chatId, out _);

                await SendScreenAsync(
                    botClient,
                    chatId,
                    "Здравствуйте! 👋\n\nДобро пожаловать в наш сервис. Выберите нужный раздел:",
                    _menu.MainMenu(),
                    null,
                    cancellationToken);

                return;
            }

            if (input == "/cancel")
            {
                _flows.TryRemove(chatId, out _);

                await SendScreenAsync(
                    botClient,
                    chatId,
                    "Действие отменено. Выберите раздел:",
                    _menu.MainMenu(),
                    null,
                    cancellationToken);

                return;
            }

            if (!_flows.TryGetValue(chatId, out var flow))
                return;

            if (flow.Step == "brand")
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "Введите бренд стиральной машины.",
                        cancellationToken: cancellationToken);
                    return;
                }

                flow.Brand = input;
                flow.Step = "model";

                await SendScreenAsync(
                    botClient,
                    chatId,
                    $"Бренд: {flow.Brand}\n\nВведите марку или модель стиральной машины:",
                    _menu.BackToHome(),
                    "washing.jpg",
                    cancellationToken);

                return;
            }

            if (flow.Step == "model")
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "Введите марку или модель стиральной машины.",
                        cancellationToken: cancellationToken);
                    return;
                }

                flow.Model = input;
                flow.Step = "part";

                await SendScreenAsync(
                    botClient,
                    chatId,
                    $"Стиральная машина\nБренд: {flow.Brand}\nМарка: {flow.Model}\n\nКакую запчасть ищете?",
                    _menu.PartTypes(),
                    "washing.jpg",
                    cancellationToken);

                return;
            }

            return;
        }

        if (update.Type != UpdateType.CallbackQuery ||
            update.CallbackQuery is not { } callback)
        {
            return;
        }

        await botClient.AnswerCallbackQuery(
            callbackQueryId: callback.Id,
            cancellationToken: cancellationToken);

        if (callback.Message is not { } callbackMessage)
            return;

        long chatIdFromButton = callbackMessage.Chat.Id;
        int messageId = callbackMessage.MessageId;
        string data = callback.Data ?? "";

        string text;
        InlineKeyboardMarkup keyboard;
        string? photo = null;

        if (data == "home")
        {
            _flows.TryRemove(chatIdFromButton, out _);

            text = "Здравствуйте! 👋\n\nДобро пожаловать в наш сервис. Выберите нужный раздел:";
            keyboard = _menu.MainMenu();
        }
        else if (data == "service")
        {
            text = "🛠 Наш сервис\n\nПомогаем с ремонтом бытовой техники и подбором необходимых запчастей.\n\nВыберите нужный раздел в меню.";
            keyboard = _menu.BackToHome();
            photo = "service.jpg";
        }
        else if (data == "find_part")
        {
            text = "🔎 Найти запчасть\n\nВыберите тип техники:";
            keyboard = _menu.Equipment();
            photo = "equipment.jpg";
        }
        else if (data == "used")
        {
            text = "♻️ Купить Б/У технику\n\nВыберите тип техники:";
            keyboard = _menu.Equipment(used: true);
            photo = "used.jpg";
        }
        else if (data == "washing")
        {
            _flows[chatIdFromButton] = new FlowState
            {
                Step = "brand"
            };

            text = "🧺 Поиск запчасти для стиральной машины\n\nВведите бренд стиральной машины, например LG, Samsung или Bosch.";
            keyboard = _menu.BackToHome();
            photo = "washing.jpg";
        }
        else if (data == "dishwashers" ||
                 data == "used_washing" ||
                 data == "used_dishwashers")
        {
            text = "🚧 Этот раздел находится в разработке.";
            keyboard = _menu.BackToHome();
        }
        else if (data == "catalog")
        {
            var categories = _catalog.GetCategories();

            text = categories.Count == 0
                ? "Категорий пока нет."
                : "📦 Старый каталог\n\nВыберите категорию:";

            keyboard = _menu.Categories(categories);
        }
        else if (data.StartsWith("category:"))
        {
            string categoryId = data["category:".Length..];
            var parts = _catalog.GetParts(categoryId);

            text = parts.Count == 0
                ? "В этой категории пока нет запчастей."
                : "Выберите запчасть:";

            keyboard = _menu.Parts(parts, categoryId);
        }
        else if (data.StartsWith("parttype:"))
        {
            if (!_flows.TryGetValue(chatIdFromButton, out var flow) ||
                flow.Step != "part")
            {
                text = "Сначала выберите стиральную машину и укажите её бренд и марку.";
                keyboard = _menu.MainMenu();
            }
            else
            {
                string partType = data["parttype:".Length..];

                text =
                    $"✅ Данные для поиска\n\n" +
                    $"Бренд: {flow.Brand}\n" +
                    $"Марка: {flow.Model}\n" +
                    $"Запчасть: {partType}\n\n" +
                    "Следующий этап — подключение базы данных.";

                keyboard = _menu.BackToHome();
            }
        }
        else if (data.StartsWith("part:"))
        {
            string partId = data["part:".Length..];
            var part = _catalog.GetPart(partId);

            if (part == null)
            {
                text = "Запчасть не найдена.";
                keyboard = _menu.MainMenu();
            }
            else
            {
                text = $"📦 {part.Name}";
                keyboard = _menu.CreateKeyboard(new[]
                {
                    ("⬅️ Назад", $"category:{part.CategoryId}"),
                    ("🏠 Главная", "home")
                });
            }
        }
        else
        {
            text = "Действие не найдено. Вернитесь в меню.";
            keyboard = _menu.MainMenu();
        }

        try
        {
            await botClient.DeleteMessage(
                chatId: chatIdFromButton,
                messageId: messageId,
                cancellationToken: cancellationToken);
        }
        catch (ApiRequestException)
        {
            // Если удалить старое сообщение нельзя, отправим новое.
        }

        await SendScreenAsync(
            botClient,
            chatIdFromButton,
            text,
            keyboard,
            photo,
            cancellationToken);
    }

    private static async Task SendScreenAsync(
        ITelegramBotClient botClient,
        long chatId,
        string text,
        InlineKeyboardMarkup keyboard,
        string? photoName,
        CancellationToken cancellationToken)
    {
        string? path = photoName == null
            ? null
            : Path.Combine(AppContext.BaseDirectory, photoName);

        if (path != null && File.Exists(path))
        {
            await using var stream = File.OpenRead(path);

            await botClient.SendPhoto(
                chatId: chatId,
                photo: InputFile.FromStream(stream, photoName!),
                caption: text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
    }

    public Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка Telegram: {exception.Message}");
        return Task.CompletedTask;
    }
}
