-- ============================================
-- SEED DATA FOR APPOINTMENT BOOKING SYSTEM
-- For testing and demo purposes only
-- Generated: 2026-06-08
-- ============================================

-- ============================================
-- 1. SERVICES (خدمات متنوعة)
-- ============================================

-- خدمات صالون/حلاقة
INSERT INTO public."Services" (
    "Name", "Description", "Category", "SubCategory", 
    "Duration", "Price", "PricePerAdditionalHour", 
    "BufferBefore", "BufferAfter", "MaxCapacity",
    "Icon", "Color", "DisplayOrder", "IsActive", "IsPopular",
    "CreatedAt", "IsDeleted"
) VALUES 
-- صالون رجالي

('حلاقة كاملة','قص + غسل + تصفيف + عطر', 'صالون', 'رجالي', '01:00:00', 250.00, NULL, '00:10:00', '00:10:00', 1, 'bi-gem', '#3a9e8f', 2, true, true, CURRENT_DATE, false ),
 
('قص شعر رجالي', 'قص شعر احترافي بأحدث الموديلات', 'صالون', 'رجالي','00:30:00', 150.00, NULL, '00:05:00', '00:05:00', 1,'bi-scissors', '#2c6e7c', 1, true, true, CURRENT_DATE, false),
 
('لحية', 'تهذيب وتشذيب اللحية', 'صالون', 'رجالي','00:20:00', 80.00, NULL, '00:05:00', '00:05:00', 1,'bi-scissors', '#f39c12', 3, true, false, CURRENT_DATE, false),

-- خدمات صالون نسائي
('قص شعر نسائي', 'قص وتصفيف شعر نسائي', 'صالون', 'نسائي', '00:45:00', 200.00, NULL, '00:10:00', '00:10:00', 1, 'bi-scissors', '#e74c3c', 4, true, true, CURRENT_DATE, false),

('صبغ شعر', 'صبغ شعر بألوان طبيعية', 'صالون', 'نسائي', '02:00:00', 500.00, 100.00, '00:15:00', '00:15:00', 1, 'bi-droplet', '#9b59b6', 5, true, true, CURRENT_DATE, false),

('مكياج', 'مكياج كامل للمناسبات', 'صالون', 'نسائي', '01:30:00', 400.00, NULL, '00:10:00', '00:10:00', 1, 'bi-brush', '#e91e63', 6, true, true, CURRENT_DATE, false),

-- خدمات عيادة/مركز طبي
('كشف طبي عام', 'كشف طبي واستشارة', 'عيادة', 'طب عام', '00:30:00', 300.00, NULL, '00:05:00', '00:05:00', 1, 'bi-heart-pulse', '#2c6e7c', 7, true, true, CURRENT_DATE, false),

('علاج طبيعي', 'جلسة علاج طبيعي', 'عيادة', 'علاج طبيعي', '01:00:00', 450.00, 150.00, '00:10:00', '00:10:00', 1, 'bi-activity', '#3a9e8f', 8, true, true, CURRENT_DATE, false),

('عناية بالبشرة', 'جلسة تنظيف وتقشير البشرة', 'تجميل', 'بشرة', '01:00:00', 350.00, NULL, '00:10:00', '00:10:00', 1, 'bi-flower1', '#8e44ad', 10, true, false, CURRENT_DATE, false);


-- ============================================
-- 2. CUSTOMERS (عملاء تجريبيون)
-- ============================================

INSERT INTO public."Customers" (
    "FullName", "PhoneNumber", "Email", "Notes", 
    "TotalAppointments", "CreatedAt", "IsDeleted"
) VALUES 
('أحمد محمد علي', '01012345678', 'ahmed@example.com', 'عميل دائم - يفضل الصباح', 5, CURRENT_DATE, false),
('سارة خالد حسن', '01123456789', 'sara@example.com', 'حساسية من بعض المنتجات', 3, CURRENT_DATE, false),
('محمد عبدالله', '01234567880', 'mohamed@example.com', 'يحتاج مواعيد مسائية', 2, CURRENT_DATE, false),
('نورا أحمد', '01098765432', 'nora@example.com', NULL, 1, CURRENT_DATE, false),
('خالد محمود', '01187654321', 'khaled@example.com', 'عميل قديم', 4, CURRENT_DATE, false),
('فاطمة إبراهيم', '01234567890', 'fatma@example.com', 'تطلب خدمات منزلية', 2, CURRENT_DATE, false),
('عمر سامي', '01055554444', 'omar@example.com', NULL, 0, CURRENT_DATE, false),
('ليلى مصطفى', '01166667777', 'leila@example.com', 'تفضل المدربة (فاطمة)', 1, CURRENT_DATE, false),
('يوسف كريم', '01277778888', 'yousef@example.com', 'يحتاج كرسي متحرك', 0, CURRENT_DATE, false),
('مريم طارق', '01088889999', 'maryam@example.com', 'عميل جديد', 0, CURRENT_DATE, false);


-- ============================================
-- 3. APPOINTMENTS 
-- ============================================
WITH 
services_data AS (
    SELECT 
        s."Id", 
        s."Duration", 
        s."BufferBefore", 
        s."BufferAfter",
        s."Price"
    FROM public."Services" s
    WHERE s."IsDeleted" = false
),
customers_data AS (
    SELECT "Id", "FullName" 
    FROM public."Customers" 
    WHERE "IsDeleted" = false
),
today AS (SELECT CURRENT_DATE as d),
-- توليد حجوزات ماضية (آخر 60 يوم)
past_appointments AS (
    SELECT 
        c."Id" as "CustomerId",
        s."Id" as "ServiceId",
        s."Duration",
        s."BufferBefore",
        s."BufferAfter",
        s."Price",
        -- تاريخ عشوائي بين 60 يوم مضت وأمس
        (CURRENT_DATE - (floor(random() * 60)::int || ' days')::interval)::date as "AppointmentDate",
        -- وقت بدء عشوائي بين 9 صباحاً و 8 مساءً
        ('09:00:00'::time + (floor(random() * 11)::int || ' hours')::interval)::time as "StartTime",
        -- حالات مناسبة للماضي
        CASE (floor(random() * 3)::int)
            WHEN 0 THEN 3  -- Completed
            WHEN 1 THEN 4  -- Cancelled
            ELSE 5         -- NoShow
        END as "Status",
        ('ماضي - ' || c."FullName") as "Notes"
    FROM customers_data c
    CROSS JOIN services_data s
    WHERE (CURRENT_DATE - interval '60 days') <= CURRENT_DATE - interval '1 day'
    LIMIT 25
),
-- حجوزات حالية (اليوم والغد)
current_appointments AS (
    SELECT 
        c."Id" as "CustomerId",
        s."Id" as "ServiceId",
        s."Duration",
        s."BufferBefore",
        s."BufferAfter",
        s."Price",
        (CURRENT_DATE + (floor(random() * 2)::int || ' days')::interval)::date as "AppointmentDate",
        ('10:00:00'::time + (floor(random() * 8)::int || ' hours')::interval)::time as "StartTime",
        CASE (floor(random() * 2)::int)
            WHEN 0 THEN 1  -- Confirmed
            ELSE 0         -- Pending
        END as "Status",
        ('حالي - ' || c."FullName") as "Notes"
    FROM customers_data c
    CROSS JOIN services_data s
    LIMIT 15
),
-- حجوزات مستقبلية (بعد 3 أيام حتى 30 يوم)
future_appointments AS (
    SELECT 
        c."Id" as "CustomerId",
        s."Id" as "ServiceId",
        s."Duration",
        s."BufferBefore",
        s."BufferAfter",
        s."Price",
        (CURRENT_DATE + (3 + floor(random() * 27)::int) || ' days')::interval as "AppointmentDate",
        ('11:00:00'::time + (floor(random() * 7)::int || ' hours')::interval)::time as "StartTime",
        CASE (floor(random() * 3)::int)
            WHEN 0 THEN 0  -- Pending
            WHEN 1 THEN 1  -- Confirmed
            ELSE 6         -- Rescheduled
        END as "Status",
        ('مستقبل - ' || c."FullName") as "Notes"
    FROM customers_data c
    CROSS JOIN services_data s
    LIMIT 20
),
-- دمج كل الحجوزات
all_appointments AS (
    SELECT * FROM past_appointments
    UNION ALL
    SELECT * FROM current_appointments
    UNION ALL
    SELECT * FROM future_appointments
),
-- منع التكرارات
unique_appointments AS (
    SELECT DISTINCT ON ("CustomerId", "ServiceId", "AppointmentDate", "StartTime")
        *
    FROM all_appointments
    ORDER BY "CustomerId", "ServiceId", "AppointmentDate", "StartTime"
),
-- حساب EndTime
appointments_final AS (
    SELECT 
        "CustomerId",
        "ServiceId",
        "AppointmentDate",
        "StartTime",
        -- حساب وقت الانتهاء
        (("StartTime"::interval) + 
         COALESCE("BufferBefore", '00:00:00'::interval) + 
         "Duration" + 
         COALESCE("BufferAfter", '00:00:00'::interval)
        )::time as "EndTime",
        "Status",
        "Price" as "TotalPrice",
        -- خصم عشوائي 0% أو 10%
        CASE WHEN random() < 0.8 THEN 0 ELSE 10 END as "DiscountPercent",
        "Notes"
    FROM unique_appointments
    WHERE (("StartTime"::interval) + 
           COALESCE("BufferBefore", '00:00:00'::interval) + 
           "Duration" + 
           COALESCE("BufferAfter", '00:00:00'::interval)
          )::time <= '23:00:00'::time  -- منع الحجوزات المتأخرة جداً
)

-- الإدراج النهائي
INSERT INTO public."Appointments" (
    "CustomerId",
    "ServiceId",
    "AppointmentDate",
    "StartTime",
    "EndTime",
    "Status",
    "TotalPrice",
    "DiscountAmount",
    "FinalPrice",
    "Currency",
    "Notes",
    "CreatedAt",
    "UpdatedAt",
    "IsDeleted",
    "CreatedByUserId"
)
SELECT 
    "CustomerId",
    "ServiceId",
    "AppointmentDate",
    "StartTime",
    "EndTime",
    "Status",
    "TotalPrice",
    ROUND("TotalPrice" * "DiscountPercent" / 100.0, 2) as "DiscountAmount",
    ROUND("TotalPrice" * (100 - "DiscountPercent") / 100.0, 2) as "FinalPrice",
    'EGP' as "Currency",
    "Notes",
    CURRENT_TIMESTAMP as "CreatedAt",
    NULL as "UpdatedAt",
    false as "IsDeleted",
    'system-seed' as "CreatedByUserId"
FROM appointments_final
LIMIT 50;  -- الحد الأقصى 50 حجزاً


-- ============================================
-- 4. UPDATE CUSTOMER STATISTICS
-- ============================================

-- تحديث إحصائيات العملاء بناءً على الحجوزات الفعلية
UPDATE public."Customers" SET 
    "TotalAppointments" = (
        SELECT COUNT(*) FROM public."Appointments" 
        WHERE "CustomerId" = public."Customers"."Id" 
        AND "Status" != 'Cancelled'
    ),
    "LastAppointmentDate" = (
        SELECT MAX("AppointmentDate") FROM public."Appointments" 
        WHERE "CustomerId" = public."Customers"."Id"
        AND "Status" != 'Cancelled'
    )
WHERE EXISTS (
    SELECT 1 FROM public."Appointments" 
    WHERE "CustomerId" = public."Customers"."Id"
);


-- ============================================
-- 5. UPDATE SERVICES WITH APPOINTMENT COUNT
-- ============================================

-- تحديث عدد الحجوزات لكل خدمة (يُستخدم في Dashboard)
-- ملاحظة: هذا حقل محسوب يمكن تركه للتحديث التلقائي


-- ============================================
-- 6. VERIFICATION QUERIES (للتحقق)
-- ============================================


SELECT COUNT(*) FROM public."Customers";
SELECT COUNT(*) FROM public."Services";
SELECT COUNT(*) FROM public."Appointments";

SELECT "Id","FullName"
FROM public."Customers"
ORDER BY "Id";

SELECT "Id","Name"
FROM public."Services"
ORDER BY "Id";

SELECT "Id","AppointmentDate"
FROM public."Appointments"
ORDER BY "Id";

-- عرض الحجوزات حسب الحالة
SELECT 'Status: ' || "Status", COUNT(*) FROM public."Appointments" GROUP BY "Status";
