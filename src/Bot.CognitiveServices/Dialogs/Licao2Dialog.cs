using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.CognitiveServices.Dialogs
{
    [Serializable]
    public class Licao2Dialog : LuisDialog<object>
    {
        public Licao2Dialog(ILuisService service) : base(service) { }

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
        /// Quando a intenção for detectar emoções em uma determinada imagem.
        /// </summary>
        [LuisIntent("reconhecer-emocoes")]
        public async Task ReconhecerEmocoes(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("**ಠ~ಠ** - Desculpa, eu ainda não sei reconhecer emoções...");
        }

        /// <summary>
        /// Quando a intenção for descrever uma imagem.
        /// </summary>
        [LuisIntent("descrever-imagem")]
        public async Task DescreverImagen(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("**(¬_¬)** - Foi mal, ainda não aprendi a descrever coisas...");
        }

        /// <summary>
        /// Quando a intenção for classificar uma imagem.
        /// </summary>
        [LuisIntent("classificar-imagem")]
        public async Task ClassificarImagem(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("**(ง'̀-'́)ง** - Quase lá... juro que na próxima vez vou saber classificar uma imagem...");
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
    }
}