# ENSE707 Ticketing System

This repository contains the initial prototype developed for ENSE707 Software Quality Assurance.

The project is a Ticketing and Customer Support Analytics System designed for a university IT helpdesk. It is built using C#/.NET and ASP.NET Core MVC.

## Requirements

To run the project, install:

- .NET 10 SDK
- a web browser

## Project Structure

- `TicketingSystem/` - core models, services, and the original console prototype
- `TicketingSystem.Web/` - ASP.NET Core MVC web application
- `TicketingSystem.Tests/` - MSTest automated tests
- `TicketingSystem.slnx` - solution file

## Running the Project

Open a terminal in the repository root folder.

The repository root is the folder containing:

`TicketingSystem.slnx`

Run the following commands in order.

### 1. Build the Solution

```bash
dotnet build TicketingSystem.slnx
```

The build should complete successfully before continuing.

### 2. Run the Automated Tests

```bash
dotnet test TicketingSystem.slnx
```

The test project contains nine automated MSTest tests.

### 3. Run the Web Application

```bash
dotnet run --project TicketingSystem.Web/TicketingSystem.Web.csproj
```

The terminal will display the local address that the web application is using.

The default HTTP address is:

`http://localhost:5122`

Open this address in a web browser if it does not open automatically.

## Loading Sample Ticket Data

The prototype uses in-memory storage, so ticket data is reset when the application is stopped.

To load the provided sample tickets:

1. Open the **Import** page.
2. Select `TicketingSystem/sample_tickets.csv`.
3. Click **Import Tickets**.
4. Open the **Dashboard** or **Tickets** page.

The sample data can be used to demonstrate dashboard metrics, ticket filtering, ticket details, status updates, ticket history, and SLA calculations.

## Current Prototype Features

- CSV ticket import and validation
- valid and invalid record reporting
- dashboard ticket metrics
- ticket listing and filtering
- ticket details
- ticket status updates
- ticket history
- response and resolution time calculations
- SLA calculations
- automated MSTest coverage

## Current Limitations

This is a mid-project prototype.

The following planned features are not yet complete:

- login and role-based access
- restricted export functionality
- persistent database storage
- some planned dashboard filters

## Stopping the Application

To stop the running web application, return to the terminal and press:

`Control + C`