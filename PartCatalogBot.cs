
using PartsCatalogBot;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace PartCatalogBot;

public sealed class PartCatalogBot
{
    private readonly ITelegramBotClient _botClient;
    private readonly BotService _botService;

    public PartCatalogBot(string token)
    {
        _botClient = new TelegramBotClient(token);

        var catalog = new CatalogService();
        var menu = new BotMenu();

        _botService = new BotService(catalog, menu);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var me = await _botClient.GetMe(
            cancellationToken: cancellationToken);

        Console.WriteLine($"Бот запущен: @{me.Username}");

        _botClient.StartReceiving(
            updateHandler: _botService.HandleUpdateAsync,
            errorHandler: _botService.HandleErrorAsync,
            receiverOptions: new ReceiverOptions(),
            cancellationToken: cancellationToken
        );
    }
}
