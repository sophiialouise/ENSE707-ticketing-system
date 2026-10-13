# ENSE707 Ticketing System

This repository contains the final prototype developed for ENSE707 Software Quality Assurance.

The project is a Ticketing and Customer Support Analytics System designed for a university IT helpdesk. It is built using C#/.NET and ASP.NET Core MVC.

## Requirements

To run the project, install:

- .NET 10 SDK
- a web browser

## Project Structure

- `TicketingSystem/` - core models, services, sample ticket data, and the original console prototype
- `TicketingSystem.Web/` - ASP.NET Core MVC web application
- `TicketingSystem.Tests/` - MSTest automated test suite
- `tools/` - development helper utilities, including password-hash generation
- `TicketingSystem.slnx` - solution file

## Running the Project

Open a terminal in the repository root folder.

The repository root is the folder containing:

`TicketingSystem.slnx`

Run the following steps in order.

### 1. Build the Solution

```bash
dotnet build TicketingSystem.slnx
```

The build should complete successfully before continuing.

### 2. Run the Automated Tests

```bash
dotnet run --project TicketingSystem.Tests/TicketingSystem.Tests.csproj
```
The final automated regression suite contains 28 MSTest tests.

The test project contains an automated MSTest suite covering ticket validation, calculations, filtering, workflow behaviour, ticket history, and export privacy.

### 3. Authentication

The prototype includes two local demonstration accounts:

- `support` - Support role
- `manager` - Manager role

ASP.NET Core verifies login passwords against salted password hashes.

For the current academic prototype, the demonstration password hashes are stored in the application's configuration. Readable passwords are not committed to the repository.

The readable demonstration passwords are shared with the project team separately for local testing.

This configuration is prototype-only. Storing the demo hashes in the repository is recorded as a known limitation in the final SQA report, and a production system would use an organisational identity provider or secure credential-management solution.

Developers may optionally override the configured hashes locally using .NET User Secrets.

### 4. Run the Web Application

```bash
dotnet run --project TicketingSystem.Web/TicketingSystem.Web.csproj
```

The terminal will display the local address used by the web application.

A typical development address is:

```text
http://localhost:5122
```

Use the address shown in the terminal if the port is different.

Open the address in a web browser if it does not open automatically.

When the application starts, protected pages redirect unauthenticated users to the login page.

### 5. Log In

Use one of the two locally configured demonstration accounts:

```text
Username: support
Role: Support
```

or:

```text
Username: manager
Role: Manager
```

Use the corresponding password that was selected when generating the account's password hash.

The Support role can access the Dashboard, Tickets, ticket details, status functions, filtering, and privacy-restricted CSV exports.

The Manager role can access the same functions and also has access to Manager-restricted functionality such as ticket import and exports containing authorised requester details.

## Loading Sample Ticket Data

The prototype uses in-memory storage, so ticket data is reset when the application is stopped.

Ticket import is restricted to the Manager role.

To load the provided sample tickets:

1. Log in using the `manager` demonstration account.
2. Open the **Import** page.
3. Select `TicketingSystem/sample_tickets.csv`.
4. Click **Import Tickets**.
5. Open the **Dashboard** or **Tickets** page.

The sample data can be used to demonstrate:

- CSV ticket import and validation
- dashboard metrics
- dashboard filtering
- ticket filtering
- ticket details
- ticket status updates
- ticket history
- response and resolution calculations
- SLA calculations
- role-based access control
- filtered CSV export

## Authentication and Roles

The prototype uses ASP.NET Core cookie authentication.

Two roles are implemented:

### Support

Support users can:

- access the Dashboard
- view and filter tickets
- view ticket details and history
- update ticket status
- export filtered ticket information

Support CSV exports exclude requester names and email addresses to protect requester privacy.

### Manager

Manager users can:

- access all Support functionality
- access the ticket Import page
- import ticket CSV files
- export filtered ticket information including authorised requester details

Access restrictions are enforced by server-side authorization rather than relying only on hidden navigation links.

## Final Prototype Features

The completed prototype includes:

- CSV ticket import and validation
- valid and invalid record reporting
- rejection of records with invalid or missing required data
- dashboard ticket metrics
- dashboard filtering by date, assigned staff, category, and channel
- ticket listing and filtering
- date-range filtering
- ticket details
- ticket status updates
- ticket history
- response-time calculations
- resolution-time calculations
- SLA calculations
- cookie-based authentication
- Support and Manager role-based authorization
- password verification using generated password hashes
- filtered CSV ticket export
- privacy-aware export behaviour based on user role
- automated MSTest coverage
- ASP.NET Core MVC graphical user interface

## Password Security

Prototype passwords are verified using ASP.NET Core password hashing.

Only generated password hashes, rather than readable passwords, are stored in application configuration.

Readable passwords are:

- not stored in the repository
- not stored in application source code
- not stored in application configuration
- not committed to Git

Authentication compares the password entered during login against the stored password hash.

This approach is suitable for the local academic prototype. A production system would use a dedicated organisational identity and credential-management solution.

## Current Prototype Limitations

The system is an academic prototype designed for local execution with sample data.

Current limitations include:

- ticket data is stored in memory and is lost when the application stops
- prototype user accounts are locally configured rather than connected to a university identity provider
- persistent database storage is not implemented
- production deployment is not implemented
- institutional single sign-on is not implemented
- multi-factor authentication is not implemented
- HTTPS is not fully configured for the local development environment
- concurrency testing was limited to the Task 7 stress scenario and does not prove full thread safety
- requester details remain visible to Support users on screen
- repeated failed logins are not currently locked out or security-logged
- some dashboard and ticket filter controls still lack programmatic accessibility labels
- header-only or unrecognised-header CSV imports can report success with zero imported records
- the Tickets page does not currently use pagination

## Data Storage

Ticket data is currently stored in memory using the core ticket service.

This approach was retained because the application is a local academic prototype using sample data.

Imported tickets, status changes, and ticket history therefore remain available only while the application is running.

Persistent database storage is identified as a future improvement.

## Stopping the Application

To stop the running web application, return to the terminal and press:

```text
Control + C
```

## Project Context

This prototype was originally developed during Assessment 1 and completed during Assessment 2 of ENSE707 Software Quality Assurance.

Assessment 2 development focused on completing previously unfinished requirements and strengthening software quality through:

- validation improvements
- regression testing
- completed filtering
- authentication
- role-based authorization
- export privacy
- automated testing
- traceability
- quality evidence
- documentation improvements

Further SQA activities, including final test execution, CI/CD, defect analysis, quality testing, metrics, and release evaluation, are documented in the Assessment 2 final report.