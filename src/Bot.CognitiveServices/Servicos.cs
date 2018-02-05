using Bot.CognitiveServices.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Bot.CognitiveServices
{
    public class VisaoComputacional
    {
        private readonly string _emotionApiKey = ConfigurationManager.AppSettings["EmotionApiKey"];
        private readonly string _emotionUri = ConfigurationManager.AppSettings["EmotionApiUri"];

        private readonly string _customApiKey = ConfigurationManager.AppSettings["CustomVisionKey"];
        private readonly string _customVisionUri = ConfigurationManager.AppSettings["CustomVisionUri"];

        private readonly string _computerVisionApiKey = ConfigurationManager.AppSettings["ComputerVisionApiKey"];
        private readonly string _computerVisionUri = ConfigurationManager.AppSettings["ComputerVisionUri"];

        private static readonly Dictionary<string, string> Adjetivos = new Dictionary<string, string>()
        {
            { "Anger", "angry" },
            { "Contempt", "contemptuous" },
            { "Disgust", "disgusted" },
            { "Fear", "scared" },
            { "Happiness", "happy" },
            { "Neutral", "neutral" },
            { "Sadness", "sad" },
            { "Surprise", "surprised" }
        };

        public async Task<string> DeteccaoDeEmocoesAsync(Uri query)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _emotionApiKey);

            HttpResponseMessage response = null;

            var byteData = Encoding.UTF8.GetBytes("{ 'url': '" + query + "' }");

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(_emotionUri, content).ConfigureAwait(false);
            }

            var responseString = await response.Content.ReadAsStringAsync();

            var emotions = JsonConvert.DeserializeObject<EmotionResult[]>(responseString)
                .Select(e => Adjetivos[e.scores.ToRankedList().First().Key]).ToList();

            if (!emotions.Any()) return "Nenhuma face detectada :( Eu não consegui encontrar " +
                                        "nenhuma pessoa nessa imagem.";

            var count = emotions.Count;

            string retorno;

            switch (count)
            {
                case 1:
                    retorno = $"Eu identifiquei uma pessoa nessa imagem, e ela parece estar **{emotions.First()}**";
                    break;
                default:
                    var builder = new StringBuilder($"E identifiquei **{count}** pessoas nessa imagem, e elas estão: ");
                    for (var i = 0; i < count; i++)
                    {
                        if (i == count - 1) builder.Append(" & ");
                        else if (i != 0) builder.Append(", ");

                        builder.Append($"**{emotions.ElementAt(i)}**");
                    }
                    retorno = builder.ToString();
                    break;
            }

            var dicionarioDeEmocoes = new Dictionary<string, string>();

            foreach (var item in Adjetivos)
            {
                var i = emotions.Count(c => c == item.Value);

                if (i > 0)
                    dicionarioDeEmocoes.Add(item.Value, i.ToString());
            }

            return dicionarioDeEmocoes.Aggregate(retorno, (current, item) =>
                current + $"\nAchei **{item.Value}** " +
                $"pessoas que eu identifiquei como **{item.Key}**");
        }

        public async Task<string> ClassificacaoCustomizadaAsync(Uri query)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Prediction-key", _customApiKey);

            HttpResponseMessage response = null;

            var byteData = Encoding.UTF8.GetBytes("{ 'url': '" + query + "' }");

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(_customVisionUri, content).ConfigureAwait(false);
            }

            var responseString = await response.Content.ReadAsStringAsync();

            var emotions = JsonConvert.DeserializeObject<CustomVisionResult>(responseString);

            var emotion = emotions.Predictions.OrderByDescending(c => c.Probability).FirstOrDefault();

            return $"Eu identifiquei um objeto do tipo **{emotion?.Tag}** na imagem, com " +
                   $"**{emotion?.Probability}**% de acertividade.";
        }

        public async Task<string> AnaliseDetalhadaAsync(Uri query)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _computerVisionApiKey);

            HttpResponseMessage response = null;

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["visualFeatures"] = "Categories,Tags,Description,Faces,ImageType,Color,Adult";

            var byteData = Encoding.UTF8.GetBytes("{ 'url': '" + query + "' }");

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync($"{_computerVisionUri}?{queryString}",
                    content).ConfigureAwait(false);
            }

            var responseString = await response.Content.ReadAsStringAsync();

            var analise = JsonConvert.DeserializeObject<AnalyzeResult>(responseString);

            var descricao = analise.description.captions.FirstOrDefault()?.text;
            var temConteudoAdulto = analise.adult.isAdultContent;
            var temConteudoRacista = analise.adult.isRacyContent;
            var quantasPessoasForamIdentificadas = analise.faces.Count;

            var builder = new StringBuilder("Eu identifiquei os seguintes objetos na imagem: ");
            for (var i = 0; i < analise.tags.Count; i++)
            {
                if (i == analise.tags.Count - 1) builder.Append(" & ");
                else if (i != 0) builder.Append(", ");

                builder.Append(analise.tags[i].name);
            }

            return $"Descrição: **{descricao}**\n" +
                   $"Tags: **{builder}**\n" +
                   $"Tem conteúdo adulto: **{temConteudoAdulto}**\n" +
                   $"Tem conteúdo racista: **{temConteudoRacista}**\n" +
                   $"Tem alguma pessoa na foto: **{quantasPessoasForamIdentificadas}**";
        }
    }

    public class Recomendacao
    {
        private readonly string _recommendationApiKey = ConfigurationManager.AppSettings["RecommendationApiKey"];
        private readonly string _recommendationUri = ConfigurationManager.AppSettings["RecommendationUri"];
        private readonly string _modeloId = ConfigurationManager.AppSettings["RecommendationModelId"];
        private readonly long _buildId = Convert.ToInt64(ConfigurationManager.AppSettings["RecommendationBuildId"]);
        private readonly int _numeroDeResultados = Convert.ToInt32(ConfigurationManager.AppSettings["RecommendationNumberOfResults"]);

        public string RecomendacoesPorProduto(string produtoId)
        {
            var response = "Minhas sugestões são:";
            var itemSets = BuscarRecomendacoesPorProduto(_modeloId, _buildId, produtoId, _numeroDeResultados);

            response = itemSets.recommendedItems != null ? 
                itemSets.recommendedItems.SelectMany(recoSet => recoSet.items)
                .Aggregate(response, (current, item) => current + $"\n* Id: **{item.id}** Item: **{item.name}**") : 
                "Não foi possível encontar nenhuma recomendação para este produto.";

            return response;
        }

        public string RecomendacoesPorUsuario(string usuarioId)
        {
            var response = "Minhas sugestões são:";
            var itemSets = BuscarRecomendacoesPorUsuario(_modeloId, _buildId, usuarioId, _numeroDeResultados);

            response = itemSets.recommendedItems != null ? 
                itemSets.recommendedItems.SelectMany(recoSet => recoSet.items)
                .Aggregate(response, (current, item) => current + $"\n* Id: **{item.id}** Item: **{item.name}**") : 
                "Não foi possível encontar nenhuma recomendação para este usuário.";

            return response;
        }

        /// <summary>
        /// Get Item to Item (I2I) Recommendations or Frequently-Bought-Together (FBT) recommendations
        /// </summary>
        /// <param name="modelId">The model identifier.</param>
        /// <param name="buildId">The build identifier. Set to null if you want to use active build</param>
        /// <param name="itemIds"></param>
        /// <param name="numberOfResults"></param>
        /// <returns>
        /// The recommendation sets. Note that I2I builds will only return one item per set.
        /// FBT builds will return more than one item per set.
        /// </returns>
        private RecommendedItemSetInfoList BuscarRecomendacoesPorProduto(string modelId, long buildId, string itemIds, int numberOfResults)
        {

            var uri = _recommendationUri + "/models/" + modelId + "/recommend/item?itemIds=" + itemIds +
                         "&numberOfResults=" + numberOfResults + "&minimalScore=0&buildId=" + buildId;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _recommendationApiKey);

            var response = client.GetAsync(uri).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Error {response.StatusCode}: Failed to get recommendations for " +
                    $"modelId {modelId}, buildId {buildId}, Reason: {ExtrairDetalhesDoErro(response)}");
            }

            var jsonString = response.Content.ReadAsStringAsync().Result;
            var recommendedItemSetInfoList = JsonConvert.DeserializeObject<RecommendedItemSetInfoList>(jsonString);
            return recommendedItemSetInfoList;
        }

        /// <summary>
        /// Use historical transaction data to provide personalized recommendations for a user.
        /// The user history is extracted from the usage files used to train the model.
        /// </summary>
        /// <param name="modelId">The model identifier.</param>
        /// <param name="buildId">The build identifier. Set to null to use active build.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="numberOfResults">Desired number of recommendation results.</param>
        /// <returns>The recommendations for the user.</returns>
        private RecommendedItemSetInfoList BuscarRecomendacoesPorUsuario(string modelId, long buildId, string userId, int numberOfResults)
        {
            var uri = _recommendationUri + "/models/" + modelId + "/recommend/user?userId=" + userId +
                "&numberOfResults=" + numberOfResults + "&buildId=" + buildId;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _recommendationApiKey);

            var response = client.GetAsync(uri).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Error {response.StatusCode}: Failed to get user recommendations for " +
                    $"modelId {modelId}, buildId {buildId}, Reason: {ExtrairDetalhesDoErro(response)}");
            }

            var jsonString = response.Content.ReadAsStringAsync().Result;
            var recommendedItemSetInfoList = JsonConvert.DeserializeObject<RecommendedItemSetInfoList>(jsonString);
            return recommendedItemSetInfoList;
        }

        /// <summary>
        /// Extract error message from the httpResponse, (reason phrase + body)
        /// </summary>
        /// <param name="resposta"></param>
        /// <returns></returns>
        private static string ExtrairDetalhesDoErro(HttpResponseMessage resposta)
        {
            string detalhes = null;
            if (resposta.Content != null)
            {
                detalhes = resposta.Content.ReadAsStringAsync().Result;
            }
            return detalhes == null ? resposta.ReasonPhrase : resposta.ReasonPhrase + "->" + detalhes;
        }
    }

    public class Linguagem
    {
        private readonly string _translateApiKey = ConfigurationManager.AppSettings["TranslateApiKey"];
        private readonly string _translateUri = ConfigurationManager.AppSettings["TranslateUri"];

        public async Task<string> TraducaoDeTextoAsync(string texto)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _translateApiKey);

            var uri = _translateUri + "?to=pt-br" +
                      "&text=" + System.Net.WebUtility.UrlEncode(texto);

            var response = await client.GetAsync(uri);
            var result = await response.Content.ReadAsStringAsync();
            var content = XElement.Parse(result).Value;

            return $"Texto original: **{ texto }**\nTradução: **{ content }**";
        }
    }
}