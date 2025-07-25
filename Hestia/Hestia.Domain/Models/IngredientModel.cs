using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Persistence.Models;
using System.ComponentModel.DataAnnotations;

namespace Hestia.Domain.Models
{
    public class IngredientModel
    {
        [Required]
        public required string Name { get; set; }
        public required Optional<string> Brand { get; set; }
        public required Optional<string> Vendor { get; set; }

        public required Optional<decimal> ServingSizeQuantity { get; set; }
        public required Optional<string> ServingSizeUnit { get; set; }
        public required Optional<decimal> Calories { get; set; }
        public required Optional<decimal> FatGrams { get; set; }
        public required Optional<decimal> CarbGrams { get; set; }
        public required Optional<decimal> ProteinGrams { get; set; }

        public required Optional<decimal> PackageSizeQuantity { get; set; }
        public required Optional<string> PackageSizeUnit { get; set; }
        public required Optional<decimal> PackageCostDollars { get; set; }

        private string _notes = "";
        public string Notes { get => _notes; set => _notes = value ?? ""; }

        public static IngredientModel FromDataModel(IngredientDataModel model) => new()
        {
            Name = model.Name,
            Brand = model.Brand.AsOptional(),
            Vendor = model.Vendor.AsOptional(),
            ServingSizeQuantity = model.ServingSizeQuantity.AsOptional(),
            ServingSizeUnit = model.ServingSizeUnit.AsOptional(),
            Calories = model.Calories.AsOptional(),
            FatGrams = model.FatGrams.AsOptional(),
            CarbGrams = model.CarbGrams.AsOptional(),
            ProteinGrams = model.ProteinGrams.AsOptional(),
            PackageSizeQuantity = model.PackageSizeQuantity.AsOptional(),
            PackageSizeUnit = model.PackageSizeUnit.AsOptional(),
            PackageCostDollars = model.PackageCostDollars.AsOptional(),
            Notes = model.Notes
        };

        public IngredientDataModel AsDataModel() => new()
        {
            Name = Name,
            Brand = Brand.Unwrap(),
            Vendor = Vendor.Unwrap(),
            ServingSizeQuantity = ServingSizeQuantity.Unwrap(),
            ServingSizeUnit = ServingSizeUnit.Unwrap(),
            Calories = Calories.Unwrap(),
            FatGrams = FatGrams.Unwrap(),
            CarbGrams = CarbGrams.Unwrap(),
            ProteinGrams = ProteinGrams.Unwrap(),
            PackageSizeQuantity = PackageSizeQuantity.Unwrap(),
            PackageSizeUnit = PackageSizeUnit.Unwrap(),
            PackageCostDollars = PackageCostDollars.Unwrap(),
            Notes = Notes
        };
    }
}
