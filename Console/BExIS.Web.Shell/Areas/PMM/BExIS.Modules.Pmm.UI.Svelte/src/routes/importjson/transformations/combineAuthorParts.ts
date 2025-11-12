export function combineAuthorParts(obj: any): any {
  if (obj === null || typeof obj !== "object") return obj;

  // Prüfen, ob das Objekt ein "author"-Array hat
  if (Array.isArray(obj.author)) {
    obj.author = obj.author.map((a: any) => {
      const given = a.given?.trim() ?? "";
      const family = a.family?.trim() ?? "";
      return `${given} ${family}`.trim();
    });
  }

  // Rekursiv in verschachtelten Strukturen weitermachen
  for (const key in obj) {
    if (typeof obj[key] === "object") {
      obj[key] = combineAuthorParts(obj[key]);
    }
  }

  return obj;
}
