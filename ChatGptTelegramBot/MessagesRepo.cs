using OpenAI.GPT3.ObjectModels.RequestModels;

namespace ChatGptTelegramBot;

public class MessagesRepo : IMessagesRepo
{
    private static readonly Dictionary<long, List<ChatMessage>> Messages = new();

    public List<ChatMessage> GetHistory(long chatId)
    {
        if (Messages.ContainsKey(chatId))
        {
            return Messages[chatId];
        }

        Messages.Add(chatId, new List<ChatMessage>());
        return Messages[chatId];
    }

    public void Save(ChatMessage message, long chatId)
    {
        if (!Messages.ContainsKey(chatId))
        {
            Messages.Add(chatId, new List<ChatMessage> { message });
        }
        else
        {
            Messages[chatId].Add(message);
        }
    }

    public ChatMessage RemoveLast(long chatId)
    {
        if (!Messages.ContainsKey(chatId))
        {
            Messages.Add(chatId, new List<ChatMessage>());
        }

        var last = Messages[chatId].Last();
        Messages[chatId].RemoveAt(Messages.Count - 1);
        return last;
    }

    public void RemoveAll(long chatId)
    {
        if (!Messages.ContainsKey(chatId)) return;
        Messages[chatId] = new List<ChatMessage>();
    }
}