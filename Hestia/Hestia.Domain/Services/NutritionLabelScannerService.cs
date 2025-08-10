using Google.Cloud.DocumentAI.V1;
using Google.Protobuf;
using Haondt.Core.Models;
using Hestia.Domain.Models;
using Hestia.Persistence.Models;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Reflection;

namespace Hestia.Domain.Services
{
    public class NutritionLabelScannerService(IOptions<NutritionLabelScannerSettings> options,
        IOptions<PersistenceSettings> persistenceOptions,
        DocumentProcessorServiceClient client,
        INutritionLabelProcessingStateService stateService) : INutritionLabelScannerService
    {
        NutritionLabelScannerSettings _settings = options.Value;
        PersistenceSettings _persistenceSettings = persistenceOptions.Value;

        private static readonly Dictionary<string, string> _fieldMappings = typeof(ScannedNutritionLabel)
            .GetProperties()
            .Where(p => p.GetCustomAttribute<DocumentAIFieldAttribute>() != null)
            .ToDictionary(
                p => p.Name,
                p => p.GetCustomAttribute<DocumentAIFieldAttribute>()!.FieldName
            );

        private static readonly HashSet<string> _documentAIFields = new(_fieldMappings.Values);


        public async Task ProcessLabelImageAsync(Stream imageStream, Guid processingId)
        {
            stateService.UpdateProcessingStatus(processingId, "Resizing image...");

            using var image = await Image.LoadAsync(imageStream);

            // Resize image if it's too large (max 1024px on longest side)
            var maxSize = 1024;
            if (image.Width > maxSize || image.Height > maxSize)
            {
                var ratio = Math.Min((double)maxSize / image.Width, (double)maxSize / image.Height);
                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                image.Mutate(x => x.Resize(newWidth, newHeight));
            }

            stateService.UpdateProcessingStatus(processingId, "Compressing image...");
            var encoder = new JpegEncoder
            {
                Quality = 85 // Good balance between quality and file size
            };

            using var memoryStream = new MemoryStream();
            await image.SaveAsJpegAsync(memoryStream, encoder);
            memoryStream.Position = 0;

            var rawDocument = new RawDocument
            {
                Content = ByteString.FromStream(memoryStream),
                MimeType = "image/jpeg"
            };

            var request = new ProcessRequest
            {
                Name = ProcessorName.FromProjectLocationProcessor(
                    _settings.DocumentAI!.ProjectId,
                    _settings.DocumentAI!.ProcessorLocationId,
                    _settings.DocumentAI!.ProcessorId)
                    .ToString(),
                RawDocument = rawDocument
            };

            stateService.UpdateProcessingStatus(processingId, "Detecting labels...");
            var response = await client.ProcessDocumentAsync(request);
            var document = response.Document;

            stateService.UpdateProcessingStatus(processingId, "Processing result...");
            var scannedLabel = new ScannedNutritionLabel();
            foreach (var entity in document.Entities)
            {
                if (!_documentAIFields.Contains(entity.Type)) continue;

                switch (entity.Type)
                {
                    case var fieldName when fieldName == GetFieldName(nameof(ScannedNutritionLabel.ServingSizeQuantity)):
                        var hasServingSizeUnit = document.Entities.Any(e => e.Type == GetFieldName(nameof(ScannedNutritionLabel.ServingSizeUnit)));
                        scannedLabel.ServingSizeQuantity = ParseQuantityWithFractions(entity.MentionText, hasServingSizeUnit);
                        break;

                    case var fieldName when fieldName == GetFieldName(nameof(ScannedNutritionLabel.AlternateServingSizeQuantity)):
                        var hasAlternateServingSizeUnit = document.Entities.Any(e => e.Type == GetFieldName(nameof(ScannedNutritionLabel.AlternateServingSizeUnit)));
                        scannedLabel.AlternateServingSizeQuantity = ParseQuantityWithFractions(entity.MentionText, hasAlternateServingSizeUnit);
                        break;

                    case var fieldName when fieldName == GetFieldName(nameof(ScannedNutritionLabel.ServingSizeUnit)):
                        var servingSizeUnitValue = entity.MentionText?.Trim();
                        if (!string.IsNullOrEmpty(servingSizeUnitValue))
                            scannedLabel.ServingSizeUnit = new Optional<string>(servingSizeUnitValue);
                        break;

                    case var fieldName when fieldName == GetFieldName(nameof(ScannedNutritionLabel.AlternateServingSizeUnit)):
                        var alternateServingSizeUnitValue = entity.MentionText?.Trim();
                        if (!string.IsNullOrEmpty(alternateServingSizeUnitValue))
                            scannedLabel.AlternateServingSizeUnit = new Optional<string>(alternateServingSizeUnitValue);
                        break;

                    case var fieldName when fieldName == GetFieldName(nameof(ScannedNutritionLabel.Calories)):
                        scannedLabel.Calories = ParseSimpleDecimal(entity.MentionText);
                        break;

                    case var fieldName when fieldName == GetFieldName(nameof(ScannedNutritionLabel.CarbohydrateGrams)):
                        scannedLabel.CarbohydrateGrams = ParseSimpleDecimal(entity.MentionText);
                        break;

                    case var fieldName when fieldName == GetFieldName(nameof(ScannedNutritionLabel.FatGrams)):
                        scannedLabel.FatGrams = ParseSimpleDecimal(entity.MentionText);
                        break;

                    case var fieldName when fieldName == GetFieldName(nameof(ScannedNutritionLabel.ProteinGrams)):
                        scannedLabel.ProteinGrams = ParseSimpleDecimal(entity.MentionText);
                        break;

                    case var fieldName when fieldName == GetFieldName(nameof(ScannedNutritionLabel.FibreGrams)):
                        scannedLabel.FibreGrams = ParseSimpleDecimal(entity.MentionText);
                        break;

                    case var fieldName when fieldName == GetFieldName(nameof(ScannedNutritionLabel.SodiumGrams)):
                        scannedLabel.SodiumGrams = ParseSimpleDecimal(entity.MentionText);
                        break;
                }
            }

            stateService.SetProcessingResult(processingId, scannedLabel);
        }

        public Guid StartBackgroundProcessing(Stream imageStream)
        {
            var processingId = stateService.StartProcessing();

            // Copy the stream since we need to process it in the background
            var memoryStream = new MemoryStream();
            imageStream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            // Start background processing
            Task.Run(async () =>
            {
                try
                {
                    await ProcessLabelImageAsync(memoryStream, processingId);
                }
                catch (Exception ex)
                {
                    stateService.SetProcessingFailure(processingId, $"Error processing image: {ex.Message}");
                }
                finally
                {
                    memoryStream.Dispose();
                }
            });

            return processingId;
        }

        private static Optional<decimal> ParseSimpleDecimal(string? mentionText)
        {
            var value = mentionText?.Trim();
            if (!string.IsNullOrEmpty(value) && decimal.TryParse(value, out var parsed) && parsed >= 0)
                return parsed;
            return default;
        }

        private static Optional<decimal> ParseQuantityWithFractions(string? mentionText, bool hasCorrespondingUnit)
        {
            if (string.IsNullOrWhiteSpace(mentionText))
            {
                // Special case: if we have a unit but no quantity, assume quantity is 1
                if (hasCorrespondingUnit)
                    return 1m;

                return default;
            }

            var components = mentionText.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (components.Length == 0)
                return default;

            var value = 0m;
            var success = true;

            foreach (var component in components)
            {
                var parts = component.Split('/');
                if (parts.Length == 1)
                {
                    // Whole number or decimal
                    if (decimal.TryParse(parts[0], out var parsedValue) && parsedValue >= 0)
                    {
                        value += parsedValue;
                        continue;
                    }
                }
                else if (parts.Length == 2)
                {
                    // Fraction like "1/4"
                    if (decimal.TryParse(parts[0], out var numerator)
                        && decimal.TryParse(parts[1], out var denominator)
                        && denominator > 0 && numerator >= 0)
                    {
                        value += numerator / denominator;
                        continue;
                    }
                }

                success = false;
                break;
            }

            return success && value >= 0 ? value : default;
        }

        private static string GetFieldName(string propertyName)
        {
            if (!_fieldMappings.TryGetValue(propertyName, out var fieldName))
                throw new InvalidOperationException($"No DocumentAI field mapping found for property '{propertyName}'");

            return fieldName;
        }
    }
}
