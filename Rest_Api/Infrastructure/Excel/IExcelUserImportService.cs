using Models;

namespace Rest_Api.Infrastructure.Excel
{
    public interface IExcelUserImportService
    {
        Task<ExcelImportResult> ImportFromFileAsync(string filePath, string batchId, CancellationToken cancellationToken = default);
        string GetBatchResultPath(string batchId);
        ExcelImportResult? ReadBatchResult(string batchId);
        void SaveBatchResult(ExcelImportResult result);
    }
}
