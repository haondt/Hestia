using Google.Cloud.DocumentAI.V1;
using Google.Protobuf;
using Hestia.Domain.Models;
using Microsoft.Extensions.Options;

namespace Hestia.Domain.Services
{
    public class DocumentAINutritionLabelTextExtractor<T> : IScanTextExtractor<T>
    {
        private readonly DocumentProcessorServiceClient _documentAiClient;
        private readonly DocumentAISettings _settings;
        private readonly IScanProcessingStateService<T> _stateService;

        public DocumentAINutritionLabelTextExtractor(
            DocumentProcessorServiceClient documentAiClient,
            IOptions<ScannerSettings> options,
            IScanProcessingStateService<T> stateService)
        {
            _documentAiClient = documentAiClient;
            _settings = options.Value.DocumentAI ?? throw new InvalidOperationException("DocumentAI settings not configured");
            _stateService = stateService;
        }

        public async Task<string> ExtractTextAsync(Stream imageData, Guid processingId)
        {
            _stateService.UpdateProcessingStatus(processingId, "Extracting text with DocumentAI OCR...");

            var request = new ProcessRequest
            {
                Name = ProcessorName.FromProjectLocationProcessor(
                    _settings.ProjectId,
                    _settings.ProcessorLocationId,
                    _settings.ProcessorId)
                    .ToString(),
                RawDocument = new RawDocument
                {
                    Content = ByteString.FromStream(imageData),
                    MimeType = "image/jpeg"
                }
            };

            var response = await _documentAiClient.ProcessDocumentAsync(request);
            return response.Document.Text;
        }
    }
}