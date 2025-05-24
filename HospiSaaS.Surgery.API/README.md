# HospiSaaS Operating-Room Scheduler (API)

A lightweight **.NET 9 Web-API** that lets hospital doctors request operating-room (OR) slots, find out whether they were **scheduled** or added to the **waiting-list**, and (optionally) promote waiting items.
All data is kept **in-memory**—no database required.

---

## 1  Run the service

```bash
# restore & launch  (HTTPS on :8080 by default)
dotnet run --project HospiSaaS.Surgery.API
```

Startup seeds **one hospital**, **four doctors** and **five ORs** matching the assignment spec.

---

## 2  Interactive docs

| UI                                | URL                                                                      |
| --------------------------------- |--------------------------------------------------------------------------|
| **Swagger UI**                    | [https://localhost:8080/swagger](https://localhost:8080/swagger)         |
| **Scalar** (nicer OpenAPI viewer) | [https://localhost:8080/scalar](https://localhost:8080/scalar) |

Open either link in your browser, expand an endpoint and click **“Try it out”**—no curl needed.

---

## 3  Essential endpoints (with curl)

> Replace `$HOSP` with
> `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa` (the seeded hospital ID).

### 3.1 Request a surgery slot

```bash
curl -k -X POST https://localhost:5001/api/hospitals/$HOSP/surgeries \
     -H "Content-Type: application/json" \
     -d '{
           "doctorId":       "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
           "surgeryType":    "Heart",
           "desiredTimeUtc": "2025-06-01T12:00:00Z"
         }'
```

| HTTP                | Meaning               | JSON snippet                                 |
| ------------------- | --------------------- | -------------------------------------------- |
| **201 Created**     | OR & time assigned    | `"status":"Scheduled","operatingRoomId":"…"` |
| **202 Accepted**    | Added to waiting-list | `"status":"Waiting","waitingPosition":3`     |
| **400 Bad Request** | Rule violated         | `{ "error":"…"} `                            |

Save the `requestId` to poll status later.

---

### 3.2 Get status of a request

```bash
curl -k \
  https://localhost:5001/api/hospitals/$HOSP/surgeries/{requestId}
```

### 3.3 List all scheduled surgeries

```bash
curl -k https://localhost:5001/api/hospitals/$HOSP/surgeries
```

### 3.4 List current waiting-list

```bash
curl -k https://localhost:5001/api/hospitals/$HOSP/waiting-list
```

### 3.5 Manually promote the waiting-list

The background worker does this every minute, but you can force it:

```bash
curl -k -X POST \
  https://localhost:5001/api/hospitals/$HOSP/waiting-list/process
```

---

## 4  Seed data for instant testing

### Doctors

| Name       | ID (GUID)                              | Specialty |
| ---------- | -------------------------------------- | --------- |
| Dr Moshe   | `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb` | Heart     |
| Dr Yitzhak | `cccccccc-cccc-cccc-cccc-cccccccccccc` | Brain     |
| Dr Maya    | `dddddddd-dddd-dddd-dddd-dddddddddddd` | Heart     |
| Dr Lily    | `eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee` | Heart     |

### Operating rooms

| OR   | Equipment      | Accepts      | Brain duration |
| ---- | -------------- | ------------ | -------------- |
| OR-1 | MRI + CT + ECG | Heart, Brain | **2 h**        |
| OR-2 | MRI + CT       | Heart, Brain | **2 h**        |
| OR-3 | MRI + CT       | Heart, Brain | **2 h**        |
| OR-4 | MRI + ECG      | Heart, Brain | 3 h            |
| OR-5 | MRI + ECG      | Heart, Brain | 3 h            |

*Heart surgery duration: **3 h***
*Brain surgery: **2 h** if OR has CT, otherwise **3 h***
Scheduler prevents any overlap based on these durations.

---

## 5  Common error messages

| Message fragment           | Explanation                               |
| -------------------------- | ----------------------------------------- |
| “10:00 and 18:00”          | Start time outside working hours (local). |
| “within the next 7 days”   | Scheduled too far in the future.          |
| “specialty does not match” | Doctor tried to book wrong surgery type.  |

---

## 6  Automated tests

```bash
dotnet test
```

The xUnit suite covers:

* Immediate scheduling success
* Waiting-list acceptance & promotion
* Working-hours and 7-day rules
* Duration-based overlap detection
* Specialty mismatch handling

---

Clone → `dotnet run` → open **/swagger** → start booking surgeries—done!
