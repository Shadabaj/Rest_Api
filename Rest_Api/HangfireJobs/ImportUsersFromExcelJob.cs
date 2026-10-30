using Rest_Api.Infrastructure.Excel;

namespace Rest_Api.HangfireJobs
{
    /// <summary>Enqueue = Now — import/update users from uploaded Excel.</summary>
    public class ImportUsersFromExcelJob
    {
        private readonly IExcelUserImportService _import;
        private readonly ILogger<ImportUsersFromExcelJob> _logger;

        public ImportUsersFromExcelJob(IExcelUserImportService import, ILogger<ImportUsersFromExcelJob> logger)
        {
            _import = import;
            _logger = logger;
        }

        public async Task Execute(string filePath, string batchId)
        {
            _logger.LogInformation("Starting Excel import batch {BatchId}", batchId);
            var result = await _import.ImportFromFileAsync(filePath, batchId);
            _logger.LogInformation(
                "Finished Excel import batch {BatchId}: {Updated} updated",
                batchId, result.UpdatedCount);
        }
    }
}
