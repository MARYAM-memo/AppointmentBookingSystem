# ⚙️ Configuration Guide

Comprehensive guide for configuring the Appointment Booking System for your environment.

---

## Configuration Files

### Primary Configuration File
- **Location:** `AppointmentBooking.Web/appsettings.json`
- **Environment-Specific:** `appsettings.Development.json`, `appsettings.Production.json`
- **Format:** JSON

### Secrets (Development)
- **Location:** `%APPDATA%\Microsoft\UserSecrets\` (Windows) or `~/.microsoft/usersecrets/` (macOS/Linux)
- **Scope:** Local development only
- **Security:** Never commit to version control

---

## Complete appsettings.json Template

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

---

## Configuration Sections Explained

### 1. Database Connection

```json
"ConnectionStrings": {
  "DefaultConnection": ""
}
```

**Note**: The connection string is intentionally left empty in `appsettings.json` for security. Use *User Secrets* for development or *Environment Variables* for production.

**Full Connection String Format (PostgreSQL):**
```Host=localhost;Port=5432;Database=appointment_booking;Username=<Owner Name>;Password=<Your Password>;SSL Mode=Prefer;Trust Server Certificate=true;Include Error Detail=true;Connection Timeout=15;Command Timeout=30```

**Parameters:**
- `Host` - PostgreSQL server hostname/IP
- `Port` - PostgreSQL port (default: 5432)
- `Database` - Database name
- `User Id` - Database user
- `Password` - Database password
- `Ssl Mode` - SSL encryption (Disable, Require, Prefer)
- `Trust Server Certificate` - Trust server certificate (for development)
- `Connection Timeout` - Connection timeout in seconds (default: 15)
- `Command Timeout` - Query timeout in seconds (default: 30)
- `Include Error Detail` -  Include detailed error messages (development only)


**Examples:**

**Local Development (User Secrets):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=appointment_booking;Username=<Owner Name>;Password=<Your Password>;SSL Mode=Prefer;Trust Server Certificate=true;Include Error Detail=true;Connection Timeout=15;Command Timeout=30"
  }
}
```

**Remote Server (Production):**
```
Host=prod-db.example.com;Port=5432;Database=appointment_booking;Username=produser;Password=STRONG_PASSWORD;SSL Mode=Require;Connection Timeout=15;Command Timeout=30
```

**Azure PostgreSQL:**
```
Host=myserver.postgres.database.azure.com;Port=5432;Database=appointment_booking;Username=username@myserver;Password=PASSWORD;SSL Mode=Require;
```

---

### 2. Identity Password Policy

```json
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
```

**Configuration Options:**
- `RequiredLength` - Minimum password length
- `RequiredMaxLength` - Maximum password length
- `RequireNonAlphanumeric` - Require special characters (!@#$%^&*)
- `RequireUppercase` - Require uppercase letters (A-Z)
- `RequireLowercase` - Require lowercase letters (a-z)
- `RequireDigit` - Require numbers (0-9)

**Presets:**

**Current Settings (Medium Security):**
- Minimum length: *6* characters
- Maximum length: *50* characters
- Requires: *uppercase*, *lowercase*, *digit*, *special character*

**High Security (Recommended for Production):**
```json
{
  "RequiredLength": 12,
  "RequiredMaxLength": 50,
  "RequireNonAlphanumeric": true,
  "RequireUppercase": true,
  "RequireLowercase": true,
  "RequireDigit": true
}
```

**Relaxed (Development Only):**
```json
{
  "RequiredLength": 6,
  "RequiredMaxLength": 50,
  "RequireNonAlphanumeric": false,
  "RequireUppercase": false,
  "RequireLowercase": false,
  "RequireDigit": false
}
```

---

### 3. Business Settings

```json
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
}
```

**Parameters:**

*Profile Settings*
- `BusinessName` - Initial business name displayed in the application
- `BusinessType` - Business category (e.g., "شامل", "صالون", "عيادة", "مستشار")
- `Currency` - Currency symbol for prices (e.g., "ج.م", "$", "€")
- `Language` - Default UI language code (e.g., "ar", "en")
- `Direction` - Text direction (rtl for Arabic, ltr for English)

*Working Hours*
- `WorkingHoursStart` - Business hours start in 24-hour format (HH:mm:ss)
- `WorkingHoursEnd` - Business hours end in 24-hour format (HH:mm:ss)
- `SlotDurationMinutes` - Default appointment slot duration in minutes (30 = half-hour slots)

*Branding Colors*
- `Primary` - Main brand color (buttons, headers, active states)
- `Secondary` - Secondary brand color (accents, hover states)
- `Accent` - Highlight color (warnings, badges, special indicators)
- `DarkModePrimary` - Optional: Primary color for dark mode

*Custom Labels*
- `service` - Label for services section (e.g., "الخدمات", "Services")
- `serviceItem` - Label for individual service (e.g., "الخدمة", "Service")
- `customer` - Label for customers section (e.g., "العملاء", "Customers")
- `appointment` - Label for appointments section (e.g., "الحجوزات", "Appointments")

**Example Customizations:**

*Salon:*
```json
{
  "BusinessName": "صالون الأناقة",
  "BusinessType": "صالون",
  "CustomLabels": {
    "service": "الخدمات",
    "serviceItem": "الخدمة",
    "customer": "العملاء",
    "appointment": "الحجوزات"
  }
}
```

*Clinic:*
```json
{
  "BusinessName": "عيادة الشفاء",
  "BusinessType": "عيادة",
  "CustomLabels": {
    "service": "الكشوفات",
    "serviceItem": "الكشف",
    "customer": "المرضى",
    "appointment": "المواعيد"
  }
}
```
---

### 4. Rate Limiting

```json
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
}
```

**Policy Descriptions:**
- **strict** (10/min) - Appointment creation/deletion, sensitive operations
- **default** (50/min) - General API endpoints
- **light** (100/min) - Read-only operations, list views
- **auth** (5/min) - Login attempts (brute force protection)

**Policy Descriptions:**
- **permitLimit** - Maximum requests allowed within the window
- **windowMinutes** - Time window for counting requests
- **queueLimit** - Requests queued when limit exceeded (0 = reject immediately)

**Adjust for Your Server:**
- Low traffic (< 100 users): Increase all limits by 2x
- High traffic (> 10K users): Consider distributed rate limiting
- Development: Increase all limits by 2-5x for easier testing

---

### 5. Localization

```json
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
}
```

**Parameters:**
- `SupportedCultures` - Mapping of language codes to culture codes
  - `ar` → `ar-EG` (Arabic - Egypt) with RTL support
  - `en` → `en-US` (English - United States) with LTR
- `DefaultCulture` - Default culture for new users (ar-EG)
- `DefaultLanguage` - Default language code (ar)
- `AvailableLanguages` - List of enabled languages for language switcher
- `SupportedCurrencies` - List of available currencies
- `DefaultCurrency` - Default currency for the application

**Supported Culture Codes:**
- `ar-EG` - Arabic (Egypt) - RTL
- `en-US` - English (United States) - RTL
- `ar-SA` - Arabic (Saudi Arabia) - LTR
- `en-GB` - English (United Kingdom) - LTR

**How Currency Works:**
- Currency is set at *application level* (not per business)
- All prices use the same currency throughout the app
- Users can change currency from settings
- Custom labels for services/customers remain in Arabic by default

**Adding a New Currency:**
```json
"SupportedCurrencies": [
  "ج.م",
  "EGP", 
  "USD",
  "SAR",
  "AED",
  "EUR",
  "GBP"  // Add British Pound
]
```

**Adding a New Language:**
```json
{
  "SupportedCultures": {
    "ar": "ar-EG",
    "en": "en-US",
    "fr": "fr-FR"
  },
  "DefaultCulture": "ar-EG",
  "DefaultLanguage": "ar",
  "AvailableLanguages": [
    "ar",
    "en",
    "fr"
  ]
}
```

**Custom Labels & Localization:**
- Default labels are translated (AR/EN)
- If user customizes labels (e.g., changes "الخدمات" to "المنتجات"):
  - Custom labels *do NOT* auto-translate when switching languages
  - User must manually update custom labels in Settings → Profile
- This preserves business-specific terminology

**Example:**
```
Default Labels:
  AR: "الخدمات" → EN: "Services"
  
Customized Labels (User changes):
  AR: "المنتجات" → EN: "Products" (must be set manually in settings)
```
---

### 6. Serilog Logging

```json
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
```

**Parameters:**

**Minimum Level**
- `Default` - Minimum log level for application logs (Information)
- `Override:Microsoft` - Suppress Microsoft logs to Warning
- `Override:Microsoft.AspNetCore` - Suppress ASP.NET Core logs to Warning

**Log Levels (from most to least verbose):**
- `Verbose` / `Trace` - Very detailed diagnostic information
- `Debug` - Debug information for development
- `Information` - General operational messages (RECOMMENDED for production)
- `Warning` - Warning messages for abnormal events
- `Error` - Error messages for failed operations
- `Fatal` - Critical errors requiring immediate attention

**File Sink**
- `Path` - Log file path with date placeholder (logs/appointment-.txt → logs/appointment-20240115.txt)
- `RollingInterval` - How often to create new file (Day, Hour, Month)
- `OutputTemplate` - Log message format
- `FileSizeLimitBytes` - Maximum file size before rolling (10MB = 10485760)
- `RetainedFileCountLimit` - Number of log files to keep (31 = ~1 month)

**Console Sink**
- `OutputTemplate` - Console output format (shorter than file)

**Enrichers**
- `FromLogContext` - Include contextual properties
- `WithMachineName` - Include server machine name
- `WithThreadId` - Include thread ID for async debugging

**Properties**
- `Application` - Application identifier in logs
- `Environment` - Environment name (Development, Production)

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

**Development-Specific Settings:**
- Serilog level: `Debug` (more verbose than production)
- EF Core logging: `Debug` (shows SQL queries)
- File logging only (no console output template specified, uses default)
- Connection string: Use User Secrets (see below)

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
      "strict": {
        "permitLimit": 10,
        "windowMinutes": 1,
        "queueLimit": 2
      },
      "default": {
        "permitLimit": 50,
        "windowMinutes": 1,
        "queueLimit": 5
      }
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

**Production-Specific Settings:**
- `AllowedHosts` - Restrict to your domain only (security)
- Password policy: Strong (12 chars, all requirements)
- Serilog: `Information` level only (no Debug/Trace)
- Connection string: Use Environment Variables or Key Vault
- Consider adding HTTPS/HSTS settings
---

## Managing Secrets

### Development (Local Secrets)

**Development (User Secrets)**
User Secrets store sensitive configuration outside your project directory (never committed to Git).

**Your Current Secrets:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=appointment_booking;Username=<Owner Name>;Password=<Your Password>;SSL Mode=Prefer;Trust Server Certificate=true;Include Error Detail=true;Connection Timeout=15;Command Timeout=30"
  },
  "AdminSettings": {
    "Email": "admin@system.com",
    "Password": "Admin@123",
    "Phone": "01234567890",
    "Role": "Admin"
  }
}
```

**CLI Commands:**
```bash
# Initialize secrets for project
dotnet user-secrets init --project AppointmentBooking.Web

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=appointment_booking;Username=<Owner Name>;Password=<Your Password>;SSL Mode=Prefer;Trust Server Certificate=true;Include Error Detail=true;Connection Timeout=15;Command Timeout=30" --project AppointmentBooking.Web

# Set admin credentials
dotnet user-secrets set "AdminSettings:Email" "admin@system.com" --project AppointmentBooking.Web
dotnet user-secrets set "AdminSettings:Password" "Admin@123" --project AppointmentBooking.Web
dotnet user-secrets set "AdminSettings:Phone" "01234567890" --project AppointmentBooking.Web
dotnet user-secrets set "AdminSettings:Role" "Admin" --project AppointmentBooking.Web

# List all secrets
dotnet user-secrets list --project AppointmentBooking.Web

# Remove a secret
dotnet user-secrets remove "AdminSettings:Password" --project AppointmentBooking.Web

# Clear all secrets (use with caution!)
dotnet user-secrets clear --project AppointmentBooking.Web
```

**Secrets Storage Location:**
- **Windows:** %APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json
- **Linux/macOS:** ~/.microsoft/usersecrets/<user_secrets_id>/secrets.json

**Note:** The <user_secrets_id> is stored in your .csproj file:

```xml
<UserSecretsId>your-guid-here</UserSecretsId>
```

### Production (Environment Variables)

```bash
# Linux/macOS
export ConnectionStrings__DefaultConnection="Host=prod-db.example.com;Port=5432;Database=appointment_booking;Username=<Owner Name>;Password=<Your Strong Password>;SSL Mode=Require;Connection Timeout=15;Command Timeout=30"
export AdminSettings__Email="admin@yourcompany.com"
export AdminSettings__Password="STRONG_ADMIN_PASSWORD"
export ASPNETCORE_ENVIRONMENT="Production"

# Windows (PowerShell)
$env:ConnectionStrings__DefaultConnection = "Host=prod-db.example.com;..."
$env:AdminSettings__Email = "admin@yourcompany.com"
$env:ASPNETCORE_ENVIRONMENT = "Production"

# Windows (CMD)
set ConnectionStrings__DefaultConnection=Host=prod-db.example.com;...
set ASPNETCORE_ENVIRONMENT=Production
```

**Docker:**
```bash
docker run -e ConnectionStrings__DefaultConnection="Host=prod-db;..." -e AdminSettings__Email="admin@yourcompany.com" -e ASPNETCORE_ENVIRONMENT="Production" appointmentbooking:latest
```

**docker-compose.yml:**
```yaml
services:
  web:
    image: appointmentbooking:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=appointment_booking;Username=produser;Password=${DB_PASSWORD};SSL Mode=Require
      - AdminSettings__Email=admin@yourcompany.com
      - AdminSettings__Password=${ADMIN_PASSWORD}
    secrets:
      - db_password
      - admin_password
```

### Production (Vault/Key Management)

For enterprise deployments, use dedicated secret management:
- **Azure Key Vault:**
```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri("https://<your-keyvault>.vault.azure.net/"),
    new DefaultAzureCredential()
);
```
- **AWS Secrets Manager**
```csharp
builder.Configuration.AddSecretsManager(
    region: RegionEndpoint.USEast1,
    secretName: "appointment-booking-secrets"
);
```
- **HashiCorp Vault**
```bash
# Using Vault Agent sidecar
vault write secret/appointment-booking connection_string="Host=prod-db;..." admin_password="STRONG_PASSWORD"
```
- **Kubernetes Secrets**


### Admin Settings

The `AdminSettings` section is stored in User Secrets (development) or Environment Variables (production) for security.

```json
{
  "AdminSettings": {
    "Email": "admin@system.com",
    "Password": "Admin@123",
    "Phone": "01234567890",
    "Role": "Admin"
  }
}
```
**Parameters:**
- `Email` - Default admin account email
- `Password` - Default admin account password (change after - `first login!)
- `Phone` - Admin contact phone
- `Role` - Identity role assigned to admin (`Admin`, `SuperAdmin`)

**Security Recommendations:**
1. Change default admin password immediately after first login
2. Use strong, unique passwords in production
3. Enable two-factor authentication if available
4. Store in User Secrets (dev) or Key Vault (production)
5. Never commit admin credentials to version control



## Performance Tuning

### For Small Deployments (< 1K users)

Current settings are optimized for small deployments. No changes needed.

### For Medium Deployments (1K - 10K users)

```json
{
  "RateLimiting": {
    "Policies": {
      "strict": { "permitLimit": 30, "windowMinutes": 1, "queueLimit": 5 },
      "default": { "permitLimit": 100, "windowMinutes": 1, "queueLimit": 10 },
      "light": { "permitLimit": 200, "windowMinutes": 1, "queueLimit": 20 }
    }
  },
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "Path": "logs/appointment-.txt",
          "RollingInterval": "Day",
          "FileSizeLimitBytes": 20971520,
          "RetainedFileCountLimit": 60
        }
      }
    ]
  }
}
```

### For Large Deployments (> 10K users)

```json
{
  "RateLimiting": {
    "Policies": {
      "strict": { "permitLimit": 50, "windowMinutes": 1, "queueLimit": 10 },
      "default": { "permitLimit": 200, "windowMinutes": 1, "queueLimit": 20 },
      "light": { "permitLimit": 500, "windowMinutes": 1, "queueLimit": 50 }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db.example.com;Port=5432;Database=appointment_booking;Username=produser;Password=STRONG_PASSWORD;SSL Mode=Require;Connection Timeout=15;Command Timeout=60;Max Pool Size=50;Min Pool Size=10"
  },
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "Path": "logs/appointment-.txt",
          "RollingInterval": "Hour",
          "FileSizeLimitBytes": 52428800,
          "RetainedFileCountLimit": 168
        }
      }
    ]
  }
}
```

**Additional Large-Scale Considerations:**
- Use Redis for distributed rate limiting
- Implement database read replicas
- Use CDN for static assets
- Configure application load balancing
- Set up log aggregation (ELK stack, Splunk, etc.)

---

## Validation Checklist

Before deploying:

- [ ] Database connection string tested and working
- [ ] User Secrets initialized (development) or Environment Variables set (production)
- [ ] Admin credentials configured and strong password set
- [ ] HTTPS enabled in production (AllowedHosts restricted)
- [ ] Password policy appropriate for environment
- [ ] Rate limits calibrated for expected traffic
- [ ] Serilog path writable and disk space sufficient
- [ ] Business settings (name, currency, hours) customized
- [ ] Localization settings match target audience
- [ ] Email service configured (if using notifications)
- [ ] Backup strategy in place for database
- [ ] Monitoring and alerting configured
- [ ] Secrets NOT committed to version control

---

## Troubleshooting Configuration Issues

### Configuration Not Applied

```bash
# 1. Check environment variable
echo $ASPNETCORE_ENVIRONMENT  # Linux/macOS
# Should show "Production" or "Development"

# 2. Verify appsettings.json is in correct location
ls -la AppointmentBooking.Web/appsettings.json

# 3. Rebuild application
dotnet clean
dotnet build

# 4. Run with specific environment
ASPNETCORE_ENVIRONMENT=Production dotnet run --project AppointmentBooking.Web

# 5. Check configuration sources (in code)
var config = builder.Configuration.GetDebugView();
Console.WriteLine(config);
```

### Database Connection Failed

```bash
# 1. Test PostgreSQL connection directly
# <Owner Name> -or- postgres
psql -h localhost -U postgres -d appointment_booking

# 2. Verify connection string format
# Correct: Host=localhost;Port=5432;Database=appointment_booking;Username=<Owner Name>;Password=<Your Password>;...
# Wrong: Server=localhost;User Id=<Owner Name>; (SQL Server format!)
z
# 3. Check PostgreSQL service status
sudo systemctl status postgresql  # Linux
brew services list | grep postgresql  # macOS

# 4. Check PostgreSQL logs
tail -f /var/log/postgresql/postgresql-*.log  # Linux
```

### Secrets Not Loading

```bash
# 1. Verify secrets initialized
dotnet user-secrets list --project AppointmentBooking.Web

# 2. Check secrets file exists
# Windows: dir %APPDATA%\Microsoft\UserSecrets\
# Linux/macOS: ls ~/.microsoft/usersecrets/

# 3. Verify UserSecretsId in .csproj
grep -i UserSecretsId AppointmentBooking.Web/AppointmentBooking.Web.csproj

# 4. Re-initialize if needed
dotnet user-secrets init --project AppointmentBooking.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..." --project AppointmentBooking.Web
```

### Serilog Not Writing to File

```bash
# 1. Check directory exists and is writable
mkdir -p logs
chmod 755 logs  # Linux/macOS

# 2. Verify path in configuration
# Correct: "Path": "logs/appointment-.txt"
# Wrong: "Path": "C:\\logs\\appointment-.txt" (use forward slashes or escape backslashes)

# 3. Check disk space
df -h  # Linux/macOS
```

### Admin Account Not Created

```bash
# 1. Verify AdminSettings in secrets
dotnet user-secrets list --project AppointmentBooking.Web | grep Admin

# 2. Check seeding logic runs on startup
# Look for: EnsureAdminUserAsync or similar in Program.cs

# 3. Verify database migrations applied
dotnet ef database update --project AppointmentBooking.Infrastructure --startup-project AppointmentBooking.Web
```

---

## Configuration Hierarchy (Priority Order)

ASP.NET Core loads configuration in this order (later sources override earlier):
1. `appsettings.json`
2. `appsettings.{Environment}.json` (e.g., `appsettings.Development.json`)
3. User Secrets (Development only)
4. Environment Variables
5. Command-line arguments
*Example:* If `ConnectionStrings:DefaultConnection` is defined in both appsettings.json and Environment Variables, the Environment Variable wins.

---

## Support Resources

- [Microsoft ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Serilog Documentation](https://serilog.net/)
- [Entity Framework Core Configuration](https://learn.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext)
- [ASP.NET Core Identity Configuration](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration)

---

**Configuration Complete! Ready for Deployment 🚀**
