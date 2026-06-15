# 📅 Appointment Booking System

Professional appointment booking and scheduling platform built with **ASP.NET Core** and **PostgreSQL**.

## 🎯 Overview

A production-ready appointment booking system designed for service-based businesses. Features include appointment management, customer database, service catalog, availability tracking, multi-language support (Arabic/English), dark mode, and comprehensive business profile management.

### Key Features:
- ✅ **Appointment Management** - Create, edit, delete, and track appointments
- ✅ **Customer Database** - Manage customer profiles and appointment history
- ✅ **Service Catalog** - Define services with duration, buffers, and pricing
- ✅ **Availability Management** - Automatic slot generation and conflict detection
- ✅ **Business Profile** - Customizable branding (colors, logo, custom labels)
- ✅ **Localization** - Arabic and English with RTL support
- ✅ **Theme Support** - Light/Dark mode toggle
- ✅ **Rate Limiting** - Protect against abuse with adaptive rate limits
- ✅ **Dashboard** - Real-time statistics and analytics
- ✅ **Authentication** - Secure user registration and login
- ✅ **Responsive Design** - Mobile-friendly Bootstrap 5 interface
- ✅ **Soft Delete** - Preserve historical data while maintaining clean views
- ✅ **Optimistic Concurrency** - Prevent double-booking and conflicts
- ✅ **Structured Logging** - Serilog integration for production monitoring

---

## 🏗️ Architecture

**Clean Architecture** with **6-layer** design:

```
AppointmentBooking.Web          ← ASP.NET MVC (Controllers, Views)
    ↓
AppointmentBooking.Application  ← Business Logic (DTOs, Validators, Services)
    ↓
AppointmentBooking.Infrastructure ← Data Access (EF Core, Repositories)
    ↓
AppointmentBooking.Core         ← Domain Models & Interfaces
    ↓
AppointmentBooking.Tests        ← Unit Tests
```

### Technologies:
- **Framework:** .NET 10.0
- **Database:** PostgreSQL (via Npgsql)
- **ORM:** Entity Framework Core 10.0
- **Validation:** FluentValidation 11.3
- **Mapping:** Mapster 10.0
- **Authentication:** ASP.NET Core Identity
- **Logging:** Serilog with file sink
- **UI Framework:** Bootstrap 5
- **Frontend:** Vanilla JavaScript with SweetAlert2

---

## 📊 Database Schema

### Core Tables:
- **Appointments** - Booking records with status, pricing, concurrency control
- **Customers** - Client information and appointment history tracking
- **Services** - Service definitions with duration and buffer configuration
- **Businesses** - Business profile and branding settings
- **AspNetCore Identity Tables** - User authentication and authorization

### Key Relationships:
```
Business (1) ──→ (Many) Services
         ├─→ (Many) Customers
         └─→ (Many) Appointments

Service  (1) ──→ (Many) Appointments
Customer (1) ──→ (Many) Appointments
```

---

## 🚀 Quick Start

### Prerequisites:
- .NET 10.0 SDK or higher
- PostgreSQL 12+ 
- Node.js (optional, for build tools)

### Installation:
1. **Clone Repository**
   ```bash
   git clone https://github.com/MARYAM-memo/AppointmentBookingSystem.git
   cd AppointmentBookingSystem
   ```

2. **Install Dependencies**
   ```bash
   dotnet restore
   ```
3. **Setup User Secrets** (⚠️ Important!)
   ```bash
   cd AppointmentBooking.Web
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=appointment_booking;Username=your_user;Password=your_password;..."
   dotnet user-secrets set "AdminSettings:Email" "admin@system.com"
   dotnet user-secrets set "AdminSettings:Password" "Admin@123"
   ```
3. **Setup Database**
   ```bash
   # Update connection string in appsettings.json
   # Then run migrations
   dotnet ef database update -p ../AppointmentBooking.Infrastructure -s .
   ```

4. **Run Application**
   ```bash
   cd AppointmentBooking.Web
   dotnet run
   ```

5. **Access Application**
   ```
   https://localhost:7000
   ```

### Default Credentials:
- **Email:** admin@system.com
- **Password:** Admin@123

---

## 📝 Features Deep Dive

### 1. Appointment Management
- Create new appointments with automatic conflict detection
- Update appointment times with slot validation
- Track appointment status (Pending, Confirmed, In Progress, Completed, Cancelled, No Show, Rescheduled)
- Soft delete appointments while maintaining historical records

### 2. Availability System
- Automatic time slot generation based on service duration and buffer
- Configurable working hours and slot duration
- Real-time availability checking with N-way conflict detection
- Calendar view of appointments

### 3. Customer Management
- Maintain customer profiles with contact information
- Track appointment history per customer
- Customer booking statistics

### 4. Service Management
- Define services with customizable durations and buffers
- Set service pricing
- Service-specific availability rules

### 5. Business Profile
- Customize company branding (primary, secondary, accent colors)
- Set custom labels for UI elements
- Configure working hours
- Support for multiple locales
- Business contact information

### 6. Security Features
- Rate limiting (adaptive policies: strict, default, light, auth)
- HTML sanitization for user input
- CSRF protection
- Soft delete with audit trail
- Optimistic concurrency control
- Role-based authorization

### 7. Localization
- Full Arabic (ar-EG) support with RTL layout
- English (en-US) support
- Language switcher with warning about custom labels
- Localized error messages via FluentValidation

---

## 🎨 Customization

### Changing Colors:
Visit **Settings → Profile → Branding Colors** to customize:
- Primary Color
- Secondary Color
- Accent Color

These apply site-wide in real-time.

### Custom Labels:
Edit custom labels from **Settings → Profile → Custom Labels** to rebrand:
- "Appointments" → "Bookings"
- "Customers" → "Clients"
- "Services" → "Products"
- etc.

### Working Hours:
Configure in **Settings → Profile** to set:
- Business hours (start/end times)
- Slot duration (15, 30, 45, 60 minutes)

### Localization:
Switch between Arabic and English from navbar language button.

---

## 🔐 Security

### Implemented:
- ✅ HTTPS/HSTS enforcement
- ✅ Content Security Policy (CSP) headers
- ✅ XSS protection via HTML sanitization
- ✅ SQL injection prevention (parameterized queries)
- ✅ CSRF token validation
- ✅ Rate limiting on sensitive endpoints
- ✅ Secure password hashing (PBKDF2)
- ✅ Session timeout (30 minutes)
- ✅ HttpOnly cookies
- ✅ SameSite cookie attributes

### Best Practices:
- Secrets stored in User Secrets (dev) / Vault (production)
- Audit logging of sensitive operations
- Regular dependency updates
- Security headers validation

---

## 📊 Performance

### Optimizations:
- ✅ Request-level caching for business profile
- ✅ Memory cache for form dropdown data
- ✅ Async/await throughout data access
- ✅ Database indexes on frequently queried columns
- ✅ Connection pooling configured
- ✅ Static file compression
- ✅ Intersection Observer for lazy animations

### Scalability:
- Supports 10,000+ concurrent users (with distributed cache layer)
- Horizontal scalability via stateless architecture
- Optional Redis for distributed caching
- Optional background job queue (Hangfire) for notifications

---

## 🧪 Testing

### Test Coverage:
- ✅ Pagination logic tests
- ✅ Mock data helpers

### Running Tests:
```bash
cd AppointmentBooking.Tests
dotnet test
```

---

## 🌍 Localization

### Supported Languages:
- **Arabic (ar-EG)** - RTL layout, Arabic number formatting
- **English (en-US)** - LTR layout, standard formatting

### To Add New Language:
1. Add culture to `LocalizationSettings:SupportedCultures` && `LocalizationSettings:AvailableLanguages` in appsettings.json
2. Create resource files in `Resources/` folder
3. Update language selector in navbar

---

## 📦 Deployment

### Pre-deployment Checklist:
- [ ] Update `appsettings.Production.json`
- [ ] Set strong database password
- [ ] Configure SSL certificate
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Enable HSTS in production
- [ ] Configure logging for external service (Azure, AWS)
- [ ] Set up automated backups for database
- [ ] Configure rate limiting limits for your server capacity
- [ ] Test appointments under expected load

### Docker Support:
Dockerfile available in repository root:
```bash
docker build -t appointmentbooking .
docker run -p 80:8080 appointmentbooking
```

### Environment Variables:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=pg-server;Database=appointments;...
```

---

## 🐛 Troubleshooting

### Common Issues:

**"Database connection failed"**
- Verify PostgreSQL is running
- Check connection string in appsettings.json
- Ensure database exists and user has permissions

**"Appointments not saving"**
- Check validation errors in network console
- Verify all required fields filled
- Check for double-booking conflicts

**"Rate limit exceeded"**
- Wait for rate limit window to expire
- Configure rate limits in appsettings.json
- Consider distributed rate limiting for production

---

## 📄 License

See [Commercial license](LICENSE) file. Redistribution and resale prohibited.

---

## 🤝 Support

For issues, feature requests, or customization inquiries, contact support.

---

## 📈 Version History

- **v1.0.0** (2026-06-07) - Initial release
  - Core appointment booking functionality
  - Multi-language support (AR/EN)
  - Business profile customization
  - Dashboard with statistics

---

## 📖 Additional Resources

- [Installation Guide](INSTALLATION.md)
- [Configuration Guide](CONFIGURATION.md)
- [Change Log](CHANGELOG.md)
- [Database Schema](database/Appointments_Schema.sql)
- [Database Diagram](database/Appointments_ER_Diagram.png)
- [Screenshots](screenshots/)
---

**Made with ❤️ for Service-Based Businesses**

Happy Booking! 📅✨
