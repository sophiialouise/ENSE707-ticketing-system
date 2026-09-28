using TicketingSystem.Models;
using TicketingSystem.Services;

namespace TicketingSystem.Tests;

[TestClass]
public class TicketServiceAutomationTests
{
    private readonly List<string> _tempFiles = new();

    private string CreateCsv(params string[] rows)
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

    [TestMethod]
    public void GetDashboard_DateRangeFilter_IncludesEntireEndDate()
    {
        // arrange
        var service = new TicketService();

        var file = CreateCsv(
            "User1,user1@email.com,Support,High,Open,Alice,Email,Ticket1,2026-07-01 10:00:00,",
            "User2,user2@email.com,Support,High,Open,Alice,Email,Ticket2,2026-07-02 23:59:00,",
            "User3,user3@email.com,Support,High,Open,Alice,Email,Ticket3,2026-07-03 00:00:00,");

        service.ImportTicketsFromCsv(file);

        var filter = new TicketFilter
        {
            StartDate = new DateTime(2026, 7, 1),
            EndDate = new DateTime(2026, 7, 2)
        };

        // act
        var dashboard = service.GetDashboard(filter);

        // assert
        Assert.AreEqual(2, dashboard.TotalTickets);
    }

    [TestMethod]
    public void GetDashboard_FilteredByChannel_ReturnsOnlyMatchingTickets()
    {
        // arrange
        var service = new TicketService();

        var file = CreateCsv(
            "User1,user1@email.com,Support,High,Open,Alice,Email,Ticket1,2026-07-01 10:00:00,",
            "User2,user2@email.com,Support,Low,Open,Bob,Phone,Ticket2,2026-07-02 10:00:00,");

        service.ImportTicketsFromCsv(file);

        var filter = new TicketFilter
        {
            Channel = "Email"
        };

        // act
        var dashboard = service.GetDashboard(filter);

        // assert
        Assert.AreEqual(1, dashboard.TotalTickets);
        Assert.AreEqual(1, dashboard.TicketsByChannel["Email"]);
        Assert.IsFalse(dashboard.TicketsByChannel.ContainsKey("Phone"));
    }

    [TestMethod]
    public void ImportTicketsFromCsv_UnsupportedPriority_FlagsAsInvalid()
    {
        // arrange
        var service = new TicketService();

        var file = CreateCsv(
            "Test User,test@email.com,Support,Urgent,Open,Alice,Email,Test ticket,2026-07-01 10:00:00,");

        // act
        var result = service.ImportTicketsFromCsv(file);

        // assert
        Assert.AreEqual(0, result.ValidRecords);
        Assert.AreEqual(1, result.InvalidRecords);
        StringAssert.Contains(
            result.Errors.First(),
            "priority");
    }

    [TestMethod]
    public void ImportTicketsFromCsv_UnsupportedStatus_FlagsAsInvalid()
    {
        // arrange
        var service = new TicketService();

        var file = CreateCsv(
            "Test User,test@email.com,Support,High,Pending,Alice,Email,Test ticket,2026-07-01 10:00:00,");

        // act
        var result = service.ImportTicketsFromCsv(file);

        // assert
        Assert.AreEqual(0, result.ValidRecords);
        Assert.AreEqual(1, result.InvalidRecords);
        StringAssert.Contains(
            result.Errors.First(),
            "status");
    }

    [TestMethod]
    public void UpdateTicketStatus_ResolvedThenClosed_PreservesOriginalResolvedAt()
    {
        // arrange
        var service = new TicketService();

        var file = CreateCsv(
            "Test User,test@email.com,Support,High,Resolved,Alice,Email,Test ticket,2026-07-01 10:00:00,2026-07-01 14:00:00");

        var importResult =
            service.ImportTicketsFromCsv(file);

        var ticketId =
            importResult.ValidTickets.First().Id;

        var originalResolvedAt =
            service.GetTicketById(ticketId)!.ResolvedAt;

        // act
        var updated =
            service.UpdateTicketStatus(
                ticketId,
                "Closed");

        var ticket =
            service.GetTicketById(ticketId);

        // assert
        Assert.IsTrue(updated);
        Assert.AreEqual("Closed", ticket!.Status);
        Assert.AreEqual(
            originalResolvedAt,
            ticket.ResolvedAt);
    }

    [TestMethod]
    public void GetSlaReport_ReopenedTicket_IsNotCountedAsResolved()
    {
        // arrange
        var service = new TicketService();

        var file = CreateCsv(
            "Test User,test@email.com,Support,High,Resolved,Alice,Email,Test ticket,2026-07-01 10:00:00,2026-07-01 14:00:00");

        var importResult =
            service.ImportTicketsFromCsv(file);

        var ticketId =
            importResult.ValidTickets.First().Id;

        service.UpdateTicketStatus(
            ticketId,
            "Open");

        // act
        var report = service.GetSlaReport();

        var totalResolved =
            report.GetType()
                .GetProperty("TotalResolved")
                ?.GetValue(report);

        // assert
        Assert.AreEqual(
            0,
            Convert.ToInt32(totalResolved));
    }

    [TestMethod]
    public void ImportTicketsFromCsv_ResolvedWithoutResolvedAt_FlagsAsInvalid()
    {
        // arrange
        var service = new TicketService();

        var file = CreateCsv(
            "Test User,test@email.com,Support,High,Resolved,Alice,Email,Test ticket,2026-07-01 10:00:00,");

        // act
        var result = service.ImportTicketsFromCsv(file);

        // assert
        Assert.AreEqual(0, result.ValidRecords);
        Assert.AreEqual(1, result.InvalidRecords);

        StringAssert.Contains(
            result.Errors.First().ToLowerInvariant(),
            "resolved time");
    }

    [TestMethod]
    public void ImportTicketsFromCsv_OpenWithResolvedAt_FlagsAsInvalid()
    {
        // arrange
        var service = new TicketService();

        var file = CreateCsv(
            "Test User,test@email.com,Support,High,Open,Alice,Email,Test ticket,2026-07-01 10:00:00,2026-07-01 14:00:00");

        // act
        var result = service.ImportTicketsFromCsv(file);

        // assert
        Assert.AreEqual(0, result.ValidRecords);
        Assert.AreEqual(1, result.InvalidRecords);

        StringAssert.Contains(
            result.Errors.First().ToLowerInvariant(),
            "resolved time");
    }
}