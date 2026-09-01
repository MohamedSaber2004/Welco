# Attachment Service — Frontend Integration Guide

> Dedicated reference for integrating the **Attachment microservice** (`Attachment.Services.API`) from the frontend: every endpoint (route, request, response), how files are stored/named, and — most importantly — **how to resolve any stored file name (e.g. `imageName`) into a working URL**.

---

## 1. What this service does

Handles every file in the platform: images, videos, audio and documents (datasheets, PDFs, certificates). Other microservices **do not store binary files** — they only store the **file name** that the Attachment service returned at upload time (e.g. `ProductDto.imageName`, `UserProfileDto.profilePictureName`, `CertificationDto.certificationImageName`). It is always the frontend's job to turn that name into a URL.

### Where the files physically live
```
{storage root}            (UploadPaths.RootPath; dev = Welco.API/wwwroot)
├── Providers/             (place = 1)
├── Users/                 (place = 2)
├── Uploads/               (place 3–12; written by the API, see gotcha §8.3)
└── default.png            (place = 0 fallback)
```

---

## 2. Base URLs & gateway routing

The frontend calls the **gateway**, which forwards the paths 1:1 to the attachment service (development port **7180**):

| Path | Gateway → downstream |
|---|---|
| `/api/v1/attachments/{everything}` | `https://localhost:7180` (dev) |
| `/files/{everything}` | `https://localhost:7180` (dev) |

| Environment | Gateway Base URL |
|---|---|
| Development (HTTP) | `http://localhost:5293` |
| Development (HTTPS) | `https://localhost:7166` |
| Test (hosted) | `https://welco-gateway.runasp.net` |

All examples below use `{base}` as a placeholder for the current environment's gateway base URL.

---

## 3. How files are named — **read this first**

Every upload returns a **"stored name"** string in `data`:

```
{place}_{<32-hex-guid>}.{ext}     e.g.  1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.png
```

The leading integer (`place`) **encodes which folder the file lives in**:

| place | Folder | Typical use |
|:---:|---|---|
| `0` | default path | generic |
| `1` | `Providers` | product / category / catalog images |
| `2` | `Users` | user profile pictures |
| `3` – `12` | `Uploads` | generic uploads (⚠ see §8.3) |

**Store this string verbatim** in the entity's field (`imageName`, `profilePictureName`, `certificationImageName`, …). Do **not** store the original filename, and do **not** prefix/suffix it yourself.

---

## 4. Resolving a file name into a URL (the key part)

The stored name alone is enough to build a working URL:

```
GET {base}/files/{storedName}
```

Example:
```
storedName = 1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.png
URL        = https://welco-gateway.runasp.net/files/1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.png
```

Why this works: the gateway proxies `/files/{everything}` to the Attachment service, whose static-file handler (`CustomFileProvider`) reads the `{place}_` prefix, resolves the folder (`1` → `Providers`), and streams the matching file. It also falls back to searching the configured folders (`Providers`, `Users`) and the storage root, so a bare name usually resolves too.

> **Rule for the frontend:** whenever a DTO returns an `…ImageName` / `…PictureName` / file-name field, render it as **`{base}/files/{value}`**. Treat an empty/`null` value as "no image" and show a placeholder.

---

## 5. Endpoints

**Auth:** upload/update require `Authorization: Bearer <JWT>` with role `OrganizationUser`, `WelcoStaff`, or `Admin`. Download and static file access are **public**.

### 5.1 POST `{base}/api/v1/attachments/upload` — upload a single file → `201`

`Content-Type: multipart/form-data`

| Form field | Type | Required | Constraints |
|---|---|---|---|
| `file` | file | ✅ | non-empty |
| `place` | int | ✅ | `0`–`2` for this endpoint |
| `fileType` | int | ✅ | `0` Image, `1` Video, `2` Audio, `3` File |

```
POST /api/v1/attachments/upload
Authorization: Bearer <token>
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary

------WebKitFormBoundary
Content-Disposition: form-data; name="file"; filename="logo.png"
Content-Type: image/png

<binary data>
------WebKitFormBoundary
Content-Disposition: form-data; name="place"

1
------WebKitFormBoundary
Content-Disposition: form-data; name="fileType"

0
------WebKitFormBoundary--
```

Response `201` — `data` is the **stored name**:
```json
{
  "isSuccess": true, "statusCode": 201,
  "message": "AttachmentMessages.FileUploaded",
  "errors": [],
  "data": "1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.png"
}
```

### 5.2 POST `{base}/api/v1/attachments/upload-multiple` — upload several files → `201`

`Content-Type: multipart/form-data`. Repeat the same field name for multiple files in a category.

| Form field | Type | Notes |
|---|---|---|
| `images` | `List<file>` | repeatable |
| `imagesPlace` | int | folder for all images |
| `videos` | `List<file>` | repeatable |
| `videosPlace` | int | |
| `audios` | `List<file>` | repeatable |
| `audiosPlace` | int | |
| `documents` | `List<file>` | repeatable |
| `documentsPlace` | int | |

At least one file list must contain a file (else `400`).

Response `201` — `data` is an **array of stored names**:
```json
{
  "isSuccess": true, "statusCode": 201,
  "message": "AttachmentMessages.FileUploaded",
  "errors": [],
  "data": [
    "1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.jpg",
    "3_d5e6f7g8a1b2c3d4e5f6a7b8c9d0e1f2.pdf"
  ]
}
```

### 5.3 PUT `{base}/api/v1/attachments/{name}` — replace an existing file → `200`

`{name}` = the **current stored name** of the file being replaced (the same string you received from upload). The old file is deleted, the new one uploaded.

`Content-Type: multipart/form-data`

| Form field | Type | Required | Constraints |
|---|---|---|---|
| `file` | file | ✅ | non-empty |
| `place` | int | ✅ | `0`–`12` |
| `fileType` | int | ✅ | `0`–`3` |

> `oldFileName` is **not** a form field — it comes from the URL path.

Response `200` — `data` is the **new stored name** (persist it in the entity field):
```json
{
  "isSuccess": true, "statusCode": 200,
  "message": "AttachmentMessages.FileUploaded",
  "errors": [],
  "data": "1_9f8e7d6c5b4a3c2d1e0f9a8b7c6d5e4f3.png"
}
```

### 5.4 GET `{base}/api/v1/attachments/download` — file metadata (public)

Query string:

| Param | Type | Required | Constraints |
|---|---|---|---|
| `place` | int | ✅ | `0`–`12` |
| `fileName` | string | ✅ | the stored name (its `place_` prefix overrides `place`) |

```
GET /api/v1/attachments/download?place=1&fileName=1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.png
```

Response `200` — `data` = `FileResponseDto`:
```json
{
  "isSuccess": true, "statusCode": 200,
  "message": "AttachmentMessages.FileDownloaded",
  "errors": [],
  "data": {
    "filePath": "Providers/1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.png",
    "fileName": "1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.png",
    "contentType": "image/png",
    "success": true,
    "errorMessage": null
  }
}
```
If the file does not exist the envelope is still `200`/`isSuccess:true` but `data.success=false` and `data.errorMessage` is set. Use this endpoint when you need the exact MIME type or relative folder; for plain display use §4.

### 5.5 GET `{base}/files/{storedName}` — static file (public, recommended for display)

Streams the raw bytes. Use directly in `<img src>`, `<video src>`, `<a href>` / `window.open()`.
```
GET /files/1_3fa8c5d0a1b2c3d4e5f6a7b8c9d0e1f2.png
```

---

## 6. File type rules (`fileType` / `MediaType`)

| fileType | Value | Max size | Allowed extensions |
|:---:|:---:|---:|---|
| Image | `0` | 5 MB | `.jpg` `.jpeg` `.png` `.gif` `.bmp` `.webp` |
| Video | `1` | 100 MB | `.mp4` `.avi` `.mkv` `.mov` `.wmv` |
| Audio | `2` | 10 MB | `.mp3` `.wav` `.ogg` `.m4a` `.aac` |
| File | `3` | 10 MB | `.pdf` `.doc` `.docx` `.xls` `.xlsx` `.txt` `.zip` `.rar` |

Invalid type/size/extension → `400` with `errors` populated.

---

## 7. Related files — DTOs that carry stored file names

These are the fields the frontend will receive from other services and must resolve with §4.

| DTO / Endpoint | Field (camelCase) | Example value | Where returned |
|---|---|---|---|
| `UserProfileDto` (GET/PUT `/api/v1/auth/profile`) | `profilePictureName` | `2_…png` | Auth service |
| `UserDto` (GET `/api/v1/user-management/users`) | `profilePictureName` | `2_…png` | UserManagement service |
| `ProductDto` (GET `/api/v1/products`) | `imageName` | `1_…png` | Product service |
| `CategoryDto` (GET `/api/v1/categories`) | `imageName` | `1_…png` | Product service |
| `CertificationDto` (GET `/api/v1/certifications`) | `certificationImageName` | `1_…jpg` | Certification service |

**Resolution (identical for every field):**
```
src = "{base}/files/" + dto.imageName
```

Related source files (for developers, not the frontend):
- `Attachment.Services.API/Controllers/AttachmentController.cs`
- `Attachment.Services.API/Infrastructure/BaseFileService.cs`, `FilePathHelper.cs`, `ImageValidator.cs`, `VideoValidator.cs`, `AudioValidator.cs`, `FileValidator.cs`
- `Attachment.Services.API/Features/Attachments/Commands/{UploadFile,UploadMultiple,UpdateFile,DownloadFile}/`
- `Welco.Shared/Common/Services/UploadPaths.cs`, `CustomFileProvider.cs`
- `Welco.API/Ocelot/ocelot.attachment.{Development,Test,Production}.json`

---

## 8. Frontend implementation examples

### 8.1 URL helper
```js
// baseUrl = current environment gateway base, e.g. "https://welco-gateway.runasp.net"
export function resolveFileUrl(storedName) {
  if (!storedName) return null;            // null/"" → show placeholder
  return `${baseUrl}/files/${storedName}`; // e.g. baseUrl/files/1_3fa8….png
}

// usage
<img src={resolveFileUrl(product.imageName) ?? "/images/placeholder.png"} alt={product.nameEn} />
<a href={resolveFileUrl(cert.certificationImageName)} download>View certificate</a>
```

### 8.2 Single upload
```js
async function uploadFile(file, place, fileType, token) {
  const form = new FormData();
  form.append("file", file);      // field name MUST be "file"
  form.append("place", place);    // number, e.g. 1
  form.append("fileType", fileType); // number: 0 image | 1 video | 2 audio | 3 file

  const res = await fetch(`${baseUrl}/api/v1/attachments/upload`, {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` }, // do NOT set Content-Type
    body: form,
  });
  const json = await res.json();   // { isSuccess, statusCode, message, errors, data }
  if (!json.isSuccess) throw new Error(json.errors.join(", "));
  return json.data;                // stored name, e.g. "1_3fa8….png"
}
```

### 8.3 Multi upload
```js
async function uploadImages(files, place, token) {
  const form = new FormData();
  files.forEach((f) => form.append("images", f)); // repeat the field name
  form.append("imagesPlace", place);

  const res = await fetch(`${baseUrl}/api/v1/attachments/upload-multiple`, {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: form,
  });
  const json = await res.json();
  return json.data; // string[]
}
```

---

## 9. Gotchas

1. **`place` is part of the file's identity.** Always send it again when updating (`PUT /attachments/{name}`) so the new file lands in the same folder.
2. **Update replaces — persist the new name.** After `PUT /attachments/{name}` save the returned name back to the entity (`imageName`, etc.), otherwise the UI points at a deleted file.
3. **⚠ place `3`–`12` and static display:** files uploaded with `place` 3–12 are written to an `Uploads/` folder that the static `/files` handler does **not** currently search (it only resolves `Providers`, `Users` and the storage root). For anything the frontend must display via `/files/{name}`, use `place` **1** (or `2` for user avatars). If you must use 3–12, resolve via the download endpoint (§5.4) to get the folder path.
4. **Enums are numbers.** `fileType` / `place` must be sent as integers in the multipart form, not strings.
5. **Don't set `Content-Type` header** on upload requests — the browser must generate the multipart boundary.
6. **Auth:** uploads/updates return `401` without a valid token; downloads and `/files` are public.
7. **Response envelopes:** always check `isSuccess`/`statusCode` first; `message`/`errors` are localized (`Accept-Language: en|ar`).
8. **Delete/replace old-file errors are silent** — the endpoint still succeeds and returns the new name even if the old file couldn't be removed.
