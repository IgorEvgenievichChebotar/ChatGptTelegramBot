using OpenAI.GPT3.ObjectModels.RequestModels;

namespace ChatGptTelegramBot;

public interface IMessagesRepo
{
    List<ChatMessage> GetHistory(long chatId);
    void Save(ChatMessage message, long chatId);
    ChatMessage RemoveLast(long chatId);
    void RemoveAll(long chatId);
}