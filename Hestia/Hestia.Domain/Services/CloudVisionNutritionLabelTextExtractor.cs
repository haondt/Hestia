using Google.Cloud.Vision.V1;

namespace Hestia.Domain.Services
{
    public class CloudVisionNutritionLabelTextExtractor : INutritionLabelTextExtractor
    {
        private readonly ImageAnnotatorClient _visionClient;
        private readonly INutritionLabelProcessingStateService _stateService;

        public CloudVisionNutritionLabelTextExtractor(
            ImageAnnotatorClient visionClient,
            INutritionLabelProcessingStateService stateService)
        {
            _visionClient = visionClient;
            _stateService = stateService;
        }

        public async Task<string> ExtractTextAsync(Stream imageData, Guid processingId)
        {
            _stateService.UpdateProcessingStatus(processingId, "Extracting text with Cloud Vision OCR...");

            var image = Google.Cloud.Vision.V1.Image.FromStream(imageData);

            var response = await _visionClient.DetectDocumentTextAsync(image);
            return response.Text;
        }
    }
}