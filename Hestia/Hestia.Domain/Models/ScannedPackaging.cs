using Haondt.Core.Models;

namespace Hestia.Domain.Models
{
    public class ScannedPackaging : IScannedIngredientData
    {
        [DocumentAIField("name")]
        public Optional<string> Name { get; set; }

        [DocumentAIField("brand")]
        public Optional<string> Brand { get; set; }

        [DocumentAIField("packageSize")]
        public Optional<decimal> PackageSize { get; set; }

        [DocumentAIField("packageSizeUnit")]
        public Optional<string> PackageSizeUnit { get; set; }
        public TimeSpan ProcessingDuration { get; set; }
    }
}
