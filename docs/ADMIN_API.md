# IranConnect — Admin API Reference

مرجع کامل endpointهای پنل ادمین برای اتصال فرانت‌اند.

## پایه

| مورد | مقدار |
|------|-------|
| Base URL | `https://<host>/api` |
| Prefix ادمین | `/api/admin` |
| احراز هویت | `Authorization: Bearer <accessToken>` |
| دسترسی | فقط ادمین — policy `AdminOnly` = claim `role == "Admin"` در JWT |
| فرمت | JSON (request body camelCase، response camelCase) |

### گرفتن توکن

`POST /api/auth/login`

Request:
```json
{ "email": "admin@x.com", "password": "...", "deviceInfo": null, "ipAddress": null }
```
Response 200:
```json
{
  "accessToken": "ey...",         // در Header همه‌ی درخواست‌های ادمین
  "refreshToken": "...",
  "expiresAt": "2026-06-27T12:00:00Z",
  "email": "admin@x.com",
  "fullName": null,
  "plan": "Premium",
  "isEmailVerified": true
}
```
> توکن باید claim `role=Admin` داشته باشه وگرنه همه‌ی endpointهای زیر **403** می‌دن. accessToken ~۶۰ دقیقه اعتبار؛ با `POST /api/auth/refresh-token` تمدید.

### کدهای خطای مشترک

| کد | معنی |
|----|------|
| 401 | توکن نیست/منقضی |
| 403 | توکن ادمین نیست |
| 404 | منبع پیدا نشد |
| 409 | تضاد (مثلاً رسید قبلاً بررسی شده) |
| خطا | بدنه: `{ "error": "متن خطا" }` |

### مدل صفحه‌بندی (`PagedResult<T>`)
```json
{ "items": [ ... ], "totalCount": 0, "page": 1, "pageSize": 20, "totalPages": 0 }
```

---

# 1) کاربران

## GET `/api/admin/users` — لیست کاربران
Query: `page=1` · `pageSize=20` · `search` (اختیاری، ایمیل/نام) · `plan` (اختیاری: `Free`/`Premium`)

Response 200 — `PagedResult<UserSummary>`:
```json
{
  "items": [{
    "id": "guid",
    "email": "u@x.com",
    "fullName": "نام",
    "isEmailVerified": true,
    "isActive": true,
    "plan": "Free",
    "subscriptionStatus": "Active",
    "expireDate": "2026-07-01T00:00:00Z",
    "createdAt": "2026-06-01T00:00:00Z",
    "lastLoginAt": "2026-06-26T10:00:00Z"
  }],
  "totalCount": 120, "page": 1, "pageSize": 20, "totalPages": 6
}
```

## GET `/api/admin/users/{userId}` — جزئیات کاربر
Path: `userId` (Guid). Response 200 — `UserDetailResponse`:
```json
{
  "id": "guid", "email": "u@x.com", "fullName": "نام",
  "isEmailVerified": true, "isActive": true, "isDeviceUser": false,
  "createdAt": "...", "lastLoginAt": "...",
  "subscription": {
    "plan": "Premium", "status": "Active",
    "startDate": "...", "expireDate": "...",
    "daysRemaining": 25, "isActive": true
  },
  "peer": {
    "assignedIp": "10.0.0.2/32", "publicKey": "...",
    "isOnline": true, "bytesReceivedHuman": "1.2 GB",
    "bytesSentHuman": "300 MB", "lastHandshake": "...", "lastSeenAt": "..."
  },
  "recentReceipts": [{
    "id": "guid", "status": "Approved",
    "requestedDurationDays": 30, "submittedAt": "...", "reviewedAt": "..."
  }]
}
```
`peer` ممکنه `null` باشه (کاربر بدون peer). خطا: 404.

## PUT `/api/admin/users/{userId}/upgrade` — ارتقای اشتراک
Body — `UpgradeUserRequest`:
```json
{ "durationDays": 30 }
```
Response: 200 (بدون بدنه) · 404.

## PUT `/api/admin/users/{userId}/deactivate` — غیرفعال‌سازی
بدون body. Response: 200 · 404.

## PUT `/api/admin/users/{userId}/activate` — فعال‌سازی مجدد
بدون body. Response: 200 · 404.

## DELETE `/api/admin/users/{userId}` — حذف کاربر
Response: 200 · 404. (peer وایرگارد کاربر هم حذف می‌شه.)

---

# 2) آمار

## GET `/api/admin/stats` — آمار کلی
Response 200 — `StatsResponse`:
```json
{
  "totalUsers": 120, "activeUsers": 100,
  "freeUsers": 80, "premiumUsers": 40,
  "expiredSubscriptions": 10,
  "newUsersToday": 3, "newUsersThisMonth": 25
}
```

## GET `/api/admin/stats/daily` — آمار روزانه
Query: `days=30`. Response 200 — `List<DailyStatItem>`:
```json
[{ "date": "2026-06-26T00:00:00Z", "newUsers": 3, "activeUsers": 40 }]
```

## GET `/api/admin/stats/monthly` — آمار ماهانه
Query: `months=12`. Response 200 — `List<MonthlyStatItem>`:
```json
[{
  "year": 2026, "month": 6, "monthName": "June",
  "newUsers": 25, "approvedPayments": 12, "totalRevenue": 6000000
}]
```

---

# 3) رسیدهای پرداخت

## GET `/api/admin/receipts` — لیست رسیدها
Query: `status` (اختیاری: `Pending`/`Approved`/`Rejected`؛ پیش‌فرض pending) · `page=1` · `pageSize=20`

Response 200 — `PagedResult<ReceiptAdminResponse>`:
```json
{
  "items": [{
    "id": "guid", "userId": "guid", "userEmail": "u@x.com",
    "userFullName": "نام", "payerFullName": "پرداخت‌کننده",
    "lastFourDigits": "1234",
    "storedFileName": "abc.jpg", "originalFileName": "receipt.jpg",
    "status": "Pending", "requestedDurationDays": 30,
    "submittedAt": "...", "adminNote": null, "reviewedAt": null
  }],
  "totalCount": 5, "page": 1, "pageSize": 20, "totalPages": 1
}
```

## GET `/api/admin/users/{userId}/receipts` — رسیدهای یک کاربر
Response 200 — `List<AdminReceiptResponse>`:
```json
[{
  "id": "guid", "payerFullName": "...", "lastFourDigits": "1234",
  "storedFileName": "abc.jpg", "originalFileName": "receipt.jpg",
  "status": "Approved", "requestedDurationDays": 30,
  "submittedAt": "...", "adminNote": "تأیید شد", "reviewedAt": "..."
}]
```

## PUT `/api/admin/receipts/{receiptId}/review` — تأیید/رد رسید
Body — `ReviewReceiptRequest`:
```json
{ "approved": true, "note": "اختیاری" }
```
Response: 200 · 404 · 409 (قبلاً بررسی شده). تأیید → اشتراک کاربر تمدید.

## GET `/api/admin/receipts/{receiptId}/file` — دانلود فایل رسید
Response: **باینری فایل** (`image/jpeg` · `image/png` · `application/pdf`)، نه JSON. خطا: 404.
> برای نمایش: `<img src>` با header توکن نمی‌شه؛ یا fetch+blob یا توکن در query (اگه پشتیبانی شه). فعلاً fetch با Bearer → blob → objectURL.

---

# 4) VPN / Peers

## GET `/api/admin/vpn/peers` — آمار همه‌ی peerها
Response 200 — `List<PeerStatsResponse>`:
```json
[{
  "userId": "guid", "email": "u@x.com",
  "assignedIp": "10.0.0.2/32", "publicKey": "...",
  "isOnline": true,
  "bytesReceived": 1288490188, "bytesSent": 314572800,
  "bytesReceivedHuman": "1.2 GB", "bytesSentHuman": "300 MB",
  "lastHandshake": "...", "lastSeenAt": "..."
}]
```

## GET `/api/admin/vpn/peers/{userId}` — جزئیات peer کاربر
Response 200 — `PeerDetailResponse`:
```json
{
  "userId": "guid", "userEmail": "u@x.com",
  "assignedIp": "10.0.0.2/32", "publicKey": "...",
  "isOnline": true, "isActive": true,
  "bytesReceived": 0, "bytesSent": 0,
  "bytesReceivedHuman": "0 B", "bytesSentHuman": "0 B",
  "lastHandshake": "...", "lastSeenAt": "...", "createdAt": "...",
  "bandwidthLimitBytes": 10737418240, "bandwidthLimitHuman": "10 GB"
}
```
`bandwidthLimit*` ممکنه `null` (بدون محدودیت). خطا: 404.

## DELETE `/api/admin/vpn/peers/{userId}` — حذف peer
Response: 200 · 404. (از interface زنده awg + DB حذف می‌شه.)

## POST `/api/admin/vpn/peers/{userId}/reset` — ریست peer
بدون body. کلید/IP جدید می‌سازه. Response: 200 · 404.

## PUT `/api/admin/vpn/peers/{userId}/bandwidth-limit` — تنظیم سقف مصرف
Body — `SetBandwidthLimitRequest`:
```json
{ "limitBytes": 10737418240 }
```
`limitBytes: null` = حذف محدودیت. Response: 200 · 404.
> هنگام عبور از سقف، سرویس پس‌زمینه peer رو غیرفعال می‌کنه.

## GET `/api/admin/vpn/bandwidth` — گزارش مصرف همه
Query: `page=1` · `pageSize=20` · `sortBy=total` (`total`/`received`/`sent`)

Response 200 — `PagedResult<BandwidthReportItem>`:
```json
{
  "items": [{
    "userId": "guid", "email": "u@x.com", "assignedIp": "10.0.0.2/32",
    "isOnline": true,
    "bytesReceived": 0, "bytesSent": 0, "totalBytes": 0,
    "bytesReceivedHuman": "0 B", "bytesSentHuman": "0 B", "totalBytesHuman": "0 B",
    "limitBytes": null, "limitHuman": null,
    "hasExceededLimit": false, "lastSeenAt": "..."
  }],
  "totalCount": 50, "page": 1, "pageSize": 20, "totalPages": 3
}
```

## GET `/api/admin/vpn/bandwidth/{userId}` — مصرف یک کاربر
Response 200 — `UserBandwidthResponse`:
```json
{
  "userId": "guid", "email": "u@x.com",
  "bytesReceived": 0, "bytesSent": 0, "totalBytes": 0,
  "bytesReceivedHuman": "0 B", "bytesSentHuman": "0 B", "totalBytesHuman": "0 B",
  "limitBytes": 10737418240, "limitHuman": "10 GB",
  "usagePercent": 12.5, "hasExceededLimit": false
}
```
`usagePercent`/`limit*` ممکنه `null`. خطا: 404.

## GET `/api/admin/vpn/online` — کاربران آنلاین همزمان
Response 200 — `OnlineStatsResponse`:
```json
{
  "totalPeers": 50, "onlinePeers": 12,
  "totalBytesReceived": 0, "totalBytesSent": 0,
  "totalBytesReceivedHuman": "0 B", "totalBytesSentHuman": "0 B"
}
```

---

# 5) کاتالوگ اپ‌های مجاز (Iranian Apps)

اپ‌هایی که کلاینت از تونل عبور می‌ده (split tunneling). قبلاً لیست ثابت در RAM بود؛ حالا جدول DB با CRUD ادمین. هر اپ: `packageName` (applicationId)، `nameEn`، `nameFa` (Title فارسی)، `isFree` (Free/Premium)، `isActive`.

> کاتالوگ عمومی برای کلاینت: `GET /api/subscription/apps` (فقط اپ‌های `isActive`، فیلدهای packageName/nameEn/nameFa/isFree). endpointهای زیر مدیریتیه و فقط ادمین.

مدل خروجی مشترک — `AdminAppResponse`:
```json
{
  "id": "guid",
  "packageName": "com.samanpr.blu",
  "nameEn": "Blu Bank",
  "nameFa": "بلوبانک",
  "isFree": true,
  "isActive": true,
  "createdAt": "2026-06-27T00:00:00Z",
  "updatedAt": null
}
```

## GET `/api/admin/apps` — لیست کاتالوگ (شامل غیرفعال‌ها)
Query (همه اختیاری): `search` (در nameEn/nameFa/packageName) · `isFree` (true/false) · `isActive` (true/false)

Response 200 — `List<AdminAppResponse>` (همون مدل بالا، آرایه).

## POST `/api/admin/apps` — افزودن اپ
Body — `CreateAppRequest`:
```json
{ "packageName": "com.example.app", "nameEn": "Example", "nameFa": "نمونه", "isFree": false }
```
Response: **201** + `AdminAppResponse` · 400 (فیلد خالی) · 409 (packageName تکراری).

## PUT `/api/admin/apps/{id}` — ویرایش اپ
Path: `id` (Guid). Body — `UpdateAppRequest`:
```json
{ "packageName": "com.example.app", "nameEn": "Example New", "nameFa": "نمونه جدید" }
```
> `isFree` اینجا تغییر نمی‌کنه — برای tier از endpoint بعدی استفاده کن.

Response: 200 + `AdminAppResponse` · 404 · 409 (packageName برای اپ دیگه).

## DELETE `/api/admin/apps/{id}` — حذف اپ
Response: 200 `{ "data": "اپ '...' حذف شد" }` (متن موفقیت) · 404.

## PUT `/api/admin/apps/{id}/tier` — تغییر Free/Premium
Body — `SetAppTierRequest`:
```json
{ "isFree": true }
```
`true` = Free، `false` = Premium. Response: 200 + `AdminAppResponse` · 404.

---

# جدول خلاصه

| # | Method | Path | توضیح |
|---|--------|------|-------|
| 1 | GET | `/admin/users` | لیست کاربران (صفحه‌بندی/جستجو/فیلتر plan) |
| 2 | GET | `/admin/users/{userId}` | جزئیات کاربر |
| 3 | PUT | `/admin/users/{userId}/upgrade` | ارتقای اشتراک |
| 4 | PUT | `/admin/users/{userId}/deactivate` | غیرفعال‌سازی |
| 5 | PUT | `/admin/users/{userId}/activate` | فعال‌سازی |
| 6 | DELETE | `/admin/users/{userId}` | حذف کاربر |
| 7 | GET | `/admin/stats` | آمار کلی |
| 8 | GET | `/admin/stats/daily` | آمار روزانه |
| 9 | GET | `/admin/stats/monthly` | آمار ماهانه |
| 10 | GET | `/admin/receipts` | لیست رسیدها |
| 11 | GET | `/admin/users/{userId}/receipts` | رسیدهای کاربر |
| 12 | PUT | `/admin/receipts/{receiptId}/review` | تأیید/رد رسید |
| 13 | GET | `/admin/receipts/{receiptId}/file` | دانلود فایل رسید (باینری) |
| 14 | GET | `/admin/vpn/peers` | آمار همه peerها |
| 15 | GET | `/admin/vpn/peers/{userId}` | جزئیات peer |
| 16 | DELETE | `/admin/vpn/peers/{userId}` | حذف peer |
| 17 | POST | `/admin/vpn/peers/{userId}/reset` | ریست peer |
| 18 | PUT | `/admin/vpn/peers/{userId}/bandwidth-limit` | سقف مصرف |
| 19 | GET | `/admin/vpn/bandwidth` | گزارش مصرف همه |
| 20 | GET | `/admin/vpn/bandwidth/{userId}` | مصرف کاربر |
| 21 | GET | `/admin/vpn/online` | آنلاین همزمان |
| 22 | GET | `/admin/apps` | لیست کاتالوگ اپ (شامل غیرفعال) |
| 23 | POST | `/admin/apps` | افزودن اپ |
| 24 | PUT | `/admin/apps/{id}` | ویرایش اپ (نام/package) |
| 25 | DELETE | `/admin/apps/{id}` | حذف اپ |
| 26 | PUT | `/admin/apps/{id}/tier` | تغییر Free/Premium |

> همه با `Authorization: Bearer <token>` (role=Admin). تاریخ‌ها ISO-8601 UTC. مقادیر `*Human` رشته‌ی آماده‌ی نمایش، مقادیر خام بایت برای محاسبه.
> کاتالوگ عمومی کلاینت (بدون ادمین): `GET /api/subscription/apps`.
