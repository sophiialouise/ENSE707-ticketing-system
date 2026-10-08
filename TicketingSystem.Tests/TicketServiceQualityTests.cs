using TicketingSystem.Services;

namespace TicketingSystem.Tests;

// task 7 quality tests: security (csv export) and reliability (concurrent access)
[TestClass]
public class TicketServiceQualityTests
{
    private readonly List<string> _tempFiles = new();

    private string CreateCsv(IEnumerable<string> rows)
    {
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);

        const string header =
            "CustomerName,CustomerEmail,Category,Priority,Status," +
            "AssignedTo,Channel,Description,CreatedAt,ResolvedAt";

        File.WriteAllLines(
            tempFile,
            new[] { header }.Concat(rows));

        return tempFile;
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        _tempFiles.Clear();
    }

    // TC39 - a formula in a ticket field must not be exported as a live spreadsheet formula
    [TestMethod]
    public void ExportTicketsToCsv_FormulaInDescription_IsEscaped()
    {
        // arrange
        var service = new TicketService();

        var file = CreateCsv(new[]
        {
            "Test User,test@email.com,Support,High,Open,Alice,Email," +
            "\"=HYPERLINK(\"\"http://example.com\"\",\"\"click\"\")\"," +
            "2026-07-01 10:00:00,"
        });

        service.ImportTicketsFromCsv(file);

        // act
        var csv = service.ExportTicketsToCsv();

        // assert - the field must not start with = once exported
        Assert.IsFalse(
            csv.Contains(",=HYPERLINK") || csv.Contains(",\"=HYPERLINK"),
            "Formula was exported unescaped.");

        StringAssert.Contains(csv, "'=HYPERLINK");
    }

    // TC43 - parallel imports and dashboard reads must not throw or create duplicate ids
    [TestMethod]
    public void TicketService_ConcurrentImportAndRead_NoErrorsAndUniqueIds()
    {
        const int iterations = 20;
        const int importers = 8;
        const int rowsPerFile = 200;

        var rows = Enumerable.Range(1, rowsPerFile)
            .Select(i =>
                $"User {i},user{i}@email.com,Billing,Medium,Open," +
                "Bob,Phone,Load ticket,2026-07-01 10:00:00,");

        var file = CreateCsv(rows);
        var failedIterations = 0;

        for (var run = 0; run < iterations; run++)
        {
            // arrange
            var service = new TicketService();
            var errors = new System.Collections.Concurrent.ConcurrentBag<string>();
            var importsDone = 0;

            // act - importers and readers run at the same time
            var importTasks = Enumerable.Range(0, importers)
                .Select(_ => Task.Run(() =>
                {
                    var result = service.ImportTicketsFromCsv(file);
                    errors.Add(string.Join(";", result.Errors));
                    Interlocked.Increment(ref importsDone);
                }));

            var readTasks = Enumerable.Range(0, importers)
                .Select(_ => Task.Run(() =>
                {
                    while (Volatile.Read(ref importsDone) < importers)
                    {
                        try
                        {
                            service.GetDashboard();
                        }
                        catch (Exception ex)
                        {
                            errors.Add(ex.GetType().Name);
                        }
                    }
                }));

            Task.WaitAll(importTasks.Concat(readTasks).ToArray());

            var tickets = service.GetTickets();

            var failed =
                errors.Any(e => !string.IsNullOrEmpty(e)) ||
                tickets.Count != importers * rowsPerFile ||
                tickets.Select(t => t.Id).Distinct().Count() != tickets.Count;

            if (failed)
            {
                failedIterations++;
            }
        }

        // assert
        Assert.AreEqual(
            0,
            failedIterations,
            $"Concurrent access failed in {failedIterations} of {iterations} runs.");
    }
}
