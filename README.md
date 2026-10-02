# University Management System

A web-based university management system built with ASP.NET Core MVC, Entity Framework Core, and SQL Server. The application supports three user roles (Staff, Professor, Student), a flexible permission system, exam creation and grading, assignment submission, and grade reporting.

## Requirements

- **.NET 10 SDK** — Download from: https://dotnet.microsoft.com/download/dotnet/10.0
- **Docker Desktop** — Download from: https://www.docker.com/products/docker-desktop
- **SQL Server 2022** — Provided automatically via Docker (no manual install needed)
- Optional: **Visual Studio Code** with the C# Dev Kit extension — https://code.visualstudio.com

## Setup and Running

### Step 1 — Clone the repository

git clone https://github.com/YOUR_USERNAME/UniversityManagementSystem.git
cd UniversityManagementSystem

### Step 2 — Start SQL Server with Docker

docker compose up -d sqlserver

Wait about 45 seconds for SQL Server to become ready.

### Step 3 — Apply database migrations

dotnet restore
dotnet ef database update

This will create the database, all tables, permissions, and seed an initial admin user.

### Step 4 — Run the application

dotnet run

Open your browser at:
http://localhost:5023

### Alternative — Run everything with Docker

docker compose up --build -d

Then open:
http://localhost:5023

To stop:
docker compose stop

To remove all containers and data:
docker compose down -v

## Default Credentials

After the first run, the following user is created automatically:

| Field    | Value      |
|----------|------------|
| Username | admin      |
| Password | admin123   |
| Role     | Staff      |

Fixed codes for self-registration:

| Role      | Fixed Code |
|-----------|------------|
| Staff     | STAFF001   |
| Professor | PROF001    |
| Student   | STU001     |

## Features

### Authentication
- Login and logout with session-based authentication
- Self-registration with a fixed code per role
- Admin-created users with default permissions per role
- SuperAdmin flag for staff members who registered with a fixed code
- Access protection on every endpoint based on role and permissions

### Permission System
- 21 granular permissions grouped by category (Users, Courses, Exams, Assignments, Grades, Student)
- SuperAdmin bypasses all permission checks
- Staff members with the "Manage Permissions" permission can grant or revoke any permission from any user
- Users cannot modify their own permissions
- Permission changes take effect after the affected user logs in again

### User Management (Staff)
- View all users
- Create new users (Student, Professor, Staff)
- Delete users
- Edit permissions per user with a categorized checkbox interface

### Course Management (Staff)
- Create courses with the following fields:
  - Course code (unique)
  - Course name
  - Assigned professor
  - Class time
  - Class location
  - Start date
  - End date
  - Final exam date
  - Academic year
  - Enrolled students
- View course details including the full student list
- Delete courses

### Exam Management (Professor)
- Create exams with multiple question types:
  - Multiple choice (with correctness flags)
  - True/False
  - Descriptive
  - Fill in the blank
- Set a maximum score per exam (default 20)
- Edit exam metadata and questions (questions can only be changed if no student has answered yet)
- Delete exams

### Exam Grading (Professor)
- Single-student grading: assign a grade to each question manually
- Bulk grading: display all students in one table with a sticky first column, auto-sum per row
- Automatic calculation of final course grade (sum of exam grades + assignment grades, capped at 20)
- Automatic Upsert into the Grades table

### Assignment Management (Professor)
- View all assignment submissions from students
- Download submitted files
- Assign grades
- Automatic recalculation of the student's total assignment grade

### Grade Editing (Professor)
- Edit assignment grade, exam grade, and final grade per student
- Final grade is auto-calculated if left empty
- Grades are capped at 20

### Reports (Professor and Staff)
- View grades across all courses (Professor: only their own courses; Staff: all courses)
- Average exam grade, average assignment grade, and average final grade per course

### Student Section
- View enrolled courses
- View available exams with time-window restrictions
- Take exams (prevented from retaking)
- Submit assignments (file stored in database, max 10 MB)
- View submitted assignments with download links
- View personal grades

### Database
- SQL Server 2022 running in Docker
- Entity Framework Core with migrations
- Automatic seeding of permissions, fixed codes, and admin user
- Automatic backfill of default permissions for any user missing them

### Security
- Passwords hashed with BCrypt
- Session stored in HttpOnly cookies
- All delete operations use HTTP POST only
- File downloads restricted to the owner or the responsible professor
- Exam access restricted by enrollment, time window, and prior participation
- Friendly error pages for 403 and 404

## Project Structure

- **Controllers/** — MVC controllers grouped by area
- **Data/** — DbContext, initializer, and design-time factory
- **Filters/** — Custom authorization attributes and session extensions
- **Migrations/** — Entity Framework Core migrations
- **Models/** — Entities and ViewModels
- **Views/** — Razor views organized by controller
- **wwwroot/** — Static assets (CSS)

## Technology Stack

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core 10
- SQL Server 2022 (Docker)
- BCrypt.Net-Next
- Docker and Docker Compose

## Notes

- The application uses Development mode by default. For production, set ASPNETCORE_ENVIRONMENT to Production and update the connection string accordingly.
- The database is stored in a Docker volume named mssql-data. Removing the volume with "docker compose down -v" will delete all data.
- All dates are stored in Gregorian format. The user interface displays them in yyyy/MM/dd format.

## License

This project is provided as-is for educational purposes.
