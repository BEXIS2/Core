export function combineAuthorParts(obj: any): any {
  if (obj == null || typeof obj !== "object") return obj;

  if (Array.isArray(obj.author)) {

    // Sammelbehälter nur temporär
    const collectedAffiliations: any[] = [];

    obj.author = obj.author.map(a => {
      const given = a.given?.trim() ?? "";
      const family = a.family?.trim() ?? "";
      const fullName = `${given} ${family}`.trim();

      // Affiliations sammeln, aber NICHT hochziehen wenn leer
      if (Array.isArray(a.affiliation) && a.affiliation.length > 0) {
        collectedAffiliations.push(...a.affiliation);
      }

      return fullName;
    });

    // Nur setzen wenn wirklich welche drin sind
    if (collectedAffiliations.length > 0) {
      obj.affiliation = collectedAffiliations;
    }
  }

  // Rekursion
  for (const key in obj) {
    if (typeof obj[key] === "object") {
      obj[key] = combineAuthorParts(obj[key]);
    }
  }

  return obj;
}
