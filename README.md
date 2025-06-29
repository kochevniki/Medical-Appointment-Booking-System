# Medical Booking Service

## Overview

The Medical Booking Service is a comprehensive platform designed to streamline the process of scheduling and managing medical appointments. It caters to patients, doctors, and administrative staff, offering a user-friendly interface and a range of features to facilitate efficient healthcare management.

## Key Features

-   **Patient Sign-Up and Authentication:** Secure registration and login functionality for patients.
-   **Appointment Scheduling:** Patients can view available time slots and book appointments with doctors in various departments.
-   **Admin Dashboard:** Administrative staff can manage departments, doctors, and approve/reject appointments.
-   **Doctor Interface:** Doctors can view their schedules and manage appointments.
-   **Department Selection:** Patients can select a specific medical department for booking appointments.
-   **Role-Based Access Control:** Different user roles (Admin, Doctor, Patient) have access to specific features and functionalities.
-   **Real-time Calendar:** A dynamic calendar view displays scheduled appointments and available slots.
-   **Modals:** Interactive modals for managing existing appointments and booking new ones.
-   **Admin Block Slots:** Admins can block time slots for various reasons.

## Technologies Used

- **Blazor WebAssembly** – Frontend UI framework
- **.NET 9** – Backend framework using ASP.NET Core
- **C#** – Primary programming language
- **Radzen Blazor** – UI components and controls
- **ASP.NET Core Identity** – Authentication and user management
- **Entity Framework Core** – ORM for database access
- **SQL Server** – Relational database
- **Google Places API** – Address autocomplete/typeahead in patient registration
- **Box API** – File management and cloud storage integration for medical documents
- **RESTful API** – Communication between client and server


## Setup Instructions

1.  **Prerequisites:**
    -   .NET 9 SDK
    -   SQL Server

2.  **Clone the Repository:**

    ```bash
    git clone [repository URL]
    cd [repository directory]
    ```

3.  **Database Configuration:**
    -   Update the connection string in `MedicalBookingService.Server/appsettings.json` with your SQL Server details.

    ```json
    "ConnectionStrings": {
        "DefaultConnection": "Server=[YourServer];Database=[YourDatabase];User Id=[YourUserId];Password=[YourPassword];TrustServerCertificate=True"
    }
    ```

4.  **Apply Migrations:**
    -   Navigate to the `MedicalBookingService.Server` directory in the console.

    ```bash
    dotnet ef database update
    ```

5.  **Build and Run the Application:**
    -   Build the solution.

    ```bash
    dotnet build
    ```

    -   Run the server project.

    ```bash
    dotnet run --project MedicalBookingService.Server
    ```

6.  **Client Application:**
    -   The client application will be accessible at `https://localhost:8080`.

## Project Structure

-   **MedicalBookingService.Client:** Contains the Blazor WebAssembly client application.
-   **MedicalBookingService.Server:** Contains the ASP.NET Core server-side application and API controllers.
-   **MedicalBookingService.Shared:** Contains shared models and DTOs used by both client and server.