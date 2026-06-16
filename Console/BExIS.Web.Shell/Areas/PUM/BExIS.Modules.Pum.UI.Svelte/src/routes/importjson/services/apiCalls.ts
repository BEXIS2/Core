const CROSSREF_BASE = 'https://api.crossref.org';
import { Api } from '@bexis2/bexis2-core-ui';
import type { datasetType } from '../models';

export async function searchCrossref(query: string, rows = 20): Promise<any> {
    try {
        const url = `${CROSSREF_BASE}/works?query=${encodeURIComponent(query)}&rows=${rows}`;
        const res = await fetch(url, { headers: { 'User-Agent': 'BExIS-PMM/1.0 (mailto:your-email@example.com)' } });
        if (!res.ok) throw new Error(`Crossref search failed: ${res.status}`);
        return await res.json();
    } catch (err) {
        console.error('searchCrossref error', err);
        throw err;
    }
}

export async function getWorkByDOI(doi: string): Promise<any> {
    try {
        const url = `${CROSSREF_BASE}/works/${encodeURIComponent(doi)}`;
        const res = await fetch(url, { headers: { 'User-Agent': 'BExIS-PMM/1.0 (mailto:your-email@example.com)' } });
        if (!res.ok) throw new Error(`Crossref getWork failed: ${res.status}`);
        return await res.json();
    } catch (err) {
        console.error('getWorkByDOI error', err);
        throw err;
    }
}

export const getEntityTemplateList = async () => {
    try {
        const response = await Api.get('/dcm/entitytemplates/Load');
        console.log('response', response.data);
        return response.data;
    } catch (error) {
        console.error(error);
        throw error;
    }
};

export const createDataset = async (dataset: datasetType) => {
    try {
        const response = await Api.post('/api/Dataset', dataset);
        // console.log('Dataset created:', response);
        return response.data;
    } catch (error) {
        console.error('Fehler beim Erstellen des Datasets:', error);
        throw error;
    }
};

export const GetMetadata = async (id: number) => {
	try {
		const response = await Api.get('/api/Metadata/' + id + '?simplifiedJson=1');
		// console.log('response', response.data);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const putMetadata = async (id: number, value: any) => {
	try {
		// console.log(" value:", value);

		const response = await Api.put('/api/Metadata/' + id, value);
		// console.log('Dataset filled:', response);
		return response.data;
	} catch (error) {
		console.error('Fehler beim füllen vom dataset:', error);
		throw error;
	}
};

