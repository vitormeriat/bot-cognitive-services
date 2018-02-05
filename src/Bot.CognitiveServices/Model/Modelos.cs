using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.CognitiveServices.Model
{
    [Serializable]
    public class Scores
    {
        public double anger { get; set; }
        public double contempt { get; set; }
        public double disgust { get; set; }
        public double fear { get; set; }
        public double happiness { get; set; }
        public double neutral { get; set; }
        public double sadness { get; set; }
        public double surprise { get; set; }

        /// <summary>
        /// Create a sorted key-value pair of emotions and the corresponding scores, sorted from highest score on down.
        /// To make the ordering stable, the score is the primary key, and the name is the secondary key.
        /// </summary>
        public IEnumerable<KeyValuePair<string, double>> ToRankedList()
        {
            return new Dictionary<string, double>()
                {
                    { "Anger", anger },
                    { "Contempt", contempt },
                    { "Disgust", disgust },
                    { "Fear", fear },
                    { "Happiness", happiness },
                    { "Neutral", neutral },
                    { "Sadness", sadness },
                    { "Surprise", surprise }
                }
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .ToList();
        }
    }

    [Serializable]
    public class EmotionResult
    {
        public Scores scores { get; set; }
    }

    [Serializable]
    public class Prediction
    {
        public string TagId { get; set; }
        public string Tag { get; set; }
        public double Probability { get; set; }
    }

    [Serializable]
    public class CustomVisionResult
    {
        public List<Prediction> Predictions { get; set; }
    }
    
    [Serializable]
    public class Adult
    {
        public double adultScore { get; set; }
        public bool isAdultContent { get; set; }
        public bool isRacyContent { get; set; }
        public double racyScore { get; set; }
    }

    [Serializable]
    public class Category
    {
        public string name { get; set; }
        public double score { get; set; }
    }

    [Serializable]
    public class Caption
    {
        public double confidence { get; set; }
        public string text { get; set; }
    }

    [Serializable]
    public class Description
    {
        public List<Caption> captions { get; set; }
        public List<string> tags { get; set; }
    }

    [Serializable]
    public class Tag
    {
        public double confidence { get; set; }
        public string name { get; set; }
    }

    [Serializable]
    public class AnalyzeResult
    {
        public Adult adult { get; set; }
        public List<Category> categories { get; set; }
        public Description description { get; set; }
        public List<object> faces { get; set; }
        public string requestId { get; set; }
        public List<Tag> tags { get; set; }
    }

    [Serializable]
    public class RecommendedItemSetInfoList
    {
        public IEnumerable<RecommendedItemSetInfo> recommendedItems { get; set; }
    }

    [Serializable]
    public class RecommendedItemSetInfo
    {
        public RecommendedItemSetInfo()
        {
            items = new List<RecommendedItemInfo>();
        }

        public IEnumerable<RecommendedItemInfo> items { get; set; }

        public double rating { get; set; }

        public IEnumerable<string> reasoning { get; set; }
    }

    [Serializable]
    public class RecommendedItemInfo
    {
        public string id { get; set; }

        public string name { get; set; }

        public string metadata { get; set; }
    }
}