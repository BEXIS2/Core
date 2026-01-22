// src/transformations/index.ts
import { combineDateParts } from "./combineDateParts";
import { combineAuthorParts } from "./combineAuthorParts"
import { extractLatestLicense } from "./extractLatestLicense"
// Hier kannst du beliebig viele Regeln importieren

export type TransformationRule = (obj: any) => any;

// Alle Regeln, die du anwenden willst
const rules: TransformationRule[] = [
  combineDateParts,
  combineAuthorParts,
  extractLatestLicense,
  // weitereRegel,
];

export function applyTransformations(data: any): any {
  return rules.reduce((acc, rule) => rule(acc), data);
}
