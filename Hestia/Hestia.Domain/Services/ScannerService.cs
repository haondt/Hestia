using Hestia.Domain.Models;
using Hestia.Persistence.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;

namespace Hestia.Domain.Services
{
    public class ScannerService<T>(
        IScanTextExtractor<T> textExtractor,
        IScanTextTransformer<T> textTransformer,
        IScanProcessingStateService<T> stateService,
        IOptions<PersistenceSettings> persistenceOptions,
        IOptions<NutritionLabelScannerSettings> scannerOptions,
        ILogger<ScannerService<T>> logger) : IScannerService<T> where T : IScannedData
    {
        private readonly PersistenceSettings _persistenceSettings = persistenceOptions.Value;
        private readonly NutritionLabelScannerSettings _scannerSettings = scannerOptions.Value;

        public async Task ProcessLabelImageAsync(Stream imageStream, Guid processingId)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Step 1: Process and save the image
                stateService.UpdateProcessingStatus(processingId, "Processing image...");

                using var image = await SixLabors.ImageSharp.Image.LoadAsync(imageStream);

                // Resize if needed
                var maxSize = 1024;
                if (image.Width > maxSize || image.Height > maxSize)
                {
                    var ratio = Math.Min((double)maxSize / image.Width, (double)maxSize / image.Height);
                    var newWidth = (int)(image.Width * ratio);
                    var newHeight = (int)(image.Height * ratio);
                    image.Mutate(x => x.Resize(newWidth, newHeight));
                }

                var encoder = new JpegEncoder { Quality = 85 };
                if (_scannerSettings.SaveTrainingData)
                {
                    var imagesDirectory = Path.Combine(_persistenceSettings.FileDataPath, "nutrition-labels");
                    Directory.CreateDirectory(imagesDirectory);

                    var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                    var uniqueId = Guid.NewGuid().ToString("N")[..8];
                    var fileName = $"nutrition-label-{timestamp}-{uniqueId}.jpg";
                    var filePath = Path.Combine(imagesDirectory, fileName);

                    await image.SaveAsJpegAsync(filePath, encoder);
                }

                // Step 2: Extract text using the configured OCR provider
                using var memoryStream = new MemoryStream();
                await image.SaveAsJpegAsync(memoryStream, encoder);
                memoryStream.Position = 0;

                var extractedText = await textExtractor.ExtractTextAsync(memoryStream, processingId);

                // Step 3: Transform text to structured data using the configured LLM provider
                var scannedLabel = await textTransformer.TransformText(extractedText, processingId);

                stopwatch.Stop();
                scannedLabel.ProcessingDuration = stopwatch.Elapsed;
                stateService.SetProcessingResult(processingId, scannedLabel);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Failed to process nutrition label for processing ID {ProcessingId}", processingId);
                stateService.SetProcessingFailure(processingId, $"Error processing image: {ex.Message}");
            }
        }

        public Guid StartBackgroundProcessing(Stream imageStream)
        {
            var processingId = stateService.StartProcessing();
            stateService.UpdateProcessingStatus(processingId, "Processing image...");

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
                    logger.LogError(ex, "Background processing failed for processing ID {ProcessingId}", processingId);
                    stateService.SetProcessingFailure(processingId, $"Error processing image: {ex.Message}");
                }
                finally
                {
                    memoryStream.Dispose();
                }
            });

            return processingId;
        }
    }
}