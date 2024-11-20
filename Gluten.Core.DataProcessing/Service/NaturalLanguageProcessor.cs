using Azure.AI.TextAnalytics;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Gluten.Core.DataProcessing.Service
{
    /// <summary>
    /// Uses Azure AI to help process human written messages/topics
    /// </summary>
    public class NaturalLanguageProcessor
    {
        TextAnalyticsClient _client;

        /// <summary>
        /// Constructor
        /// </summary>
        public NaturalLanguageProcessor(string endpointName, string apiKey)
        {
            Uri endpoint = new(endpointName);
            AzureKeyCredential credential = new(apiKey);
            _client = new(endpoint, credential);
        }

        /// <summary>
        /// Processes a message hopefully returns something useful in formatted fields
        /// </summary>
        public List<CategorizedEntity> Process(string message)
        {

            string documentA = message;
            //e.g. documentA = "Tonight we went to OKO - Fun Okonomiyaki Bar and it was delicious. However I do not think 源氏蕎麦 Genjisoba gets enough love. We were there last night and undoubtedly the best GF meal I’ve had in a LONG time \n#osaka";

            try
            {
                // Prepare the input of the text analysis operation. You can add multiple documents to this list and
                // perform the same operation on all of them simultaneously.
                List<TextDocumentInput> batchedDocuments = new()
            {
                new TextDocumentInput("1", documentA)
                {
                     Language = "en",
                },
            };

                // Specify the project and deployment names of the desired custom model. To train your own custom model to
                // recognize custom entities, see https://aka.ms/azsdk/textanalytics/customentityrecognition.
                //string projectName = "Gluten";
                //string deploymentName = "Frodo";

                // Perform the text analysis operation.
                //_client.RecognizeCustomEntitiesAsync
                //var operation = _client.RecognizeCustomEntities(WaitUntil.Completed, batchedDocuments, projectName, deploymentName);
                var operation = _client.RecognizeEntities(documentA);
                List<CategorizedEntity> aiResponse = operation.Value.ToList();
                return aiResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new List<CategorizedEntity>();
            }

            /*foreach (RecognizeCustomEntitiesResultCollection documentsInPage in operation.GetValues())
            {
                foreach (RecognizeEntitiesResult documentResult in documentsInPage)
                {
                    Console.WriteLine($"Result for document with Id = \"{documentResult.Id}\":");

                    if (documentResult.HasError)
                    {
                        Console.WriteLine($"  Error!");
                        Console.WriteLine($"  Document error code: {documentResult.Error.ErrorCode}");
                        Console.WriteLine($"  Message: {documentResult.Error.Message}");
                        Console.WriteLine();
                        continue;
                    }

                    Console.WriteLine($"  Recognized {documentResult.Entities.Count} entities:");

                    foreach (CategorizedEntity entity in documentResult.Entities)
                    {
                        Console.WriteLine($"  Entity: {entity.Text}");
                        Console.WriteLine($"  Category: {entity.Category}");
                        Console.WriteLine($"  Offset: {entity.Offset}");
                        Console.WriteLine($"  Length: {entity.Length}");
                        Console.WriteLine($"  ConfidenceScore: {entity.ConfidenceScore}");
                        Console.WriteLine($"  SubCategory: {entity.SubCategory}");
                        Console.WriteLine();
                    }

                    Console.WriteLine();
                }
            }*/
        }
    }
}
