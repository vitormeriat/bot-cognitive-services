using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.CognitiveServices.Dialogs
{
    [Serializable]
    public class Licao3Dialog : LuisDialog<object>
    {
        private enum TipoDeProcessamento { Emocoes, Descricao, Classificacao }

        public Licao3Dialog(ILuisService service) : base(service) { }

        /// <summary>
        /// Caso a intenção não seja reconhecida.
        /// </summary>
        [LuisIntent("None")]
        public async Task NoneAsync(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Desculpe, eu não entendi...\n" +
                                    "Lembre-se que sou um bot e meu conhecimento é limitado.");
            context.Done<string>(null);
        }

        /// <summary>
        /// Quando não houve intenção reconhecida.
        /// </summary>
        [LuisIntent("")]
        public async Task IntencaoNaoReconhecida(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("**( ͡° ͜ʖ ͡°)** - Desculpe, mas não entendi o que você quis dizer.\n" +
                                    "Lembre-se que sou um bot e meu conhecimento é limitado.");
            context.Done<string>(null);
        }

        /// <summary>
        /// Caso a intenção não seja reconhecida.
        /// </summary>
        [LuisIntent("consciencia")]
        public async Task ConscienciaAsync(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("**(▀̿Ĺ̯▀̿ ̿)** - Eu sou famoso **Bot Inteligentão**\nFalo vários idiomas e reconheço padrões...");
            context.Done<string>(null);
        }

        /// <summary>
        /// Quando a intenção for por ajuda.
        /// </summary>
        [LuisIntent("ajudar")]
        public async Task AjudarAsync(IDialogContext context, LuisResult result)
        {
            var response = "Não se esqueça que eu sou um **Bot** e minha conversação é limitada. Olha ai o que eu consigo fazer:\n" +
                       "* **Falar que nem gente**\n" +
                       "* **Descrever imagens**\n" +
                       "* **Reconhecer emoções**\n" +
                       "* **Classificar objetos**\n" +
                       "* **Traduzir textos**\n" +
                       "* **Recomendar produtos por item**\n" +
                       "* **Recomendar produtos para um determinado perfil**";
            await context.PostAsync(response);
            context.Done<string>(null);
        }

        /// <summary>
        /// Quando a intenção for uma saudação.
        /// </summary>
        [LuisIntent("saudar")]
        public async Task Saudar(IDialogContext context, LuisResult result)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).TimeOfDay;
            string saudacao;

            if (now < TimeSpan.FromHours(12)) saudacao = "Bom dia";
            else if (now < TimeSpan.FromHours(18)) saudacao = "Boa tarde";
            else saudacao = "Boa noite";

            await context.PostAsync($"{saudacao}! Em que posso ajudar?");
            context.Done<string>(null);
        }

        /// <summary>
        /// Quando a intenção for de tradução de um determinado texto.
        /// </summary>
        [LuisIntent("traduzir-texto")]
        public async Task TraduzirTexto(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("**(ಥ﹏ಥ)** - Ainda estou estudando... tenha um pouco de paciência...");
        }

        /// <summary>
        /// Quando a intenção for a solicitação de recomendação baseada em um produto.
        /// </summary>
        [LuisIntent("recomendar-por-produto")]
        public async Task RecomendarPorProduto(IDialogContext context, LuisResult result)
        {
            var produtoId = result.Entities.FirstOrDefault(c => c.Type == "produto")?.Entity;

            if (string.IsNullOrEmpty(produtoId))
            {
                await context.PostAsync("**(ಥ﹏ಥ)** - Foi mal, não sei recomendar nada, mas quando aprender" +
                                        "vou precisar do seu código de produto...");
            }
            else
            {
                await context.PostAsync($"**(¬‿¬)** - Eu ainda não sei fazer recomendações mas já identifico " +
                                        $"o código do seu produto: **{produtoId}**");
            }
        }

        /// <summary>
        /// Quando a intenção for a solicitação de recomendação baseada em um perfil de compra.
        /// </summary>
        [LuisIntent("recomendar-por-perfil")]
        public async Task RecomendarPorPerfil(IDialogContext context, LuisResult result)
        {
            var usuarioId = result.Entities.FirstOrDefault(c => c.Type == "usuario")?.Entity;

            if (string.IsNullOrEmpty(usuarioId))
            {
                await context.PostAsync("**(ಥ﹏ಥ)** - Foi mal, não sei recomendar nada, mas quando aprender" +
                                        "vou precisar do seu id de usuário...");
            }
            else
            {
                await context.PostAsync($"**(¬‿¬)** - Seu id de usuário é **{usuarioId}**. Quando eu aprender a " +
                                        "recomendar eu te respondo...");
            }
        }

        /// <summary>
        /// Quando a intenção for detectar emoções em uma determinada imagem.
        /// </summary>
        [LuisIntent("reconhecer-emocoes")]
        public async Task ReconhecerEmocoes(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Ok, me envia a imagem a ser analisada.");
            context.Wait((c, a) => ProcessarImagemAsync(c, a, TipoDeProcessamento.Emocoes));
        }

        /// <summary>
        /// Quando a intenção for descrever uma imagem.
        /// </summary>
        [LuisIntent("descrever-imagem")]
        public async Task DescreverImagen(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Beleza, me envia uma imagem que eu descrevo o que tem nela.");
            context.Wait((c, a) => ProcessarImagemAsync(c, a, TipoDeProcessamento.Descricao));
        }

        /// <summary>
        /// Quando a intenção for classificar uma imagem.
        /// </summary>
        [LuisIntent("classificar-imagem")]
        public async Task ClassificarImagem(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Me envia uma imagem e eu te direi o que ela é!");
            context.Wait((c, a) => ProcessarImagemAsync(c, a, TipoDeProcessamento.Classificacao));
        }

        private async Task ProcessarImagemAsync(IDialogContext contexto,
            IAwaitable<IMessageActivity> argument,
            TipoDeProcessamento tipoDeProcessamento)
        {
            var activity = await argument;

            var uri = activity.Attachments?.Any() == true ?
                new Uri(activity.Attachments[0].ContentUrl) :
                new Uri(activity.Text);

            try
            {
                string reply;

                switch (tipoDeProcessamento)
                {
                    case TipoDeProcessamento.Descricao:
                        reply = await new VisaoComputacional().AnaliseDetalhadaAsync(uri);
                        break;
                    case TipoDeProcessamento.Emocoes:
                        reply = await new VisaoComputacional().DeteccaoDeEmocoesAsync(uri);
                        break;
                    case TipoDeProcessamento.Classificacao:
                        reply = await new VisaoComputacional().ClassificacaoCustomizadaAsync(uri);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tipoDeProcessamento),
                            tipoDeProcessamento, null);
                }
                await contexto.PostAsync(reply);
            }
            catch (Exception)
            {
                await contexto.PostAsync("Ops! Deu algo errado na hora de analisar sua imagem!");
            }

            contexto.Wait(MessageReceived);
        }
    }
}