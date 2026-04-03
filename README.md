# Kachara Management System

## Overview
This project is a .NET 10 backend for a smart waste management system. It integrates with IoT devices (Arduino/ESP8266/ESP32) to monitor bin status and provides APIs for a Unity-based dashboard.

## Features
- RESTful API for bin status updates and admin operations
- Database persistence for sensor data and admin actions
- Dockerized for easy deployment (e.g., Railway)
- Swagger UI enabled in all environments
- Supports integration with Arduino-based IoT devices
- Provides data endpoints for Unity dashboard

## Project Structure
- `KacharaManagement.API/` - ASP.NET Core Web API
- `KacharaManagement.Business/` - Business logic layer
- `KacharaManagement.Core/` - Core entities and models
- `KacharaManagement.Repository/` - Data access layer (EF Core)

## Running Locally
1. **.NET 10 SDK** required
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Run the API:
   ```bash
   dotnet run --project KacharaManagement.API
   ```
   The API will run on port 5000 by default.
4. Access Swagger UI at: [http://localhost:5000/swagger](http://localhost:5000/swagger)

### Hot Reload / Watch Mode

For local development with hot reload, run:
```bash
dotnet watch --project KacharaManagement.API run
```

In VS Code, use the task named `watch: api` to start the same hot reload loop.

## Docker Deployment
- The app is Dockerized and exposes port 5000.
- Example build & run:
  ```bash
  docker build -t kachara-management .
  docker run -p 5000:5000 kachara-management
  ```

## API Endpoints
- `/api/update` - Receives bin status from Arduino
- `/api/status` - Returns the latest bin status (reads from the database)
- `/api/history` - Returns historical sensor data (reads from the database)
- `/api/admin/...` - Admin operations
- Swagger UI: `/swagger`

## Configuration
- Database and other settings are in `appsettings.json`.
- For production, override with environment variables as needed.

## Integrations
- **Arduino/IoT**: Sends bin status to `/api/update`
- **Unity**: Consumes API endpoints for dashboard visualization

## Data Consistency
- `/api/status` always returns the most recent data from the database, ensuring no stale or reset-prone in-memory state. This is robust to Railway restarts and always reflects the latest sensor update.

## License
MIT License

## Git Mirroring & Collaboration

This repository is mirrored to:

https://github.com/EpitechMscInternationalPromo2027/I-IOT-801-INT-8-1-smarttrashcans-2

All changes are pushed to both the main and Epitech repositories. To manually mirror:

```bash
git remote add mirror https://github.com/EpitechMscInternationalPromo2027/I-IOT-801-INT-8-1-smarttrashcans-2
git push --mirror mirror
```

## Troubleshooting

- If you see a 500 Internal Server Error from any API endpoint:
   - Check Railway logs for exception details.
   - Ensure your Railway environment variables (especially DB connection) are set correctly.
   - Confirm the database has data (e.g., for /api/history).
   - For local debugging, run with `dotnet run --project KacharaManagement.API` and check console output.

## Maintainers
- Tejaswini3107
- EpitechMscInternationalPromo2027
