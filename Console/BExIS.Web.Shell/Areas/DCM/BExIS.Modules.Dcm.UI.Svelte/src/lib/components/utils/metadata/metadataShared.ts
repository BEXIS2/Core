// converted display name
export function convertDisplayName(name: string, header: boolean = false): string {
    let displayName = name.replace(/([a-z])([A-Z])/g, '$1 $2');
    let wordList = displayName.split(' ');

    if (header) {
        // if header, capitalize all words except for fully uppercase words (e.g., DOI, URL)
        for (let i = 0; i < wordList.length; i++) {
            if (wordList[i] !== wordList[i].toUpperCase()) {
                wordList[i] = wordList[i].charAt(0).toUpperCase() + wordList[i].slice(1).toLowerCase();
            }
        }
    }
    else {
        for (let i = 0; i < wordList.length; i++) {
            if (wordList[i] !== wordList[i].toUpperCase()) {
                wordList[i] = wordList[i].toLowerCase();
            }
        }
    }
    
    displayName = wordList.join(' ');
    displayName = displayName.charAt(0).toUpperCase() + displayName.slice(1).replace(/_/g, ' ');

    // add hardcoded exception rules here for specific terms (also replaces if part of label)
    const specialTerms: { [key: string]: string } = {
        'e mail': 'E-Mail',
        'ip address': 'IP Address',
        'url': 'URL',
        'uuid': 'UUID',
        'id': 'ID',
        'doi': 'DOI',
        'isbn': 'ISBN',
        'issn': 'ISSN',
        'uri': 'URI',
        'ph': 'pH',
        'elabftw': 'eLabFTW',
        'ror': 'ROR',
        'orcid': 'ORCiD'
    };

    for (const [lowerTerm, correctTerm] of Object.entries(specialTerms)) {
        const regex = new RegExp(`\\b${lowerTerm}\\b`, 'gi');
        displayName = displayName.replace(regex, correctTerm);
    }



    return displayName;
}