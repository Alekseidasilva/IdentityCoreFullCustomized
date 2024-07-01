using IdentityCoreFullCustomized.Service.Models;

namespace IdentityCoreFullCustomized.Service.Services;

public interface IEmailService
{
    void SendEmail(Message message);
}