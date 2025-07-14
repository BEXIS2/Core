import { metadataStore } from './stores';

export function getValueByPath(path) {
    let obj: any;
    metadataStore.subscribe(v => {obj = v;});
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
}

export function setValueByPath(obj, path, value) {
    const parts = path.split('.');
    let current = obj;
    for (let i = 0; i < parts.length - 1; i++) {
        if (!(parts[i] in current) || typeof current[parts[i]] !== 'object') {
            current[parts[i]] = {};
        }
        current = current[parts[i]];
    }
    current[parts[parts.length - 1]] = value;
    return obj;
}

export function updateMetadataStore(path,value) {
    let obj: any;
    metadataStore.subscribe(v => {obj = v;})
	{
		if (value !== undefined && value !== null && value !== getValueByPath(path + '.#text')) {
			obj = setValueByPath(obj, path + '.#text', value);
			if(obj && obj !== undefined && obj !== null && obj !== metadataStore.subscribe(value => {obj = value;})) {
				metadataStore.set(obj);
			}
		}
	}
}