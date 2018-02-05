using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Bot.CognitiveServices.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Connector;

namespace Bot.CognitiveServices.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            var attributes = new LuisModelAttribute(
                ConfigurationManager.AppSettings["LuisId"],
                ConfigurationManager.AppSettings["LuisSubscriptionKey"]);
            var service = new LuisService(attributes);

            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    await Conversation.SendAsync(activity, () => new Licao2Dialog(service));
                    break;
                case ActivityTypes.ConversationUpdate:
                    if (activity.MembersAdded.Any(o => o.Id == activity.Recipient.Id))
                    {
                        var reply = activity.CreateReply();
                        reply.Text = "Olá, eu sou o **Bot Inteligentão**. Curte ai o que eu posso fazer:\n" +
                                     "* **Falar que nem gente**\n" +
                                     "* **Descrever imagens**\n" +
                                     "* **Reconhecer emoções**\n" +
                                     "* **Classificar objetos**\n" +
                                     "* **Traduzir textos**\n" +
                                     "* **Recomendar produtos por item**\n" +
                                     "* **Recomendar produtos para um determinado perfil**";

                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    break;
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}