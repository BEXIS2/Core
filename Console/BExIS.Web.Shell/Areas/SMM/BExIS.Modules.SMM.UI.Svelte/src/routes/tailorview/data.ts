import { writable } from 'svelte/store';
import { get } from 'svelte/store';
import * as CleaningUtils from './dataCleaningUtils';
import { type SpeciesMatchingRow } from "$lib/types/types";


let rows: SpeciesMatchingRow[] = []

export let tailorEditStore = writable<SpeciesMatchingRow[]>(rows);

let onlyEditsRows: SpeciesMatchingRow[] = []

export let tailorOnlyEditsStore = writable<SpeciesMatchingRow[]>(rows);

export const cleanConfig = {
    sanitize_whitespaces: {
        apply: true,
        description: "Replace non-breaking spaces, zero-width spaces, tabs, and unusual Unicode spaces. Collapse multiple consecutive spaces into a single space."
    },
    normalize_chars_and_dashes: {
        apply: true,
        description: "Normalize en-dashes, em-dashes, and non-breaking hyphens to standard ASCII hyphens. Standardize quotes and apostrophes if present."
    },
    standardize_hybrids: {
        apply: true,
        description: "Infix hybrid: Genus x species or Genus X species or Genus ✕ species -> Genus × species. Prefix hybrid: x Genus species at start of string -> × Genus species."
    },
    standardize_infraspecifics: {
        apply: true,
        description: "Matches exact rank keywords bounded by word boundaries to avoid mangling author initials."
    },
    capitalize_genus: {
        apply: true,
        description: "Capitalize the first letter of the Genus (or the word right after a leading hybrid '×')."
    },
    trim: {
        apply: true,
        description: "Trim double spaces into one."
    }
}

/**
 * Carefully cleans a scientific species name string while preserving author data.
 * Follows a conservative "do no harm" strategy: normalizes whitespace, dashes,
 * rank markers, and hybrid symbols without dropping any text or altering author capitalization.
 *
 * @param rawName - The raw species string (with or without author info, no commas).
 * @returns The safely cleaned canonical string.
 */
export function cleanName(rawName: string): string {
    if (!rawName) return '';

    let name = rawName;

    // --------------------------------------------------------------------------
    // 1. Whitespace & Control Character Sanitization
    // --------------------------------------------------------------------------
    if (cleanConfig.sanitize_whitespaces.apply) {
        name = name
        // Replace non-breaking spaces, zero-width spaces, tabs, and unusual Unicode spaces
        .replace(/[\u00A0\u1680\u2000-\u200A\u202F\u205F\u3000\uFEFF\t]/g, ' ')
        // Collapse multiple consecutive spaces into a single space
        .replace(/\s+/g, ' ')
        .trim();
    }

    if (!name) return '';

    // --------------------------------------------------------------------------
    // 2. Character & Dash Normalization
    // --------------------------------------------------------------------------
    if (cleanConfig.normalize_chars_and_dashes.apply) {
        name = name
        // Normalize en-dashes, em-dashes, and non-breaking hyphens to standard ASCII hyphens
        .replace(/[\u2010-\u2015]/g, '-')
        // Standardize quotes and apostrophes if present
        .replace(/[`'’‘]/g, "'");
    }

    // --------------------------------------------------------------------------
    // 3. Standardization of Hybrid Markers
    // --------------------------------------------------------------------------
    if (cleanConfig.standardize_hybrids.apply) {
        name = name
        // Infix hybrid: "Genus x species" or "Genus X species" or "Genus ✕ species" -> "Genus × species"
        .replace(/\s+[xX×✕✖]\s+/g, ' × ')
        // Prefix hybrid: "x Genus species" at start of string -> "× Genus species"
        .replace(/^[xX×✕✖]\s+/g, '× ');
    }

    // --------------------------------------------------------------------------
    // 4. Safe Standardization of Infraspecific Rank Indicators
    // --------------------------------------------------------------------------
    if (cleanConfig.standardize_infraspecifics.apply) {
        // Matches exact rank keywords bounded by word boundaries to avoid mangling author initials.
        name = name
        .replace(/\b(subsp|ssp|sub-sp)\.?\b/gi, 'subsp.')
        .replace(/\b(var)\.?\b/gi, 'var.')
        .replace(/\b(subvar)\.?\b/gi, 'subvar.')
        .replace(/\b(forma)\b/gi, 'f.');
    }

    // --------------------------------------------------------------------------
    // 5. Safe Genus Capitalization
    // --------------------------------------------------------------------------
    // Capitalize the first letter of the Genus (or the word right after a leading hybrid '×').
    if (cleanConfig.capitalize_genus.apply) {
        if (name.startsWith('× ')) {
            const rest = name.slice(2);
            name = '× ' + rest.charAt(0).toUpperCase() + rest.slice(1);
        } else {
            name = name.charAt(0).toUpperCase() + name.slice(1);
        }
    }

    // --------------------------------------------------------------------------
    // 6. Final Polish Trim
    // --------------------------------------------------------------------------
    if (cleanConfig.trim.apply) {
        return name.replace(/\s+/g, ' ').trim();
    } else {
        return name;
    }
}




// export const cleanConfig = {
//     stripSymbols: {
//         apply: true,
//         description: ""
//     },
//     removeSymbols: {
//         apply: true,
//         description: ""
//     },
//     replaceDiacritics: {
//         apply: true,
//         description: ""
//     },
//     replaceNonTrailing: {
//         apply: true,
//         description: ""
//     },
//     standardizeHybrids: {
//         apply: true,
//         description: ""
//     },
//     deleteAfterEqual: {
//         apply: true,
//         description: ""
//     },
//     cleanHybridFormulas: {
//         apply: true,
//         description: ""
//     },
//     deleteTripleHybrids: {
//         apply: true,
//         description: ""
//     },
//     removeCultivars: {
//         apply: true,
//         description: ""
//     },
//     cleanMiddleHyphens: {
//         apply: true,
//         description: ""
//     },
//     deleteTaxonomicAbbreviations: {
//         apply: true,
//         description: ""
//     },
//     deleteHabitatDescriptors: {
//         apply: true,
//         description: ""
//     },
//     deleteGeneralNoise: {
//         apply: true,
//         description: ""
//     },
//     deleteLeadingDescriptors: {
//         apply: true,
//         description: ""
//     },
//     truncateFromBeginning: {
//         apply: true,
//         description: ""
//     },
//     truncateFromMarker: {
//         apply: true,
//         description: ""
//     },
//     truncateFromGeographicOrBreeding: {
//         apply: true,
//         description: ""
//     },
//     truncateFromUncertainty: {
//         apply: true,
//         description: ""
//     },
//     changeVernacularNames: {
//         apply: true,
//         description: ""
//     },
//     updateFamilyNames: {
//         apply: true,
//         description: ""
//     },
//     deleteUselessMarkers: {
//         apply: true,
//         description: ""
//     },
//     correctOcrErrors: {
//         apply: true,
//         description: ""
//     },
//     harmonizeAbbreviations: {
//         apply: true,
//         description: ""
//     },
//     deletePointAfterKey: {
//         apply: true,
//         description: ""
//     },
//     deletePointAfterSpecies: {
//         apply: true,
//         description: ""
//     },
//     fixMissingSpaces: {
//         apply: true,
//         description: ""
//     },
//     validateFamilySuffix: {
//         apply: true,
//         description: ""
//     },
//     informationInParentheses: {
//         apply: true,
//         description: ""
//     },
//     correctWritingGenus: {
//         apply: true,
//         description: ""
//     },
//     spacesBeforeAndAfterParentheses: {
//         apply: true,
//         description: ""
//     },
//     correctionHybrid: {
//         apply: true,
//         description: ""
//     },
//     removeAuthors: {
//         apply: true,
//         description: ""
//     },
// }

// export const cleanName = (name: string) => {
//     if (!name) return '';
//     name = CleaningUtils.removeSpecialEscapes(name);
//     name = CleaningUtils.removeSpecialCharacters(name);

//     if (cleanConfig.stripSymbols.apply) {
//         name = CleaningUtils.stripInsideSymbols(name, '"');
//         name = CleaningUtils.stripInsideSymbols(name, "'");
//         name = CleaningUtils.stripInsideSymbols(name, "(", ")");
//     }
    
//     if (cleanConfig.removeSymbols.apply) {
//         name = name.replace(/'/g, '').replace(/"/g, '').replace("(", '').replace(")", '');
//     }
    
//     if (cleanConfig.replaceDiacritics.apply) {
//         name = CleaningUtils.replaceDiacritics(name);
//     }

//     name = CleaningUtils.removeNumbers(name);
    
//     if (cleanConfig.replaceNonTrailing.apply) {
//         name = CleaningUtils.replaceNonTrailingSymbolsWithSpace(name, "_");
//         name = CleaningUtils.replaceNonTrailingSymbolsWithSpace(name, ".");
//     }

//     name = CleaningUtils.deleteNumeral(name);

//     if (cleanConfig.standardizeHybrids.apply) {
//         name = CleaningUtils.standardizeHybrids(name);
//     }
    
//     if (cleanConfig.deleteAfterEqual.apply) {
//         name = CleaningUtils.deleteAfterEqual(name);
//     }
    
//     if (cleanConfig.cleanHybridFormulas.apply) {
//         name = CleaningUtils.cleanHybridFormulas(name);
//     }

//     if (cleanConfig.deleteTripleHybrids.apply) {
//         name = CleaningUtils.deleteTripleHybrids(name);
//     }
    
//     if (cleanConfig.removeCultivars.apply) {
//         name = CleaningUtils.removeCultivars(name);
//     }
    
//     if (cleanConfig.cleanMiddleHyphens.apply) {
//         name = CleaningUtils.cleanMiddleHyphens(name);
//     }
    
//     if (cleanConfig.deleteTaxonomicAbbreviations.apply) {
//         name = CleaningUtils.deleteTaxonomicAbbreviations(name);
//     }

//     if (cleanConfig.stripSymbols.apply) {
//         name = CleaningUtils.deleteHabitatDescriptors(name);
//     }
    
//     if (cleanConfig.deleteGeneralNoise.apply) {
//         name = CleaningUtils.deleteGeneralNoise(name);
//     }
    
//     if (cleanConfig.deleteLeadingDescriptors.apply) {
//         name = CleaningUtils.deleteLeadingDescriptors(name);
//     }
    
//     if (cleanConfig.truncateFromBeginning.apply) {
//         name = CleaningUtils.truncateFromBeginning(name);
//     }
    
//     if (cleanConfig.truncateFromMarker.apply) {
//         name = CleaningUtils.truncateFromMarker(name);
//     }
    
//     if (cleanConfig.truncateFromGeographicOrBreeding.apply) {
//         name = CleaningUtils.truncateFromGeographicOrBreeding(name);
//     }
    
//     if (cleanConfig.truncateFromUncertainty.apply) {
//         name = CleaningUtils.truncateFromUncertainty(name);
//     }
    
//     if (cleanConfig.changeVernacularNames.apply) {
//         name = CleaningUtils.changeVernacularNames(name);
//     }
    
//     if (cleanConfig.updateFamilyNames.apply) {
//         name = CleaningUtils.updateFamilyNames(name);
//     }
    
//     if (cleanConfig.deleteUselessMarkers.apply) {
//         name = CleaningUtils.deleteUselessMarkers(name);
//     }
    
//     if (cleanConfig.correctOcrErrors.apply) {
//         name = CleaningUtils.correctOcrErrors(name);
//     }
    
//     if (cleanConfig.harmonizeAbbreviations.apply) {
//         name = CleaningUtils.harmonizeAbbreviations(name);
//     }
    
//     if (cleanConfig.deletePointAfterKey.apply) {
//         name = CleaningUtils.deletePointAfterKey(name);
//     }
    
//     if (cleanConfig.deletePointAfterSpecies.apply) {
//         name = CleaningUtils.deletePointAfterSpecies(name);
//     }
    
//     if (cleanConfig.fixMissingSpaces.apply) {
//         name = CleaningUtils.fixMissingSpaces(name);
//     }

//     if (cleanConfig.validateFamilySuffix.apply) {
//         name = CleaningUtils.validateFamilySuffix(name);
//     }
    
//     if (cleanConfig.informationInParentheses.apply) {
//         name = CleaningUtils.informationInParentheses(name);
//     }
    
//     if (cleanConfig.correctWritingGenus.apply) {
//         name = CleaningUtils.correctWritingGenus(name);
//     }
    
//     if (cleanConfig.spacesBeforeAndAfterParentheses.apply) {
//         name = CleaningUtils.spacesBeforeAndAfterParentheses(name);
//     }
    
//     if (cleanConfig.correctionHybrid.apply) {
//         name = CleaningUtils.correctionHybrid(name);
//     }
    
//     if (cleanConfig.removeAuthors.apply) {
//         name = CleaningUtils.removeAuthors(name);
//     }

//     return name;
// }
