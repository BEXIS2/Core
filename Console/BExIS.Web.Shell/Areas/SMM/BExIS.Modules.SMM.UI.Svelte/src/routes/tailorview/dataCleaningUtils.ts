export const stripInsideSymbols = (text: string, startSym: string, endSym = startSym) => {
    if (!text) return '';

    // We need to escape symbols like '(' or '[' so Regex doesn't think they are code
    const escape = (s: string) => s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');

    const s = escape(startSym);
    const e = escape(endSym);

    // Dynamically build: /startSymbol\s*(.*?)\s*endSymbol/g
    const regex = new RegExp(`${s}\\s*(.*?)\\s*${e}`, 'g');

    // Replace the whole match with: startSymbol + capturedText + endSymbol
    return text.replace(regex, `${startSym}$1${endSym}`);
};

export const removeSpecialEscapes = (text: string) => {
    if (!text) return '';

    // remove newline, tab, carriage return
    let result = text.replace(/[\n\r\t]/g, '');

    // remove potentially created double whitespaces
    return result.replace(/\s\s+/g, ' ').trim();
}

export const removeSpecialCharacters = (text: string) => {
    if (!text) return '';

    const specialChars = /[!?@#\$%&*\^†,¬Ç¡ˆ◊√ó]/g;
    let result = text.replace(specialChars, '');

    result = result.replace(/\.{2,}/g, '.');

    return result.replace(/\s\s+/g, ' ').trim();
}

export const replaceDiacritics = (text: string) => {
    if (!text) return '';

    const diacriticsMap = {
        "á": "a", "é": "e", "√™": "e", "í": "i", "ó": "o", "ú": "u",
        "Á": "A", "É": "E", "Í": "I", "Ó": "O", "Ú": "U",
        "à": "a", "è": "e", "ì": "i", "ò": "o", "ù": "u",
        "À": "A", "È": "E", "Ì": "I", "Ò": "O", "Ù": "U",
        "ã": "a", "ẽ": "e", "ĩ": "i", "õ": "o", "ũ": "u",
        "Ã": "A", "Ẽ": "E", "Ĩ": "I", "Õ": "O", "Ũ": "U",
        "Â": "A", "â": "a", "Ê": "E", "ê": "e", "Î": "I", "î": "i", 
        "Ô": "O", "ô": "o", "Û": "U", "û": "u",
        "Æ": "AE", "æ": "ae", "Ç": "S", "ç": "s", "Œ": "oe",
        "Ä": "AE", "ä": "ae", "Ö": "OU", "ö": "ou", "Ü": "U", "ü": "u",
        "Ÿ": "I", "ÿ": "i"
    };

    // Create a regex that matches any of the keys in our map
    // We use [ ... ] for single chars, but since we have multi-char keys like "√™", 
    // we join them with the OR operator |
    const pattern = new RegExp(
    Object.keys(diacriticsMap)
        .map(key => key.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')) // escape keys
        .join('|'), 
        'g'
    );

    // One single pass over the string!
    return text.replace(pattern, (matched) => diacriticsMap[matched]);
};

export const removeNumbers = (text: string) => {
    if (!text) return '';

    let result = text.replace(/\d/g, '');

    return result.replace(/\s\s+/g, ' ').trim();
}

export const replaceNonTrailingSymbolsWithSpace = (text: string, separator: string) => {
    if (!text) return '';

    // We escape the separator in case it's a dot
    const escaped = separator.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    // Lookaround equivalent in JS: replace separator with space if between word characters
    const regex = new RegExp(`(?<=\\w)${escaped}(?=\\w)`, 'g');
    return text.replace(regex, ' ').trim();
};

// Deletes the # symbol
export const deleteNumeral = (text: string) => text.replace(/#/g, '');

export const standardizeHybrids = (text: string) => {
    if (!text) return '';

    let result = text;

    // 1. Ensure 'ex' followed by Uppercase has spaces: 'exName' -> ' ex Name'
    result = result.replace(/\s*ex(?=[A-Z])/g, ' ex ');

    // 2. Remove leading 'x' or 'X' if followed by Uppercase: 'x Gardenia' -> 'Gardenia'
    // ^\s* matches start of string plus any whitespace
    result = result.replace(/^\s*x\s*(?=[A-Z])/i, ''); // 'i' flag handles x and X

    return result.replace(/\s\s+/g, ' ').trim();
};

export const deleteAfterEqual = (text: string) => {
    if (!text) return '';

    // Replace '=' and everything after it (.*) with a space
    return text.replace(/=.*/, '').trim();
};

export const cleanHybridFormulas = (text: string) => {
    if (!text) return '';

    let result = text;

    // 1. Identify the first word (Genus)
    const firstSpaceIndex = result.trim().indexOf(' ');
    if (firstSpaceIndex !== -1) {
        const genus = result.substring(0, firstSpaceIndex);
        const escapedGenus = genus.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        // 2. Remove the genus if it repeats after an ' x '
        // Example: "Quercus alba x Quercus robur" -> "Quercus alba x robur"
        // We use a dynamic Regex to find: space + x + space + genus + space
        const redundantGenusRegex = new RegExp(` x ${escapedGenus} `, 'g');
        result = result.replace(redundantGenusRegex, ' x ');
    }

    // 3. Fix double 'x' markers
    result = result.replace(/x\s+x/g, 'x');

    // 4. Handle 'x Q.' or 'x Q ' (where Q is any genus initial)
    // This replaces 'x' followed by an initial and a dot/space with just ' x '
    result = result.replace(/x\s+[A-Z]\b\.?/g, ' x ');

    // 5. Final space cleanup
    return result.replace(/\s\s+/g, ' ').trim();
};

export const deleteTripleHybrids = (text: string) => {
    if (!text) return '';

    // Split the string by the hybrid marker
    const parts = text.split(' x ');

    // If there's more than one ' x ' (meaning 3 or more parts)
    if (parts.length > 2) {
    // Take only the first two parts and join them back
    return `${parts[0]} x ${parts[1]}`.trim();
    }

    return text.trim();
};

export const removeCultivars = (text: string) => {
    if (!text) return '';

    let result = text;

    // This regex matches:
    // 1. Optional 'cv.' or 'cv' (case insensitive)
    // 2. Followed by text in either 'single' or "double" quotes
    // 3. Or just the quoted text alone
    const cultivarRegex = /\s*(cv\.?)?\s*(['"])(?:(?!\2).)+\2/gi;

    result = result.replace(cultivarRegex, ' ');

    return result.replace(/\s\s+/g, ' ').trim();
};

export const cleanMiddleHyphens = (text: string) => {
    if (!text) return '';

    let result = text;

    // 1. Collapse spaces around hyphens: 'Word - Word' -> 'Word-Word'
    // Matches whitespace before and/or after a hyphen as long as text exists on both sides
    result = result.replace(/(?<=\S)\s*-\s*(?=\S)/g, '-');

    // 2. Truncate at space-dash-space followed by a Capital Letter
    // 'Pinus sylvestris - Note' -> 'Pinus sylvestris'
    result = result.replace(/\s+-\s+[A-Z].*/, '');

    // 3. Convert remaining isolated ' - ' to a single space
    result = result.replace(/\s+-\s+/g, ' ');

    // 4. Remove trailing hyphen at the end of the string
    result = result.replace(/\s*-$/, '');

    return result.replace(/\s\s+/g, ' ').trim();
};

export const deleteTaxonomicAbbreviations = (text: string) => {
    if (!text) return '';

    const badList = [
    "especie", "taxon", "s(ens)?\\.? ?str\\.", "s(ens)?\\. ?l(at)?\\.", "gen(us)?", "comb",
    "agg?r?", "subfo", "subg", "subgen", "subgrp", "aff?", "ef", "cf", "cff", "indet", "indeterminate",
    "indeterminad\\w", "inconnue", "ined", "non ?det", "sp\\w?", "sppl?", "spec", "species", "nov(o|a)?",
    "sp\\.?nov", "orth", "subspecies"
    ];

    // Join the list into a single (word1|word2|word3) pattern
    const joinedPatterns = badList.join('|');

    // The 'Lasso': Matches start of string, space, hyphen, or dot
    // before and after the forbidden words.
    const before = '(^|[ \\-\\.\\/])';
    const after = '\\.?([ \\-\\.\\/]|$|\\.)';

    const regex = new RegExp(`${before}(?:${joinedPatterns})${after}`, 'gi');

    // Replace with a space to avoid merging words together
    let result = text.replace(regex, ' ');

    return result.replace(/\s\s+/g, ' ').trim();
};

export const deleteHabitatDescriptors = (text: string) => {
    if (!text) return '';

    const descriptors = [
    "bunch", "upland", "terrestrial", "rosette", "salt marsh", "spSugden", "including",
    "swamp", "bark", "culms?", "terra firme", "chapparral", "catinga", "shortgrass", "steppe", "plateau",
    "wetland", "cultivated", "vegetables", "mistletoe", "monocot", "valley", "river", "coastal", "mountain",
    "harvest", "residues", "nublados?", "bosques?", "mesophytic", "halophytic", "bamboo", "annual", "perennial",
    "secondary", "primary", "rain", "herbaceous", "conifers?", "coniferous", "broadleaf", "broad-leaved",
    "canopy", "tall", "low", "mata", "field", "forest", "pseudospecies", "leaf", "leaves", "savanna",
    "deciduous", "evergreen", "grassland", "abandoned", "pasture", "meadow", "fine", "broad", "form",
    "forbs?", "ferns?", "epiphytes?", "trees?", "lianas?", "palms?", "graminoids?", "grass(es)?", "shrubs?",
    "sedges?", "Solling"
    ];

    // Join into a single regex pattern
    const joinedDescriptors = descriptors.join('|');

    // Boundary 'lasso': Handles spaces, start/end of string, and hyphens (-)
    const before = '(^|[ \\-])';
    const after = '([ \\-]|$|\\.)';

    const regex = new RegExp(`${before}(?:${joinedDescriptors})${after}`, 'gi');

    let result = text.replace(regex, ' ');

    return result.replace(/\s\s+/g, ' ').trim();
};

export const deleteGeneralNoise = (text: string) => {
    if (!text) return '';

    const badList = [
    "herbs?", "red", "white", "blue", "green", "yellow", "black", "name error",
    "orthodox( p)?", "hiro", "et al\\.", "none", "null", "small", "dark", "smooth"
    ];

    const joined = badList.join('|');
    // Lasso: Start of string or space | word | space or end of string
    const regex = new RegExp(`(^| )(?:${joined})( |$)`, 'gi');

    // We replace with a space to keep word boundaries clean
    let result = text.replace(regex, ' ');
    return result.replace(/\s\s+/g, ' ').trim();
};

export const deleteLeadingDescriptors = (text: string) => {
    if (!text) return '';

    const badList = ["wood", "alpine", "non-\\w{2,}", "pubescent"];
    const joined = badList.join('|');

    // Anchor to the start of the string (^)
    const regex = new RegExp(`^(?:${joined})( |$|-)`, 'i');

    return text.replace(regex, '').trim();
};

export const truncateFromBeginning = (text: string) => {
    if (!text) return '';

    // Matches 'pau' or 'mata/matas/mata' at the very start
    const badList = ["pau", "mat\\w*"];
    const joined = badList.join('|');

    // ^(?:...) matches start, (?: .*)? matches the rest of the string
    const regex = new RegExp(`^(?:${joined})(?: .*|$)`, 'i');

    return text.replace(regex, '').trim();
};

export const truncateFromMarker = (text: string) => {
    if (!text) return '';

    // se. sect. ind. indet. cv. cv
    const badList = ["se(ct)?\\.", "ind(et)?", "cv\\.?"];
    const joined = badList.join('|');

    // (?:^| ) ensures we match the word at start or after space
    // (?: .*|$) captures everything until the end
    const regex = new RegExp(`(?:^| )(?:${joined})(?: .*|$)`, 'i');

    return text.replace(regex, '').trim();
};

export const truncateFromGeographicOrBreeding = (text: string) => {
    if (!text) return '';

    const badList = [
    "caatinga", "boreal", "germany", "north(ern)?", "south(ern)?", "west(ern)?",
    "east(ern)?", "subpolar", "ural", "southafrica", "tropical", "temperate", "cultivar",
    "genotype", "hybride?", "inbred line", "variety"
    ];

    const joined = badList.join('|');
    // Lasso: Start, space, or hyphen | bad words | everything else
    const regex = new RegExp(`(^|[ \\-])(?:${joined})(?: .*|$)`, 'gi');

    return text.replace(regex, '').trim();
};

export const truncateFromUncertainty = (text: string) => {
    if (!text) return '';

    const badList = [
    "group\\w?", "death", "dwarf", "little", "mid", "average", "other", "mixed", "under",
    "all", "dry", "wet", "open", "new", "old", "unk\\.?", "not identified", "unknown", "undetermined",
    "undefined", "unidentified", "unclassified"
    ];

    const joined = badList.join('|');
    // Lasso: Start or space | bad words | everything else
    const regex = new RegExp(`(^| )(?:${joined})(?: .*|$)`, 'gi');

    return text.replace(regex, '').trim();
};

export const changeVernacularNames = (text: string) => {
    if (!text) return '';

    // 1. Handle the "Starts with" cases (Genus swaps)
    const startsWithMap = {
        'Abiu': 'Pouteria',
        'Lily': 'Lilium',
        'Cotton': 'Gossypium',
        'Strawberry': 'Fragaria',
        'Cashew': 'Anacardium'
    };

    let result = text;

    // Check if the string starts with any of our map keys
    for (const [common, scientific] of Object.entries(startsWithMap)) {
        const regex = new RegExp(`^${common}( .*|$)`, 'i');
        if (regex.test(result)) {
            return result.replace(regex, `${scientific}$1`);
        }
    }

    // 2. Handle the "Contains" cases (Specific replacements)
    // Coffee -> Coffea arabica
    result = result.replace(/(^| )coffee( .*|$)/i, '$1Coffea arabica$2');

    // Orchid -> Orchidaceae
    result = result.replace(/(^| )orchid( .*|$)/i, '$1Orchidaceae$2');

    return result.trim();
};

export const updateFamilyNames = (text: string) => {
    if (!text) return '';

    const familyMap = {
        'Compositae': 'Asteraceae',
        'Cruciferae': 'Brassicaceae',
        'Gramineae': 'Poaceae',
        'Guttiferae': 'Clusiaceae',
        'Labiatae': 'Lamiaceae',
        'Leguminosae': 'Fabaceae',
        'Palmae': 'Arecaceae',
        'Umbelliferae': 'Apiaceae'
    };

    let result = text;

    // We loop through the map and use the ^ anchor to match the start of the string
    for (const [oldName, newName] of Object.entries(familyMap)) {
        const regex = new RegExp(`^${oldName}`, 'i');
        if (regex.test(result)) {
            // Replace only the first occurrence at the start
            result = result.replace(regex, newName);
            break; // Once we find a match at the start, we can stop
        }
    }

    return result;
};

export const deleteUselessMarkers = (text: string) => {
    if (!text) return '';

    let result = text;

    // 1. Remove trailing hybrid marker: 'Quercus x' -> 'Quercus'
    result = result.replace(/\s[xX]$/, '');

    // 2. Standardize 'A-' notation
    result = result.replace(/^A-/, ' ');    // At start
    result = result.replace(/\sA-/g, ' x '); // In middle

    // 3. Remove leading lowercase words (Invalid for Genus)
    result = result.replace(/^[a-z]+\s+/, ' ');

    // 4. Remove 3-letter uppercase codes, but PROTECT 'POA'
    // (?!POA\s) is a negative lookahead
    result = result.replace(/^(?!POA\s)[A-Z]{3}\s/, '');

    // 5. Remove 'NA' markers
    result = result.replace(/\sNA(\s|$)/g, ' ');

    // 6. The d'Urville correction
    // Matches 'd', up to 2 chars, 'd', up to 2 chars, 'Urv' and everything after
    result = result.replace(/d.{0,2}d.{0,2}Urv.*/, "d'Urv");

    return result.replace(/\s\s+/g, ' ').trim();
};

export const correctOcrErrors = (text: string) => {
    if (!text) return '';
    // Replaces I with l ONLY if surrounded by lowercase letters
    return text.replace(/(?<=[a-z])I(?=[a-z])/g, 'l');
};

export const harmonizeAbbreviations = (text: string) => {
    if (!text) return '';

    let result = text;

    // 1. Standardize subspecies variations to ' subsp. '
    // Matches s., ssp, sspp, susbp, etc.
    result = result.replace(/(?:\.|\s)?s(ub)?sp(\.)?(?:\s|$|\.)/gi, ' subsp. ');
    result = result.replace(/\s(susbp|subs)(\.|\s)/gi, ' subsp. ');
    result = result.replace(/(\.)?subspecies(\.)?/gi, ' subsp. ');
    result = result.replace(/\s+s\.\s+/g, ' subsp. ');

    // 2. Standardize form variations to ' f. '
    result = result.replace(/fo?(rma)?\.?(\s|$)/gi, ' f. ');

    // 3. Standardize variety to ' var. '
    result = result.replace(/\s+var(\.|\s)/gi, ' var. ');

    // 4. Clean up "stacked" or redundant abbreviations
    result = result.replace(/f\.\s+subsp\./g, 'subsp.');
    result = result.replace(/f\.\s+var\./g, 'var.');

    // 5. Remove abbreviations if they start the string
    result = result.replace(/^ ?(subsp|var|f)(\.)?\s+.*/i, ' ');

    // 6. Remove "trailing" abbreviations with no content after them
    result = result.replace(/\s(subsp|var|f)\.?\s*$/i, '');

    return result.replace(/\s\s+/g, ' ').trim();
};

export const deletePointAfterKey = (text: string) => {
    if (!text) return '';

    // Regex breakdown:
    // ^([A-Z][a-z\-\s]*[a-z])  -> Group 1: The Genus (Starts with Caps)
    // \s+(subsp|var|f)         -> Group 2: The Rank
    // \.                       -> The dot we want to remove
    const regex = /^([A-Z][a-z\-\s]*[a-z])\s+(subsp|var|f)\./;

    // We replace the whole match with Group 1 + space + Group 2 (no dot)
    return text.replace(regex, '$1 $2').trim();
};

export const deletePointAfterSpecies = (text: string) => {
    if (!text) return '';

    // Regex breakdown:
    // ^([A-Z][a-z\-\s]*[a-z])  -> Group 1: Genus
    // \s+                      -> Space
    // ([a-z][a-z\-]*[a-z])     -> Group 2: species epithet
    // \.                       -> The dot we want to remove
    const regex = /^([A-Z][a-z\-\s]*[a-z])\s+([a-z][a-z\-]*[a-z])\./;

    return text.replace(regex, '$1 $2').trim();
};

export const fixMissingSpaces = (text: string) => {
    if (!text) return '';

    let result = text;

    // 1. Remove space before hyphen or period: 'Canis -' -> 'Canis-'
    result = result.replace(/\s-/g, '-');
    result = result.replace(/\s\./g, '.');

    // 2. Ensure a space follows a period, UNLESS it's a closing bracket
    // Example: 'C.lupus' -> 'C. lupus' | '(sp.)' -> '(sp.)' (stays same)
    result = result.replace(/\.(?!\))/g, '. ');

    // 3. Special case for period-hyphen: '. -' -> '.-'
    result = result.replace(/\.\s-/g, '.-');

    // 4. Final Spacing Cleanup
    // Replace multiple spaces with one, then trim edges
    return result.replace(/\s+/g, ' ').trim();
};

export const validateFamilySuffix = (text: string) => {
    if (!text) return '';

    // Condition 2: Does it contain a word ending in 'aceae'?
    const familyMatch = text.match(/[A-Za-z]+aceae/);
    if (!familyMatch) return text;

    const foundWord = familyMatch[0];

    // Condition 3: Is it preceded by a Genus/Species pattern?
    // This regex looks for: Capitalized Genus + word + (optional rank) + our 'aceae' word
    const speciesContextRegex = new RegExp(`[A-Z][a-z\\-]+(?:\\s+[a-z\\-]+)+(?:\\s+[a-z]+\\.)?\\s+${foundWord}`);

    const isSpeciesEpithet = speciesContextRegex.test(text);

    if (!isSpeciesEpithet) {
        // It's a TRUE family name. Capitalize the first letter.
        const capitalized = foundWord.charAt(0).toUpperCase() + foundWord.slice(1);
        return text.replace(foundWord, capitalized).trim();
    } else {
        // It's a FALSE family (a species name). Change suffix to 'cea'.
        // We handle the three specific cases from the Python code
        let corrected = text;
        corrected = corrected.replace(/ceae$/g, 'cea');
        corrected = corrected.replace(/ceae\s/g, 'cea ');
        corrected = corrected.replace(/ceae\)/g, 'cea)');
        return corrected.trim();
    }
};

export const informationInParentheses = (text: string) => {
    if (!text) return "";

    // 1. Remove (lowercase-words-with-hyphens)
    // Equivalent to: r'\([a-z]([a-z]|-){1,}[a-z]\)'
    const lowercaseInfo = /\([a-z]([a-z]|-)+[a-z]\)/g;
    text = text.replace(lowercaseInfo, '');

    // 2. Remove empty or whitespace-only parentheses: ( )
    // Equivalent to: r'\(\s*\)'
    const emptyParens = /\(\s*\)/g;
    text = text.replace(emptyParens, '');

    return text.replace(/\s+/g, ' ').trim();
}

export const correctWritingGenus = (text: string) => {
    if (!text) return "";

    const pattern = /^([A-Z]{2,}((\s(x|X))?\s+|\W|$))+/;
    const match = text.match(pattern);

    if (match) {
    const matchedText = match[0];
    const transformed = matchedText.charAt(0) + matchedText.slice(1).toLowerCase();
    text = transformed + text.slice(matchedText.length);
    }

    return text.replace(/\s+/g, ' ').trim();
};

export const spacesBeforeAndAfterParentheses = (text: string) => {
    if (!text) return "";

    text = text.replace(/\( /g, '(');
    text = text.replace(/ \)/g, ')');

    return text.replace(/\s+/g, ' ').trim();
};

export const correctionHybrid = (text: string) => {
    if (!text) return "";

    if (/^x\s/.test(text)) {
        // Note: hybrid_1 is unused in the original Python snippet's return, 
        // but the regex replacement is performed here.
        text = text.replace(/^x\s/, '');
    }

    return text.replace(/\s+/g, ' ').trim();
};

export const removeAuthors = (text: string) => {
    if (!text) return "";

    const array = text.split(" ");
    const length = array.length;

    // Find the index of the first item starting with '('
    let indexOpen = -1;
    for (let i = 0; i < array.length; i++) {
        if (array[i].startsWith('(')) {
            indexOpen = i;
            break;
        }
    }

    if (indexOpen > 0) {
        const a = array.slice(0, indexOpen);
        return a.join(' ').replace(/\s+/g, ' ').trim();
    }

    if (length > 2) {
        const a = array.slice(0, length - 1);
        return a.join(' ').replace(/\s+/g, ' ').trim();
    } else {
        return text;
    }
};