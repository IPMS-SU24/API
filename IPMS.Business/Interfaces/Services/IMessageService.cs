using IPMS.Business.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface IMessageService
    {
        Task SendMessage<TMessage>(TMessage message) where TMessage : class;
    }
}
