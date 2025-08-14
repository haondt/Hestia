using Google.Cloud.Vision.V1;

namespace Hestia.Domain.Services
{
    public class CloudVisionScanTextExtractor<T> : IScanTextExtractor<T>
    {
        private readonly ImageAnnotatorClient _visionClient;
        private readonly IScanProcessingStateService<T> _stateService;

        public CloudVisionScanTextExtractor(
            ImageAnnotatorClient visionClient,
            IScanProcessingStateService<T> stateService)
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