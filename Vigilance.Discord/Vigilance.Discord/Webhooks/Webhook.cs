using System.Net;
using System.Text;

namespace Vigilance.Discord.Webhooks
{
    public class Webhook
    {
        public string Url;
        public WebClient Client;

        public Webhook(string url)
        {
            Url = url;
            Client = new WebClient();
        }

        public void Send(string content)
        {
            Client.BaseAddress = Url;
            Client.UploadData(Url, Encoding.ASCII.GetBytes(content));
        }
    }
}