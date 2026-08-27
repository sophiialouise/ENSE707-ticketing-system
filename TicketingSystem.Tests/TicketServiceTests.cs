using TicketingSystem.Models;
using TicketingSystem.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TicketingSystem.Tests;

[TestClass]
public class TicketServiceTests
{
    [TestMethod]
    public void ImportTicketsFromCsv_ValidData_ImportsCorrectly()
    {
        // arrange
        var service = new TicketService();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "CustomerName,CustomerEmail,Category,Priority,Status,AssignedTo,Channel,Description,CreatedAt,ResolvedAt\nTest User,test@email.com,Support,High,Open,Alice,Email,Test ticket,2026-07-01 10:00:00,");

        // act
        var result = service.ImportTicketsFromCsv(tempFile);

        // Assert
        Assert.AreEqual(1, result.ValidRecords);
        Assert.AreEqual(0, result.InvalidRecords);
        File.Delete(tempFile);
    }

    [TestMethod]
    public void ImportTicketsFromCsv_MissingEmail_FlagsAsInvalid()
    {
        // arrange
        var service = new TicketService();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "CustomerName,CustomerEmail,Category,Priority,Status,AssignedTo,Channel,Description,CreatedAt,ResolvedAt\nTest User,,Support,High,Open,Alice,Email,Test ticket,2026-07-01 10:00:00,");

        // act
        var result = service.ImportTicketsFromCsv(tempFile);

        // assert
        Assert.AreEqual(0, result.ValidRecords);
        Assert.AreEqual(1, result.InvalidRecords);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("email")));
        File.Delete(tempFile);
    }

    [TestMethod]
    public void UpdateTicketStatus_ValidId_UpdatesStatus()
    {
        // arrange
        var service = new TicketService();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "CustomerName,CustomerEmail,Category,Priority,Status,AssignedTo,Channel,Description,CreatedAt,ResolvedAt\nTest User,test@email.com,Support,High,Open,Alice,Email,Test ticket,2026-07-01 10:00:00,");
        var result = service.ImportTicketsFromCsv(tempFile);
        var ticketId = result.ValidTickets.First().Id;

        // act
        var success = service.UpdateTicketStatus(ticketId, "Resolved");

        // Assert
        Assert.IsTrue(success);
        var ticket = service.GetTicketById(ticketId);
        Assert.AreEqual("Resolved", ticket?.Status);
        Assert.IsNotNull(ticket?.ResolvedAt);
        File.Delete(tempFile);
    }

    [TestMethod]
    public void GetDashboard_ReturnsCorrectCounts()
    {
        // arrange
        var service = new TicketService();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "CustomerName,CustomerEmail,Category,Priority,Status,AssignedTo,Channel,Description,CreatedAt,ResolvedAt\nUser1,test@email.com,Support,High,Open,Alice,Email,Ticket1,2026-07-01 10:00:00,\nUser2,test2@email.com,Billing,Low,Resolved,Bob,Phone,Ticket2,2026-07-02 10:00:00,2026-07-03 10:00:00");
        service.ImportTicketsFromCsv(tempFile);

        // act
        var dashboard = service.GetDashboard();

        // assert
        Assert.AreEqual(2, dashboard.TotalTickets);
        Assert.AreEqual(1, dashboard.OpenTickets);
        Assert.AreEqual(1, dashboard.ResolvedTickets);
        File.Delete(tempFile);
    }

    [TestMethod]
    public void SlaReport_CalculatesCorrectCompliance()
    {
        // arrange
        var service = new TicketService();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "CustomerName,CustomerEmail,Category,Priority,Status,AssignedTo,Channel,Description,CreatedAt,ResolvedAt\nUser1,test@email.com,Support,Critical,Resolved,Alice,Email,Ticket1,2026-07-01 10:00:00,2026-07-01 13:00:00\nUser2,test2@email.com,Billing,High,Resolved,Bob,Phone,Ticket2,2026-07-02 10:00:00,2026-07-03 10:00:00");
        service.ImportTicketsFromCsv(tempFile);

        // act
        var report = service.GetSlaReport();

        // assert

        // critical sla is 4 hours, so the first ticket should pass
        // high sla is 8 hours, so the second ticket should fail
        // expected compliance is 1 out of 2 tickets, or 50%
        var compliance = report.GetType().GetProperty("CompliancePercentage")?.GetValue(report);
        Assert.AreEqual(50.0, Convert.ToDouble(compliance), 0.1);
        File.Delete(tempFile);
    }


    [TestMethod]
    public void GetTicketHistory_ThroughLifecycle_TracesCreationToResolution()
    {
        // arrange
        var service = new TicketService();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "CustomerName,CustomerEmail,Category,Priority,Status,AssignedTo,Channel,Description,CreatedAt,ResolvedAt\nTest User,test@email.com,Support,High,Open,Alice,Email,Test ticket,2026-07-01 10:00:00,");
        var result = service.ImportTicketsFromCsv(tempFile);
        var ticketId = result.ValidTickets.First().Id;
        service.UpdateTicketStatus(ticketId, "In Progress");
        service.UpdateTicketStatus(ticketId, "Resolved");

        // act
        var history = service.GetTicketHistory(ticketId);

        // assert
        Assert.AreEqual(3, history.Count);

        var events = history
            .Select(h => h.GetType().GetProperty("Event")?.GetValue(h)?.ToString())
            .ToList();

        CollectionAssert.Contains(events, "Ticket created");
        CollectionAssert.Contains(events, "First response");
        CollectionAssert.Contains(events, "Ticket resolved");
        File.Delete(tempFile);
    }


    [TestMethod]
    public void UpdateTicketStatus_ThroughLifecycle_RecordsResponseAndResolutionTimes()
    {
        // arrange
        var service = new TicketService();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "CustomerName,CustomerEmail,Category,Priority,Status,AssignedTo,Channel,Description,CreatedAt,ResolvedAt\nTest User,test@email.com,Support,High,Open,Alice,Email,Test ticket,2026-07-01 10:00:00,");
        var result = service.ImportTicketsFromCsv(tempFile);
        var ticketId = result.ValidTickets.First().Id;

        // act
        service.UpdateTicketStatus(ticketId, "In Progress");
        service.UpdateTicketStatus(ticketId, "Resolved");
        var ticket = service.GetTicketById(ticketId);

        // assert
        Assert.IsNotNull(ticket?.FirstResponseAt);
        Assert.IsNotNull(ticket?.ResolvedAt);
        Assert.IsTrue(ticket!.ResponseTimeHours >= 0);
        Assert.IsTrue(ticket.ResolutionTimeHours >= ticket.ResponseTimeHours);
        File.Delete(tempFile);
    }

}