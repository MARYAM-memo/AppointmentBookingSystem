--
-- PostgreSQL database dump for Appointment Booking System
-- Version: 1.0.0
-- Generated: 2026-06-08
--

-- Create Database
CREATE DATABASE appointment_booking WITH 
    TEMPLATE = template0 
    ENCODING = 'UTF8' 
    LOCALE_PROVIDER = icu 
    LOCALE = 'en_US.UTF-8' 
    ICU_LOCALE = 'en-US';

-- Connect to database
\connect appointment_booking

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Set configuration
SET statement_timeout = 0;
SET lock_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;

-- ============================================
-- TABLES
-- ============================================

-- Users table (ASP.NET Core Identity)
CREATE TABLE public."Users" (
    "Id" text NOT NULL,
    "FullName" character varying(200) NOT NULL,
    "DateOfBirth" date,
    "Gender" integer,
    "PreferDarkMode" boolean DEFAULT false NOT NULL,
    "PreferredLanguage" character varying(10) DEFAULT 'ar'::character varying NOT NULL,
    "EnableNotifications" boolean DEFAULT true NOT NULL,
    "CreatedAt" date NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "LastLoginAt" timestamp with time zone,
    "UserName" character varying(256),
    "NormalizedUserName" character varying(256),
    "Email" character varying(256),
    "NormalizedEmail" character varying(256),
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

-- Roles table
CREATE TABLE public."Roles" (
    "Id" text NOT NULL,
    "Name" character varying(256),
    "NormalizedName" character varying(256),
    "ConcurrencyStamp" text,
    CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
);

-- UserRoles junction table
CREATE TABLE public."UserRoles" (
    "UserId" text NOT NULL,
    "RoleId" text NOT NULL,
    CONSTRAINT "PK_UserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_UserRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."Roles"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserRoles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE
);

-- RoleClaims table
CREATE TABLE public."RoleClaims" (
    "Id" integer NOT NULL,
    "RoleId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text,
    CONSTRAINT "PK_RoleClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RoleClaims_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."Roles"("Id") ON DELETE CASCADE
);

-- UserClaims table
CREATE TABLE public."UserClaims" (
    "Id" integer NOT NULL,
    "UserId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text,
    CONSTRAINT "PK_UserClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserClaims_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE
);

-- UserLogins table
CREATE TABLE public."UserLogins" (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text,
    "UserId" text NOT NULL,
    CONSTRAINT "PK_UserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_UserLogins_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE
);

-- UserTokens table
CREATE TABLE public."UserTokens" (
    "UserId" text NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text,
    CONSTRAINT "PK_UserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_UserTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE
);

-- Business Profiles table
CREATE TABLE public."BusinessProfiles" (
    "Id" integer NOT NULL,
    "BusinessName" character varying(200) NOT NULL,
    "BusinessType" character varying(50) NOT NULL,
    "Tagline" character varying(500),
    "LogoUrl" character varying(500),
    "FaviconUrl" character varying(500),
    "Colors_Primary" character varying(20) NOT NULL,
    "Colors_Secondary" character varying(20) NOT NULL,
    "Colors_Accent" character varying(20) NOT NULL,
    "Colors_DarkModePrimary" character varying(20),
    "Localization_Currency" character varying(10) NOT NULL,
    "Localization_Language" character varying(10) NOT NULL,
    "Localization_Direction" character varying(10) NOT NULL,
    "WorkingHoursStart" interval NOT NULL,
    "WorkingHoursEnd" interval NOT NULL,
    "SlotDurationMinutes" integer NOT NULL,
    "CustomLabels" jsonb NOT NULL,
    "Contact_Phone" character varying(20),
    "Contact_Email" character varying(200),
    "Contact_Address" character varying(500),
    "CreatedAt" date NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_BusinessProfiles" PRIMARY KEY ("Id")
);

-- Customers table
CREATE TABLE public."Customers" (
    "Id" integer NOT NULL,
    "FullName" character varying(200) NOT NULL,
    "PhoneNumber" character varying(20) NOT NULL,
    "Email" character varying(200),
    "Notes" character varying(1000),
    "TotalAppointments" integer DEFAULT 0 NOT NULL,
    "CreatedAt" date NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "LastAppointmentDate" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "ApplicationUserId" text,
    CONSTRAINT "PK_Customers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Customers_Users_ApplicationUserId" FOREIGN KEY ("ApplicationUserId") REFERENCES public."Users"("Id")
);

-- Services table
CREATE TABLE public."Services" (
    "Id" integer NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Description" character varying(2000),
    "Category" character varying(100),
    "SubCategory" character varying(100),
    "Duration" interval DEFAULT '00:30:00'::interval NOT NULL,
    "Price" numeric(18,2) NOT NULL,
    "PricePerAdditionalHour" numeric(18,2),
    "BufferBefore" interval,
    "BufferAfter" interval,
    "MaxCapacity" integer DEFAULT 1 NOT NULL,
    "RequiredDocuments" jsonb,
    "Icon" character varying(100),
    "Color" character varying(20),
    "ImageUrl" character varying(500),
    "DisplayOrder" integer DEFAULT 0 NOT NULL,
    "IsActive" boolean DEFAULT true NOT NULL,
    "IsPopular" boolean DEFAULT false NOT NULL,
    "CreatedAt" date NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    CONSTRAINT "PK_Services" PRIMARY KEY ("Id")
);

-- Appointments table
CREATE TABLE public."Appointments" (
    "Id" integer NOT NULL,
    "AppointmentDate" date NOT NULL,
    "StartTime" time without time zone NOT NULL,
    "EndTime" time without time zone NOT NULL,
    "Status" text DEFAULT 'Pending'::text NOT NULL,
    "TotalPrice" numeric(18,2),
    "DiscountAmount" numeric(18,2),
    "FinalPrice" numeric(18,2),
    "Currency" character varying(10),
    "CreatedAt" date NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedByUserId" character varying(100),
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "Notes" character varying(1000),
    "ServiceId" integer NOT NULL,
    "CustomerId" integer NOT NULL,
    "RowVersion" bytea,
    "ApplicationUserId" text,
    CONSTRAINT "PK_Appointments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Appointments_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES public."Customers"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Appointments_Services_ServiceId" FOREIGN KEY ("ServiceId") REFERENCES public."Services"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Appointments_Users_ApplicationUserId" FOREIGN KEY ("ApplicationUserId") REFERENCES public."Users"("Id")
);

-- ============================================
-- SEQUENCES (Auto-increment)
-- ============================================

CREATE SEQUENCE public."Appointments_Id_seq" START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;
ALTER TABLE public."Appointments" ALTER COLUMN "Id" SET DEFAULT nextval('public."Appointments_Id_seq"');

CREATE SEQUENCE public."BusinessProfiles_Id_seq" START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;
ALTER TABLE public."BusinessProfiles" ALTER COLUMN "Id" SET DEFAULT nextval('public."BusinessProfiles_Id_seq"');

CREATE SEQUENCE public."Customers_Id_seq" START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;
ALTER TABLE public."Customers" ALTER COLUMN "Id" SET DEFAULT nextval('public."Customers_Id_seq"');

CREATE SEQUENCE public."RoleClaims_Id_seq" START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;
ALTER TABLE public."RoleClaims" ALTER COLUMN "Id" SET DEFAULT nextval('public."RoleClaims_Id_seq"');

CREATE SEQUENCE public."Services_Id_seq" START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;
ALTER TABLE public."Services" ALTER COLUMN "Id" SET DEFAULT nextval('public."Services_Id_seq"');

CREATE SEQUENCE public."UserClaims_Id_seq" START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;
ALTER TABLE public."UserClaims" ALTER COLUMN "Id" SET DEFAULT nextval('public."UserClaims_Id_seq"');

-- ============================================
-- INDEXES (Performance)
-- ============================================

CREATE UNIQUE INDEX "UserNameIndex" ON public."Users" USING btree ("NormalizedUserName");
CREATE INDEX "EmailIndex" ON public."Users" USING btree ("NormalizedEmail");
CREATE UNIQUE INDEX "RoleNameIndex" ON public."Roles" USING btree ("NormalizedName");
CREATE UNIQUE INDEX "IX_Customers_Email" ON public."Customers" USING btree ("Email");
CREATE UNIQUE INDEX "IX_Customers_PhoneNumber" ON public."Customers" USING btree ("PhoneNumber");
CREATE UNIQUE INDEX "IX_Appointments_NoDoubleBooking" ON public."Appointments" USING btree ("ServiceId", "AppointmentDate", "StartTime");
CREATE INDEX "IX_Appointments_AppointmentDate_StartTime_EndTime" ON public."Appointments" USING btree ("AppointmentDate", "StartTime", "EndTime");
CREATE INDEX "IX_Services_Name" ON public."Services" USING btree ("Name");
CREATE INDEX "IX_Services_Category_SubCategory" ON public."Services" USING btree ("Category", "SubCategory");

-- ============================================
-- SEED DATA (Initial data)
-- ============================================

INSERT INTO public."Roles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") VALUES 
('a8f97219-0c5a-457a-97fb-cd091bd0cf6e', 'Admin', 'ADMIN', '15e023fa-64d0-4617-99a1-65cde5f15fa8'),
('1743eeb8-9c41-46da-ae06-1770feb4eaa2', 'User', 'USER', 'a07a6bf8-6fe8-4281-baf3-b21e52054007');

INSERT INTO public."Users" ("Id", "FullName", "DateOfBirth", "Gender", "PreferDarkMode", "PreferredLanguage", "EnableNotifications", "CreatedAt", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount") VALUES 
('11240251-b0b6-4d56-ad0c-832b2bea0ad6', 'Admin', '1999-12-02', 1, false, 'ar', true, '2026-06-07', 'admin@system.com', 'ADMIN@SYSTEM.COM', 'admin@system.com', 'ADMIN@SYSTEM.COM', true, 'AQAAAAIAAYagAAAAEOl+nPDaP68gIqatwsaOV4n4KHk819tv+j054lyQZTInXdTrH4r/BCRAWTkI3YJuJQ==', 'U76FBQWIJEZEVLJFU35GNJ5OJXQNZXV2', '434771b2-8857-4e10-a79b-f38acdeef4c4', '01234567890', false, false, true, 0);

INSERT INTO public."UserRoles" ("UserId", "RoleId") VALUES 
('11240251-b0b6-4d56-ad0c-832b2bea0ad6', 'a8f97219-0c5a-457a-97fb-cd091bd0cf6e');

INSERT INTO public."BusinessProfiles" ("Id", "BusinessName", "BusinessType", "Colors_Primary", "Colors_Secondary", "Colors_Accent", "Localization_Currency", "Localization_Language", "Localization_Direction", "WorkingHoursStart", "WorkingHoursEnd", "SlotDurationMinutes", "CustomLabels", "CreatedAt") VALUES 
(1, 'نظام حجز المواعيد', 'شامل', '#2c6e7c', '#3a9e8f', '#f39c12', 'ج.م', 'ar', 'rtl', '09:00:00', '22:00:00', 30, '{"service": "الخدمات", "customer": "العملاء", "appointment": "الحجوزات", "serviceItem": "الخدمة"}', '2026-06-07');

SELECT setval('public."BusinessProfiles_Id_seq"', 1, true);