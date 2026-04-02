import { writable } from 'svelte/store';
import { get } from 'svelte/store';
import * as CleaningUtils from './dataCleaningUtils';

export type TailorResultRow = {
	id: number,
    originalName: string,
    cleanedName: string,
    editedName: string,
    confirmedByUser: boolean,
    matchedName: string,
    matchType: string,
    status: string,
    matchSource: string,
    matchSourceVersion: string,
    timeStampMatch: string
}

let originalRows: TailorResultRow[] = [

]

let cleanedRows: TailorResultRow[] = [

]

export let tailorResultStore = writable<TailorResultRow[]>(originalRows);
export let tailorCleanedStore = writable<TailorResultRow[]>(cleanedRows);

export function initializeTableData(data: TailorResultRow[]) {
    const processed = data.map(item => ({
        ...item,
        cleanedName: cleanName(item.originalName),
        editedName: item.editedName || ''
    }));

    tailorCleanedStore.set(processed);
}

export function toggleDataCleaning() {
    const currentRows = get(tailorCleanedStore);

    const processed = currentRows.map(item => ({
        ...item,
        cleanedName: cleanName(item.originalName),
        editedName: item.editedName || ''
    }));

    tailorCleanedStore.set(processed);
}

export const cleanConfig = {
    stripSymbols: {
        apply: true,
        description: ""
    },
    removeSymbols: {
        apply: true,
        description: ""
    },
    replaceDiacritics: {
        apply: true,
        description: ""
    },
    replaceNonTrailing: {
        apply: true,
        description: ""
    },
    standardizeHybrids: {
        apply: true,
        description: ""
    },
    deleteAfterEqual: {
        apply: true,
        description: ""
    },
    cleanHybridFormulas: {
        apply: true,
        description: ""
    },
    deleteTripleHybrids: {
        apply: true,
        description: ""
    },
    removeCultivars: {
        apply: true,
        description: ""
    },
    cleanMiddleHyphens: {
        apply: true,
        description: ""
    },
    deleteTaxonomicAbbreviations: {
        apply: true,
        description: ""
    },
    deleteHabitatDescriptors: {
        apply: true,
        description: ""
    },
    deleteGeneralNoise: {
        apply: true,
        description: ""
    },
    deleteLeadingDescriptors: {
        apply: true,
        description: ""
    },
    truncateFromBeginning: {
        apply: true,
        description: ""
    },
    truncateFromMarker: {
        apply: true,
        description: ""
    },
    truncateFromGeographicOrBreeding: {
        apply: true,
        description: ""
    },
    truncateFromUncertainty: {
        apply: true,
        description: ""
    },
    changeVernacularNames: {
        apply: true,
        description: ""
    },
    updateFamilyNames: {
        apply: true,
        description: ""
    },
    deleteUselessMarkers: {
        apply: true,
        description: ""
    },
    correctOcrErrors: {
        apply: true,
        description: ""
    },
    harmonizeAbbreviations: {
        apply: true,
        description: ""
    },
    deletePointAfterKey: {
        apply: true,
        description: ""
    },
    deletePointAfterSpecies: {
        apply: true,
        description: ""
    },
    fixMissingSpaces: {
        apply: true,
        description: ""
    },
    validateFamilySuffix: {
        apply: true,
        description: ""
    },
    informationInParentheses: {
        apply: true,
        description: ""
    },
    correctWritingGenus: {
        apply: true,
        description: ""
    },
    spacesBeforeAndAfterParentheses: {
        apply: true,
        description: ""
    },
    correctionHybrid: {
        apply: true,
        description: ""
    },
    removeAuthors: {
        apply: true,
        description: ""
    },
}

export const cleanName = (name: string) => {
    if (!name) return '';
    name = CleaningUtils.removeSpecialEscapes(name);
    name = CleaningUtils.removeSpecialCharacters(name);

    if (cleanConfig.stripSymbols.apply) {
        name = CleaningUtils.stripInsideSymbols(name, '"');
        name = CleaningUtils.stripInsideSymbols(name, "'");
        name = CleaningUtils.stripInsideSymbols(name, "(", ")");
    }
    
    if (cleanConfig.removeSymbols.apply) {
        name = name.replace(/'/g, '').replace(/"/g, '').replace("(", '').replace(")", '');
    }
    
    if (cleanConfig.replaceDiacritics.apply) {
        name = CleaningUtils.replaceDiacritics(name);
    }

    name = CleaningUtils.removeNumbers(name);
    
    if (cleanConfig.replaceNonTrailing.apply) {
        name = CleaningUtils.replaceNonTrailingSymbolsWithSpace(name, "_");
        name = CleaningUtils.replaceNonTrailingSymbolsWithSpace(name, ".");
    }

    name = CleaningUtils.deleteNumeral(name);

    if (cleanConfig.standardizeHybrids.apply) {
        name = CleaningUtils.standardizeHybrids(name);
    }
    
    if (cleanConfig.deleteAfterEqual.apply) {
        name = CleaningUtils.deleteAfterEqual(name);
    }
    
    if (cleanConfig.cleanHybridFormulas.apply) {
        name = CleaningUtils.cleanHybridFormulas(name);
    }

    if (cleanConfig.deleteTripleHybrids.apply) {
        name = CleaningUtils.deleteTripleHybrids(name);
    }
    
    if (cleanConfig.removeCultivars.apply) {
        name = CleaningUtils.removeCultivars(name);
    }
    
    if (cleanConfig.cleanMiddleHyphens.apply) {
        name = CleaningUtils.cleanMiddleHyphens(name);
    }
    
    if (cleanConfig.deleteTaxonomicAbbreviations.apply) {
        name = CleaningUtils.deleteTaxonomicAbbreviations(name);
    }

    if (cleanConfig.stripSymbols.apply) {
        name = CleaningUtils.deleteHabitatDescriptors(name);
    }
    
    if (cleanConfig.deleteGeneralNoise.apply) {
        name = CleaningUtils.deleteGeneralNoise(name);
    }
    
    if (cleanConfig.deleteLeadingDescriptors.apply) {
        name = CleaningUtils.deleteLeadingDescriptors(name);
    }
    
    if (cleanConfig.truncateFromBeginning.apply) {
        name = CleaningUtils.truncateFromBeginning(name);
    }
    
    if (cleanConfig.truncateFromMarker.apply) {
        name = CleaningUtils.truncateFromMarker(name);
    }
    
    if (cleanConfig.truncateFromGeographicOrBreeding.apply) {
        name = CleaningUtils.truncateFromGeographicOrBreeding(name);
    }
    
    if (cleanConfig.truncateFromUncertainty.apply) {
        name = CleaningUtils.truncateFromUncertainty(name);
    }
    
    if (cleanConfig.changeVernacularNames.apply) {
        name = CleaningUtils.changeVernacularNames(name);
    }
    
    if (cleanConfig.updateFamilyNames.apply) {
        name = CleaningUtils.updateFamilyNames(name);
    }
    
    if (cleanConfig.deleteUselessMarkers.apply) {
        name = CleaningUtils.deleteUselessMarkers(name);
    }
    
    if (cleanConfig.correctOcrErrors.apply) {
        name = CleaningUtils.correctOcrErrors(name);
    }
    
    if (cleanConfig.harmonizeAbbreviations.apply) {
        name = CleaningUtils.harmonizeAbbreviations(name);
    }
    
    if (cleanConfig.deletePointAfterKey.apply) {
        name = CleaningUtils.deletePointAfterKey(name);
    }
    
    if (cleanConfig.deletePointAfterSpecies.apply) {
        name = CleaningUtils.deletePointAfterSpecies(name);
    }
    
    if (cleanConfig.fixMissingSpaces.apply) {
        name = CleaningUtils.fixMissingSpaces(name);
    }

    if (cleanConfig.validateFamilySuffix.apply) {
        name = CleaningUtils.validateFamilySuffix(name);
    }
    
    if (cleanConfig.informationInParentheses.apply) {
        name = CleaningUtils.informationInParentheses(name);
    }
    
    if (cleanConfig.correctWritingGenus.apply) {
        name = CleaningUtils.correctWritingGenus(name);
    }
    
    if (cleanConfig.spacesBeforeAndAfterParentheses.apply) {
        name = CleaningUtils.spacesBeforeAndAfterParentheses(name);
    }
    
    if (cleanConfig.correctionHybrid.apply) {
        name = CleaningUtils.correctionHybrid(name);
    }
    
    if (cleanConfig.removeAuthors.apply) {
        name = CleaningUtils.removeAuthors(name);
    }

    return name;
}