export function combineDateParts(obj: any): any {
  if (obj === null || typeof obj !== "object") return obj;

  // Wenn obj selbst ein "date-parts"-Objekt ist
  if (obj["date-parts"] && Array.isArray(obj["date-parts"])) {
    const dp = obj["date-parts"];
    let year: number | undefined;
    let month: number | undefined;
    let day: number | undefined;

    // Variante 1: [[2025, 11, 6]]
    if (Array.isArray(dp[0]) && typeof dp[0][0] === "number" && dp[0].length >= 1) {
      [year, month, day] = dp[0];
    }
    // Variante 2: [[2025],[11],[6]]
    else if (
      dp.length >= 1 &&
      Array.isArray(dp[0]) &&
      typeof dp[0][0] === "number"
    ) {
      year = dp[0][0];
      month = dp[1]?.[0];
      day = dp[2]?.[0];
    }

    // Datum zusammenbauen
    if (year) {
      const y = year.toString();
      const m = month ? String(month).padStart(2, "0") : "01";
      const d = day ? String(day).padStart(2, "0") : "01";
      return `${y}-${m}-${d}`; // Direkt als String zurückgeben
    }
  }

  // Ansonsten rekursiv durch die Kinder iterieren
  if (Array.isArray(obj)) {
    return obj.map((item) => combineDateParts(item));
  } else {
    const result: any = {};
    for (const key in obj) {
      result[key] = combineDateParts(obj[key]);
    }
    return result;
  }
}
