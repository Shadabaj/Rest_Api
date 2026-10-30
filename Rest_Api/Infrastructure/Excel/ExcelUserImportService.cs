using System.Text.Json;
using ClosedXML.Excel;
using DAL;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Rest_Api.Infrastructure.Excel
{
    public class ExcelUserImportService : IExcelUserImportService
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ExcelUserImportService> _logger;

        public ExcelUserImportService(AppDbContext db, IWebHostEnvironment env, ILogger<ExcelUserImportService> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        public string GetBatchResultPath(string batchId)
        {
            var dir = Path.Combine(_env.ContentRootPath, "App_Data", "imports");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"{batchId}-result.json");
        }

        public void SaveBatchResult(ExcelImportResult result)
        {
            var path = GetBatchResultPath(result.BatchId);
            File.WriteAllText(path, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        }

        public ExcelImportResult? ReadBatchResult(string batchId)
        {
            var path = GetBatchResultPath(batchId);
            if (!File.Exists(path))
                return null;
            return JsonSerializer.Deserialize<ExcelImportResult>(File.ReadAllText(path));
        }

        public async Task<ExcelImportResult> ImportFromFileAsync(string filePath, string batchId, CancellationToken cancellationToken = default)
        {
            var result = new ExcelImportResult { BatchId = batchId };

            using var workbook = new XLWorkbook(filePath);
            var sheet = workbook.Worksheets.First();
            var header = sheet.FirstRowUsed() ?? throw new InvalidOperationException("Excel sheet is empty.");

            var colMap = BuildHeaderMap(header);

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var employeeCode = GetCell(row, colMap, "EmployeeCode", "UserName", "Code")?.Trim().ToUpperInvariant();
                    if (string.IsNullOrWhiteSpace(employeeCode))
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    var employee = await _db.EmployeeMasters
                        .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode && !e.IsDeleted, cancellationToken);

                    if (employee == null)
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"{employeeCode}: employee not found.");
                        continue;
                    }

                    var firstName = GetCell(row, colMap, "FirstName", "First Name");
                    var lastName = GetCell(row, colMap, "LastName", "Last Name");
                    var email = GetCell(row, colMap, "EmailAddress", "Email", "EmailId");
                    var mobile = GetCell(row, colMap, "MobileNumber", "Mobile", "Phone");
                    var isActiveRaw = GetCell(row, colMap, "IsActive", "Active");

                    if (!string.IsNullOrWhiteSpace(firstName))
                        employee.FirstName = firstName.Trim();
                    if (!string.IsNullOrWhiteSpace(lastName))
                        employee.LastName = lastName.Trim();
                    if (!string.IsNullOrWhiteSpace(email))
                        employee.EmailAddress = email.Trim();
                    if (!string.IsNullOrWhiteSpace(mobile))
                        employee.MobileNumber = mobile.Trim();
                    if (!string.IsNullOrWhiteSpace(isActiveRaw)
                        && bool.TryParse(isActiveRaw, out var isActive))
                        employee.IsActive = isActive;

                    employee.UpdatedDate = DateTime.UtcNow;
                    result.UpdatedCount++;

                    if (!string.IsNullOrWhiteSpace(employee.EmailAddress))
                    {
                        result.WelcomeRecipients.Add(new WelcomeRecipient
                        {
                            EmployeeCode = employee.EmployeeCode,
                            FullName = $"{employee.FirstName} {employee.LastName}".Trim(),
                            Email = employee.EmailAddress
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.SkippedCount++;
                    result.Errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    _logger.LogWarning(ex, "Excel import row {Row} failed", row.RowNumber());
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            SaveBatchResult(result);
            _logger.LogInformation(
                "Excel import batch {BatchId}: updated {Updated}, skipped {Skipped}",
                batchId, result.UpdatedCount, result.SkippedCount);
            return result;
        }

        private static Dictionary<string, int> BuildHeaderMap(IXLRow header)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in header.CellsUsed())
            {
                var name = cell.GetString().Trim();
                if (!string.IsNullOrWhiteSpace(name))
                    map[name] = cell.Address.ColumnNumber;
            }
            return map;
        }

        private static string? GetCell(IXLRow row, Dictionary<string, int> map, params string[] names)
        {
            foreach (var name in names)
            {
                if (map.TryGetValue(name, out var col))
                    return row.Cell(col).GetString();
            }
            return null;
        }
    }
}
