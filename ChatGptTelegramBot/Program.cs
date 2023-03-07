using System.Text;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatGptTelegramBot;

class Program
{
    private static readonly TelegramBotClient _botClient = new(Secrets.BotToken);

    private static readonly OpenAIService _aiService = new(new OpenAiOptions
    {
        ApiKey = Secrets.OpenAiToken,
        DefaultModelId = Models.ChatGpt3_5Turbo
    });

    private static List<ChatMessage> messages = new();

    static void Main(string[] args)
    {
        using CancellationTokenSource cts = new();

        async void UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message is null) return;

                    var msg = update.Message;
                    var chatId = msg.Chat.Id;

                    if (msg.Type == MessageType.Document)
                    {
                        var document = msg.Document!;
                        if (document.MimeType is not "text/plain") return;

                        using var stream = new MemoryStream();

                        await bot.GetInfoAndDownloadFileAsync(
                            fileId: document.FileId,
                            destination: stream,
                            cancellationToken: token);

                        var text = Encoding.Default.GetString(stream.ToArray());
                        if (text.Length == 0)
                        {
                            await bot.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Текста в файле нет",
                                cancellationToken: token);
                            return;
                        }

                        await AskAsync(bot, chatId, token, text);
                        return;
                    }

                    if (msg.Text is null) return;

                    var parts = msg.Text.Split(' ', 2);
                    var cmd = parts[0];
                    var query = parts.Length > 1 ? parts[1] : string.Empty;

                    switch (cmd)
                    {
                        case "/start":
                            await bot.SendTextMessageAsync(
                                chatId: chatId,
                                text: "API chatGPT. " +
                                      "Суть бота - принимать на вход много текста непосредственно или в файле формата .txt, " +
                                      "который не помещается на их сайте из-за ограничений, и получать быстрый ответ. " +
                                      "Ну или просто пользоваться как обычно.",
                                parseMode: ParseMode.Html,
                                cancellationToken: token);
                            await bot.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Пиши текстом или прикрепляй сюда текстовый файл, " +
                                      "бот из него извлечёт текст и отправит chatGTP. " +
                                      "Ответ ты получишь здесь, в чате.",
                                cancellationToken: token);
                            return;
                        case "/newchat":
                            messages = new List<ChatMessage>();
                            await bot.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Контекст переписки удалён.",
                                cancellationToken: token);
                            return;
                        default:
                            var text = cmd + " " + query;
                            if (text.Length <= 0)
                            {
                                await bot.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Вопрос не задан - пустой запрос.",
                                    cancellationToken: token
                                );
                                return;
                            }

                            await AskAsync(bot, chatId, token, text);
                            return;
                    }
            }
        }

        void PollingErrorHandler(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
        }

        _botClient.StartReceiving(
            updateHandler: UpdateHandler,
            pollingErrorHandler: PollingErrorHandler,
            receiverOptions: new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
            cancellationToken: cts.Token
        );

        Console.ReadLine();
        cts.Cancel();
    }

    private static async Task AskAsync(ITelegramBotClient bot, long chatId, CancellationToken token, string query)
    {
        await bot.SendChatActionAsync(
            chatId: chatId,
            chatAction: ChatAction.Typing,
            cancellationToken: token);
        var answer = AnswerAsync(query);
        await bot.SendTextMessageAsync(
            chatId: chatId,
            text: await answer,
            cancellationToken: token
        );
    }

    private static async Task<string> AnswerAsync(string question)
    {
        messages.Add(ChatMessage.FromUser(question));
        var response = await _aiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = messages
        });
        var answer = response.Choices[0].Message.Content;
        return answer;
    }
}