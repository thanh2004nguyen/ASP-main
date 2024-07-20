using static Group5.Service.MessageService;

namespace Group5.Service
{
    public interface  IMessageService
    {
        Task<string> CheckSpelling(string inputText);
        Task<string> CheckSpelling1(string inputText);
    }
   
}
