using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data.TopicModel
{
    public class AiInformation
    {
        /// <summary>
        /// The text corresponding to the recognized entity, as it appears in the input document.
        /// </summary>
        public required string Text { get; set; }

        /// <summary>
        /// The category of the entity as recognized by the service's named entity recognition model. For the list of
        /// supported categories, see
        /// <see href="https://learn.microsoft.com/azure/cognitive-services/language-service/named-entity-recognition/concepts/named-entity-categories"/>.
        /// </summary>
        public required string Category { get; set; }
        // Examples - 
        //public static readonly EntityCategory Person = new EntityCategory("Person");
        //public static readonly EntityCategory PersonType = new EntityCategory("PersonType");
        //public static readonly EntityCategory Location = new EntityCategory("Location");
        //public static readonly EntityCategory Organization = new EntityCategory("Organization");
        //public static readonly EntityCategory Event = new EntityCategory("Event");
        //public static readonly EntityCategory Product = new EntityCategory("Product");
        //public static readonly EntityCategory Skill = new EntityCategory("Skill");
        //public static readonly EntityCategory DateTime = new EntityCategory("DateTime");
        //public static readonly EntityCategory PhoneNumber = new EntityCategory("PhoneNumber");
        //public static readonly EntityCategory Email = new EntityCategory("Email");
        //public static readonly EntityCategory Url = new EntityCategory("URL");
        //public static readonly EntityCategory IPAddress = new EntityCategory("IPAddress");
        //public static readonly EntityCategory Quantity = new EntityCategory("Quantity");
        //public static readonly EntityCategory Address = new EntityCategory("Address");



        /// <summary>
        /// The subcategory of the entity (if applicable) as recognized by the service's named entity recognition
        /// model. For the list of supported categories and subcategories, see
        /// <see href="https://learn.microsoft.com/azure/cognitive-services/language-service/named-entity-recognition/concepts/named-entity-categories"/>.
        /// </summary>
        public string? SubCategory { get; set; }

        /// <summary>
        /// The score between 0.0 and 1.0 indicating the confidence that the recognized entity accurately corresponds
        /// to the text substring.
        /// </summary>
        public double ConfidenceScore { get; set; }

        /// <summary>
        /// The starting position of the matching text in the input document.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// The length of the matching text in the input document.
        /// </summary>
        public int Length { get; set; }
    }
}
