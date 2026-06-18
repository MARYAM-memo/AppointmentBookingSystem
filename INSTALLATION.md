# 📋 Installation Guide

Complete step-by-step guide to install and configure the Appointment Booking System.

---

## Prerequisites

### System Requirements:
- **OS:** Windows 10+, macOS 11+, or Linux (Ubuntu 20.04+)
- **RAM:** 4GB minimum (8GB recommended for smooth development)
- **Disk Space:** 2GB for installation

### Software Required:
1. **.NET 10.0 SDK**
   - Download: https://dotnet.microsoft.com/download/dotnet/10.0
   - Verify: `dotnet --version` (should show 10.0.x)
   - Visual Studio 2026 or VS Code with C# Dev Kit recommended

2. **PostgreSQL 12+**
   - Download: https://www.postgresql.org/download/
   - Verify: `psql --version`

3. **Git** (for version control)
   - Download: https://git-scm.com/
   - Verify: `git --version`

4. **Text Editor/IDE** (Optional but recommended)
   - Visual Studio 2022 Community (free)
   - VS Code with C# extension
   - JetBrains Rider

---

## Installation Steps

### Step 1: Clone Repository

```bash
# Clone the repository
git clone <repository-url>
cd AppointmentBookingSystem

# Verify project structure
ls -la
# Should see: *.sln file and AppointmentBooking.* folders
```

### Step 2: Restore NuGet Packages

```bash
# Restore all NuGet dependencies
dotnet restore

# Expected output:
# Determining projects to restore...
# Restored /path/to/AppointmentBooking.Web/AppointmentBooking.Web.csproj (in X ms)
```

### Step 3: Setup PostgreSQL Database

#### Option A: Using pgAdmin UI (Recommended for beginners)

1. **Start PostgreSQL Service**
   - Windows: Should start automatically after installation
   - macOS: `brew services start postgresql`
   - Linux: `sudo systemctl start postgresql`

2. **Open pgAdmin**
   - Default URL: `http://localhost:5050`
   - Login with credentials set during installation

3. **Create Database**
   - Right-click "Databases" → "Create" → "Database"
   - Database name: `appointment_booking`
   - Owner:your PostgreSQL user (e.g., `postgres`)
   - Click "Save"

4. **Verify Connection**
   - Expand "appointment_booking" database
   - Confirm empty database structure

#### Option B: Using Command Line

```bash
# Connect to PostgreSQL as your user
psql -U postgres -h localhost

# Inside psql prompt (password will be requested)
postgres=# CREATE DATABASE appointment_booking;
postgres=# \l  # List databases - should show appointment_booking

# Exit psql
postgres=# \q
```

### Step 4: Configure User Secrets (Development)

⚠️ *Important:* Never store sensitive data (passwords, connection strings) in appsettings.json. Use User Secrets for development.
⚠️ *Important:* Admin has full system access. Change password immediately after first login.

```bash
# Navigate to Web project
cd AppointmentBooking.Web

# Initialize User Secrets (run once)
dotnet user-secrets init

# Set PostgreSQL connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection"   "Host=localhost;Port=5432;Database=appointment_booking;Username=<Owner Name>;Password=<Your Password>;SSL Mode=Prefer;Trust Server Certificate=true;Include Error Detail=true;Connection Timeout=15;Command Timeout=30"

# Set admin credentials
dotnet user-secrets set "AdminSettings:Email" "admin@system.com"
dotnet user-secrets set "AdminSettings:Password" "Admin@123"
dotnet user-secrets set "AdminSettings:Phone" "01234567890"
dotnet user-secrets set "AdminSettings:Role" "Admin"

# Verify secrets are set
dotnet user-secrets list
```

**User Account Creation:**
- New users are created with *User* role by default
- Only admins can promote users to Admin role
- User accounts have view-only access

**Connection String Parameters:**
- `Host` - PostgreSQL server hostname (e.g., `localhost`)
- `Port` - PostgreSQL port (default: `5432`)
- `Database` - Database name (`appointment_booking`)
- `Username` - PostgreSQL user (e.g., `postgres`)
- `Password` - Your PostgreSQL password
- `SSL Mode` - SSL encryption (`Prefer`, `Require`, `Disable`)
- `Trust Server Certificate` - Trust server certificate (development only)
- `Include Error Detail` - Include detailed error messages (development only)
- `Connection Timeout` - Connection timeout in seconds (`15`)
- `Command Timeout` - Query timeout in seconds (`30`)

**Example Connection Strings:**
**Local Development:**
```
Host=localhost;Port=5432;Database=appointment_booking;Username=<Owner Name>;Password=<Your Password>;SSL Mode=Prefer;Trust Server Certificate=true;Include Error Detail=true;Connection Timeout=15;Command Timeout=30
```

**Production (Environment Variable):**
```
Host=prod-db.example.com;Port=5432;Database=appointment_booking;Username=<Owner Name>;Password=<Your Strong Password>;SSL Mode=Require;Connection Timeout=15;Command Timeout=30
```

### Step 5: Verify appsettings.json

Your `appsettings.json` should look like this (no sensitive data):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "System": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Identity": {
    "Password": {
      "RequireDigit": true,
      "RequiredLength": 6,
      "RequiredMaxLength": 50,
      "RequireNonAlphanumeric": true,
      "RequireUppercase": true,
      "RequireLowercase": true
    }
  },
  "BusinessSettings": {
    "DefaultProfile": {
      "BusinessName": "نظام حجز المواعيد",
      "BusinessType": "شامل",
      "Currency": "ج.م",
      "Language": "ar",
      "Direction": "rtl",
      "WorkingHoursStart": "09:00:00",
      "WorkingHoursEnd": "22:00:00",
      "SlotDurationMinutes": 30,
      "Colors": {
        "Primary": "#2c6e7c",
        "Secondary": "#3a9e8f",
        "Accent": "#f39c12"
      },
      "CustomLabels": {
        "service": "الخدمات",
        "serviceItem": "الخدمة",
        "customer": "العملاء",
        "appointment": "الحجوزات"
      }
    }
  },
  "RateLimiting": {
    "Policies": {
      "strict": {
        "permitLimit": 10,
        "windowMinutes": 1,
        "queueLimit": 2
      },
      "default": {
        "permitLimit": 50,
        "windowMinutes": 1,
        "queueLimit": 5
      },
      "light": {
        "permitLimit": 100,
        "windowMinutes": 1,
        "queueLimit": 10
      },
      "auth": {
        "permitLimit": 5,
        "windowMinutes": 1,
        "queueLimit": 1
      }
    }
  },
  "LocalizationSettings": {
    "SupportedCultures": {
      "ar": "ar-EG",
      "en": "en-US"
    },
    "DefaultCulture": "ar-EG",
    "DefaultLanguage": "ar",
    "AvailableLanguages": [
      "ar",
      "en"
    ],
    "SupportedCurrencies": [
      "ج.م",
      "EGP",
      "USD",
      "SAR",
      "AED",
      "EUR"
    ],
    "DefaultCurrency": "ج.م"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "Path": "logs/appointment-.txt",
          "RollingInterval": "Day",
          "OutputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
          "FileSizeLimitBytes": 10485760,
          "RetainedFileCountLimit": 31
        }
      },
      {
        "Name": "Console",
        "Args": {
          "OutputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithThreadId"
    ],
    "Properties": {
      "Application": "AppointmentBookingSystem",
      "Environment": "Development"
    }
  }
}
```

### Step 6: Apply Database Migrations

```bash
# Ensure you're in the Web project directory
cd AppointmentBooking.Web

# Install EF Core CLI tool (if not already installed)
dotnet tool install --global dotnet-ef

# Create initial database schema
dotnet ef database update

# Expected output:
# Build started...
# Build succeeded.
# Applying migration '20260607154235_InitialMig'...
# Done.
```

**What This Does:**
- Creates all database tables
- Sets up relationships and constraints
- Seeds initial data (admin user, default business profile)
- Creates indexes for performance

**If Migration Fails:**
```bash
# Check available migrations
dotnet ef migrations list -p ../AppointmentBooking.Infrastructure -s .

# Test database connection
psql -h localhost -U postgres -d appointment_booking -c "SELECT 1;"

# If needed, reset database (⚠️ WARNING: This deletes all data!)
dotnet ef database drop --force -p ../AppointmentBooking.Infrastructure -s .
dotnet ef database update -p ../AppointmentBooking.Infrastructure -s .
```

### Step 7: Build Application

```bash
# From project root
cd ..
dotnet build

# Expected output:
# Build succeeded. X warning(s), 0 errors
```

**Troubleshooting Build Issues:**
```bash
# Clean build artifacts
dotnet clean

# Restore packages again
dotnet restore

# Rebuild
dotnet build
```

### Step 8: Run Application

```bash
# From Web project directory
cd AppointmentBooking.Web

# Run development server
dotnet run

# Expected output:
# Now listening on: https://localhost:7000
# Now listening on: http://localhost:5000
# Application started. Press Ctrl+C to stop.
```

### Step 9: Access Application

1. **Open Browser**
   ```
   https://localhost:7000
   ```

2. **Login**
   - Email: `admin@system.com` (or whatever you set in User Secrets)
   - Password: `Admin@123` (or whatever you set in User Secrets)

3. **Change Default Password**
   - After first login, navigate to *Settings → Profile*
   - Change to a secure password immediately

---

## Post-Installation Configuration

### 1. Update Business Profile

1. Navigate to **Settings → Profile**
2. Update business information:
   - *Business Name* - Your business name
   - *Business Type* - Salon, Clinic, Consultant, etc.
   - *Currency* - `ج.م`, `$`, `€`, etc.
   - *Logo URL*
   - *Custom Labels* - Labels for services, customers, appointments
   - *Branding Colors* - Primary, Secondary, Accent colors
   - *Working Hours* - Start and end times (e.g., 09:00:00 to 22:00:00)
   - *Slot Duration* - Appointment slot length in minutes (e.g., `30`)
3. Click Save

### 2. Create Services

1. Navigate to **Services**
2. Click **Add New Service**
3. Configure:
   - *Name* - Service name (e.g., "قص الشعر")
   - *Duration* - Service duration (e.g., `00:30:00` for 30 minutes)
   - *Buffer Befor*e - Preparation time (e.g., `00:05:00`)
   - *Buffer After* - Cleanup time (e.g., `00:05:00`)
   - *Price* - Service price (e.g., `150.00`)
   - *Max Capacity* - Concurrent bookings allowed (default: `1`)
   - *Icon* - Bootstrap icon class (e.g., `bi-scissors`)
   - *Color* - Service color (e.g., `#2c6e7c`)

4. Click Save

### 3. Add Customers

1. Navigate to **Customers**
2. Click **Add New Customer**
3. Enter:
   - *Full Name*
   - *Email*
   - *Phone Number*
4. Click Save

### 4. Create Sample Appointments

1. Navigate to **Appointments**
2. Click **New Appointment**
3. Select:
   - *Customer*
   - *Service*
   - *Date* - Pick a date (weekends are blocked by default)
   - *Time Slot* - Select from available slots
   - *Status* - `Pending`, `Confirmed`, etc.
4. Click Save

---

## Development Setup

### Entity Framework CLI Commands

```bash
# Install Entity Framework CLI globally
dotnet tool install --global dotnet-ef

# Create new migration (after model changes)
dotnet ef migrations add MigrationName -p AppointmentBooking.Infrastructure -s AppointmentBooking.Web

# Generate migration script (for version control)
dotnet ef migrations script -o migrate.sql -p AppointmentBooking.Infrastructure -s AppointmentBooking.Web

# Remove last migration (if not applied)
dotnet ef migrations remove   -p AppointmentBooking.Infrastructure   -s AppointmentBooking.Web
```

### Running Tests

```bash
# Navigate to test project
cd AppointmentBooking.Tests

# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter FullyQualifiedName~PaginationTests
```

### Debug Mode

```bash
# Run with debug symbols
dotnet run -c Debug

# Attach debugger from IDE
# F5 in Visual Studio
# Shift+F5 in VS Code
```

---

## Environment-Specific Configuration

### Development (appsettings.Development.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "Microsoft.EntityFrameworkCore": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "Path": "logs/appointment-.txt",
          "RollingInterval": "Day"
        }
      }
    ]
  }
}
```

*Development-Specific Settings:*
- Serilog level: `Debug` (shows SQL queries from EF Core)
- File logging only (uses default output template)
- Connection string: From User Secrets
- Admin credentials: From User Secrets

### Production (appsettings.Production.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "yourdomain.com",
  "Identity": {
    "Password": {
      "RequiredLength": 12,
      "RequireNonAlphanumeric": true,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireDigit": true
    }
  },
  "RateLimiting": {
    "Policies": {
      "strict": { "permitLimit": 10, "windowMinutes": 1, "queueLimit": 2 },
      "default": { "permitLimit": 50, "windowMinutes": 1, "queueLimit": 5 }
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "Path": "logs/appointment-.txt",
          "RollingInterval": "Day",
          "OutputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
          "FileSizeLimitBytes": 10485760,
          "RetainedFileCountLimit": 31
        }
      }
    ],
    "Properties": {
      "Application": "AppointmentBookingSystem",
      "Environment": "Production"
    }
  }
}
```

*Production-Specific Settings:*
`AllowedHosts` - Restrict to your domain only (security)
Password policy: Strong (12 characters, all requirements)
Serilog: `Information` level only (no Debug/Trace)
Connection string: From Environment Variables or Key Vault
Admin credentials: From Environment Variables or Key Vault

---

## Docker Installation (Alternative)

### Using Docker Compose

1. **Create Dockerfile `docker-compose.yml`** (if not included):
```yaml
version: '3.8'

services:
  db:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: appointment_booking
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER} -d appointment_booking"]
      interval: 5s
      timeout: 5s
      retries: 5

  web:
    build:
      context: .
      dockerfile: AppointmentBooking.Web/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=appointment_booking;Username=${DB_USER};Password=${DB_PASSWORD};SSL Mode=Prefer
      - AdminSettings__Email=admin@yourcompany.com
      - AdminSettings__Password=${ADMIN_PASSWORD}
      - AdminSettings__Role=Admin
    ports:
      - "8080:80"
    depends_on:
      db:
        condition: service_healthy
    volumes:
      - ./logs:/app/logs

volumes:
  postgres_data:
```

2. **Create Dockerfile :**:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "AppointmentBooking.Web.dll"]
```

3. **Run Container**:
```bash
# Create .env file
echo "DB_PASSWORD=your_db_password" > .env
echo "ADMIN_PASSWORD=your_admin_password" >> .env

# Start services
docker-compose up -d

# Apply migrations
docker-compose exec web dotnet ef database update   -p /src/AppointmentBooking.Infrastructure   -s /src/AppointmentBooking.Web
```

---

## Troubleshooting

### Common Issues & Solutions

#### ❌ "Unable to connect to database"
```
Error: Exception while connecting to database:
Connection refused at localhost:5432

Solution:
1. Verify PostgreSQL is running:
   - Windows: Check Services app → PostgreSQL service
   - macOS: brew services list | grep postgresql
   - Linux: sudo systemctl status postgresql
2. Check connection string format in appsettings.json (must use PostgreSQL format):
   ✅ Correct: Host=localhost;Port=5432;Database=appointment_booking;Username=postgres;Password=...
   ❌ Wrong:  Server=localhost;User Id=postgres; (SQL Server format!)
3. Test connection directly:
   psql -h localhost -U postgres -d appointment_booking
4. Verify database exists: psql -U postgres -l | grep appointment_booking
5. Check User Secrets are set:
   dotnet user-secrets list --project AppointmentBooking.Web
```

#### ❌ "Authentication failed for user 'postgres'"
```
Error: FATAL: password authentication failed for user "postgres"

Solution:
1. Verify PostgreSQL password in User Secrets:
   dotnet user-secrets list --project AppointmentBooking.Web
2. Reset password:
   psql -U postgres
   ALTER USER postgres WITH PASSWORD 'newpassword';
3. Update User Secrets:
   dotnet user-secrets set "ConnectionStrings:DefaultConnection"      "Host=localhost;Port=5432;Database=appointment_booking;Username=postgres;Password=newpassword;..."
```

#### ❌ "Module 'System.Net.Http' not found"
```
Error: The type or namespace name 'Http' does not exist

Solution:
1. dotnet clean
2. dotnet restore
3. Check .NET SDK version: dotnet --version (should be 10.0+)
```

#### ❌ "Port 5000/7000 already in use"
```
Error: Address already in use

Solution:
# Windows
netstat -ano | findstr :7000
taskkill /PID <PID> /F

# macOS/Linux
lsof -i :7000
kill -9 <PID>

# Or specify different port:
dotnet run --urls "https://localhost:7001;http://localhost:5001"
```

#### ❌ "Null reference exception on login"
```
Error: Object reference not set to an instance of an object

Solution:
1. Ensure migrations were applied:
   dotnet ef database update -p AppointmentBooking.Infrastructure -s AppointmentBooking.Web
2. Verify admin user was seeded (check AspNetUsers table)
3. Clear browser cookies/cache
4. Restart application: Ctrl+C then dotnet run
```

#### ❌ "Serilog not writing to file"
```
Error: No log files in /logs folder

Solution:
1. Create logs directory:
   mkdir -p AppointmentBooking.Web/logs

2. Verify directory is writable:
   chmod 755 AppointmentBooking.Web/logs  # Linux/macOS

3. Check Serilog path in appsettings.json:
   ✅ Correct: "Path": "logs/appointment-.txt"
   ❌ Wrong:  "Path": "C:\\logs\\appointment-.txt"

4. Check disk space:
   df -h  # Linux/macOS
```

#### ❌ "Admin user not created"
```
Error: Invalid login attempt

Solution:
1. Verify AdminSettings in User Secrets:
   dotnet user-secrets list --project AppointmentBooking.Web | grep Admin

2. Check seeding logic runs on startup (EnsureAdminUserAsync in Program.cs)

3. Verify database migrations applied:
   dotnet ef database update -p AppointmentBooking.Infrastructure -s AppointmentBooking.Web

4. Check AspNetUsers table in database:
   psql -U postgres -d appointment_booking -c "SELECT Email FROM AspNetUsers;"
```

#### ❌ "Weekend dates blocked"
```
Error: Cannot select Friday or Saturday

Solution:
This is by design! The system blocks weekend bookings.
To change working days, update Business Profile settings.
```

---

## Verification Checklist

After installation, verify everything works:

- [ ] Application runs without errors (`dotnet run`)
- [ ] Can access `https://localhost:7000`
- [ ] Can login with admin credentials
- [ ] Dashboard loads with statistics cards
- [ ] Can create new service
- [ ] Can add customer
- [ ] Can create appointment with time slot selection
- [ ] Language switcher works (`AR`/`EN`)
- [ ] Theme toggle works (`Light`/`Dark`)
- [ ] Database persists data between restarts
- [ ] Logs are generated in `/logs` folder
- [ ] Slot availability updates correctly (no double-booking)
- [ ] Edit appointment preserves current slot
- [ ] Weekend dates are blocked
- [ ] Past time slots are disabled for today

---

## Next Steps

1. **Read Configuration Guide** - Configure application settings
2. **Review Security** - Update default credentials, enable HTTPS
3. **Customize Branding** - Add your business colors, logo, labels
4. **Create Content** - Add your services and customers
5. **Test Workflows** - Create, edit, cancel appointments
5. **Deploy** - Deploy to production environment

---

## Support

For installation issues:
1. *Check logs:* 
```
tail -f AppointmentBooking.Web/logs/appointment-*.txt
```
2. *Verify .NET version:* 
```
dotnet --version  # Should be 10.0.x
```
3. *Check PostgreSQL:* `psql --version`
```
psql --version
pg_isready -h localhost
```
4. *Review error messages carefully* - Most issues are connection string or permission related
5. *Check Configuration Guide* for detailed settings explanation

---

**Installation Complete! Happy Booking! 📅**
