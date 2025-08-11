using Haondt.Core.Models;

namespace Hestia.Domain.Models
{
    public class ScannedNutritionLabel
    {
        [DocumentAIField("alternateServingSizeQuantity")]
        public Optional<decimal> AlternateServingSizeQuantity { get; set; }

        [DocumentAIField("alternateServingSizeUnit")]
        public Optional<string> AlternateServingSizeUnit { get; set; }

        [DocumentAIField("servingSizeQuantity")]
        public Optional<decimal> ServingSizeQuantity { get; set; }

        [DocumentAIField("servingSizeUnit")]
        public Optional<string> ServingSizeUnit { get; set; }

        [DocumentAIField("calories")]
        public Optional<decimal> Calories { get; set; }
        [DocumentAIField("carbohydrateGrams")]
        public Optional<decimal> CarbohydrateGrams { get; set; }
        [DocumentAIField("fatGrams")]
        public Optional<decimal> FatGrams { get; set; }
        [DocumentAIField("proteinGrams")]
        public Optional<decimal> ProteinGrams { get; set; }
        [DocumentAIField("fibreGrams")]
        public Optional<decimal> FibreGrams { get; set; }
        [DocumentAIField("sodiumGrams")]
        public Optional<decimal> SodiumGrams { get; set; }
        
        public TimeSpan ProcessingDuration { get; set; }
    }
}
