export function combineAuthorParts(obj: any): any {
  if (obj == null || typeof obj !== "object") return obj;

  // Falls "author" ein Array ist
  if (Array.isArray(obj.author)) {

    // Top-Level Affiliation immer anlegen
    obj.affiliation = [];

    obj.author = obj.author.map(a => {
      const given = a.given?.trim() ?? "";
      const family = a.family?.trim() ?? "";
      const fullName = `${given} ${family}`.trim();

      // affiliation hochziehen (auch wenn leer)
      if (Array.isArray(a.affiliation)) {
        obj.affiliation.push(...a.affiliation);
      }

      return {
        name: fullName
      };
    });
  }

  // rekursiv in andere Strukturen gehen
  for (const key in obj) {
    if (typeof obj[key] === "object") {
      obj[key] = combineAuthorParts(obj[key]);
    }
  }

  return obj;
}
