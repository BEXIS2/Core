import mappingResource from './mappingResource.json';

export async function applyMappingToMetadata(metadata: any, row: any, mapping: any[]) {
    for (const map of mapping) {
        const source = map.Source;
        const rawTargetPath = map.Target;
        const value = row[source];
        if (value === undefined || value === null) continue;

        // "$." entfernen → nur die Pfadteile
        const cleanPath = rawTargetPath.replace(/^\$\./, '');
        const pathParts = cleanPath.split('.');

        let current = metadata;
        let parent = null;
        let key = null;

        for (let i = 0; i < pathParts.length; i++) {
            const part = pathParts[i];

            if (i === pathParts.length - 1) {
                parent = current;
                key = part;
            } else {
                if (!(part in current) || typeof current[part] !== 'object') {
                    current[part] = {};
                }
                current = current[part];
            }
        }

        // Zielstruktur prüfen
        const targetStructure = parent?.[key];
        const isArray =
            Array.isArray(targetStructure) ||
            rawTargetPath.endsWith('[]') ||
            Array.isArray(row[source]);

        let values: string[] = [];

        // Wenn es ein Array ist oder ein String mit Trennzeichen
        if (Array.isArray(value)) {
            values = value;
        } else if (typeof value === 'string' && value.includes(';')) {
            values = value.split(';').map((v) => v.trim());
        } else {
            values = [value];
        }

        if (isArray) {
            parent[key] = values.map((v) => ({ '#text': v }));
        } else {
            parent[key] = { '#text': values[0] };
        }
    }
    let Resource: any = [];
    Resource = applyResourceMapping(metadata, row, mappingResource.Mappings);
    // console.log('Resource', Resource);
}

async function applyResourceMapping(metadata: any, row: any, mapping: any[]) {
    const resourceTemplate = metadata.Resource?.[0] || {};
    const filledResources = [];

    function setValueAtPath(obj: any, rawPath: string, value: any) {
        let cleanPath = rawPath;
        if (rawPath.startsWith('$.Resource.')) {
            cleanPath = rawPath.replace(/^\$\.Resource\./, '');
        } else if (rawPath.startsWith('$.')) {
            cleanPath = rawPath.replace(/^\$\./, '');
        }

        const pathParts = cleanPath.split('.');
        let current = obj;

        for (let i = 0; i < pathParts.length - 1; i++) {
            const part = pathParts[i];
            // Suche Property case-insensitiv
            const realKey =
                Object.keys(current).find((k) => k.toLowerCase() === part.toLowerCase()) || part;

            if (Array.isArray(current[realKey])) {
                current[realKey].forEach((item) => {
                    setValueAtPath(item, pathParts.slice(i + 1).join('.'), value);
                });
                return;
            }

            if (!(realKey in current) || typeof current[realKey] !== 'object') {
                current[realKey] = {};
            }
            current = current[realKey];
        }

        const finalPart = pathParts[pathParts.length - 1];
        const realFinalKey =
            Object.keys(current).find((k) => k.toLowerCase() === finalPart.toLowerCase()) || finalPart;

        // Dynamische Array-Erkennung
        let values: string[] = [];
        if (Array.isArray(value)) {
            values = value;
        } else if (typeof value === 'string' && value.includes(';')) {
            values = value.split(';').map((v) => v.trim());
        } else {
            values = [value];
        }

        const isArray =
            rawPath.endsWith('[]') || Array.isArray(value) || Array.isArray(current[realFinalKey]);

        if (isArray) {
            current[realFinalKey] = values.map((v) => ({ '#text': v }));
        } else {
            current[realFinalKey] = { '#text': values[0] };
        }
    }
    
    function deepCopy(obj: any) {
        return JSON.parse(JSON.stringify(obj));
    }

    function splitAndTrim(value: string | undefined): string[] {
        return (value || '')
            .split(',')
            .map((s) => s.trim())
            .filter(Boolean);
    }

    // Schritt 1: Allgemeine Metadaten (nicht Resource-spezifisch)
    for (const map of mapping) {
        const source = map.Source;
        const rawTargetPath = map.Target;
        const value = row[source];
        if (value === undefined || value === null) continue;

        // Prüfe, ob das Mapping auf alle Resource-Objekte angewendet werden soll
        if (rawTargetPath.startsWith('$.Resource.')) {
            if (Array.isArray(metadata.Resource)) {
                metadata.Resource.forEach((resourceObj: any) => {
                    setValueAtPath(resourceObj, rawTargetPath.replace('$.Resource.', ''), value);
                });
            }
            continue;
        }

        // Sonst wie gehabt
        setValueAtPath(metadata, rawTargetPath, value);
    }

    // Schritt 2: Ressourcen (Data und Code)
    const resourceTypes = ['Data', 'Code'];

    for (const type of resourceTypes) {
        // Hole alle Mapping-Einträge für diesen Typ
        const groupMappings = mapping.filter((m) => m.Source.startsWith(type));

        // Extrahiere die Werte für alle Spalten dieses Typs
        const groupedValues: Record<string, string[]> = Object.fromEntries(
            groupMappings.map((m) => [m.Source, splitAndTrim(row[m.Source])])
        );

        const urls = groupedValues[`${type} URL`] || [];
        const typeText = type === 'Data' ? 'Dataset' : 'Software';

        for (let i = 0; i < urls.length; i++) {
            const newResource = deepCopy(resourceTemplate);
            // newResource.Name = urls[i];

            // Setze den Ressourcentyp
            if (
                newResource.Resources_Type &&
                typeof newResource.Resources_Type === 'object' &&
                '#text' in newResource.Resources_Type
            ) {
                newResource.Resources_Type['#text'] = typeText;
            } else {
                newResource.Resources_Type = { '#text': typeText };
            }

            // Fülle alle anderen Felder gemäß Mapping
            for (const map of groupMappings) {
                const valueList = groupedValues[map.Source];
                if (!valueList || !valueList[i]) continue;
                setValueAtPath(newResource, map.Target, valueList[i]);
            }
            console.log('newResource', newResource);
            filledResources.push(newResource);
        }
    }
    console.log('filled', filledResources);
    metadata.Resource = filledResources;

    return filledResources;
}