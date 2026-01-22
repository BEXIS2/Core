export function extractLatestLicense(obj: any): any {
  if (obj === null || typeof obj !== "object") return obj;

  // Prüfen, ob dieses Objekt ein "license"-Array enthält
  if (Array.isArray(obj.license) && obj.license.length > 0) {
    const last = obj.license[obj.license.length - 1];
    if (last && typeof last === "object" && typeof last.URL === "string") {
      obj.license = last.URL; // direkt ersetzen durch die URL
    }
  }

  // Rekursiv in verschachtelten Objekten weitermachen
  for (const key in obj) {
    if (typeof obj[key] === "object") {
      obj[key] = extractLatestLicense(obj[key]);
    }
  }

  return obj;
}
