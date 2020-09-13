using System.Threading.Tasks;

namespace shopapp.webui.EMailServices
{
    public interface IEmailSender
    {
         Task SendEmailAsyc(string email,string subject,string htmlMessage);
    }
}