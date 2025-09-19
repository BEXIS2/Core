import type { CurationEntryCreationModel } from './types';
import { CurationEntryStatus, CurationEntryType } from './types';

export const entryTemplateRegex = /\[TemplateEntry\]\(\?([^)]*)\)/g;

export function getTemplateLinkText(
	inputData: Partial<CurationEntryCreationModel>,
	position?: 'top' | 'bottom',
	createAsDraft?: boolean,
	autoCreate?: boolean
) {
	const params: Record<string, string> = {};
	for (const [key, value] of Object.entries(inputData)) {
		if (
			value !== undefined &&
			value !== null &&
			!(typeof value === 'string' && value.trim() === '')
		) {
			params[key] = encodeURIComponent(value.toString());
		}
	}
	if (position) params['position'] = position;
	if (createAsDraft) params['draft'] = '';
	if (autoCreate) params['auto'] = '';
	const paramString = Object.entries(params)
		.map(([k, v]) => (v === '' ? k : `${k}=${v}`))
		.join('&');
	return `[TemplateEntry](?${paramString})`;
}

function mapPosition(position: string): 'top' | 'bottom' {
	if (position === 'top' || position === 'bottom') {
		return position;
	} else if (position === '0') {
		return 'top';
	} else {
		return 'bottom';
	}
}

function removeEmptyStringValues(obj: Record<string, any>) {
	for (const key in obj) {
		if (typeof obj[key] === 'string' && obj[key].trim() === '') {
			delete obj[key];
		}
	}
}

function mapParamsToExpectedTypes(params: Record<string, string>) {
	const result: any = { ...params };
	if ('type' in params && params.type !== '') {
		const typeValue = parseInt(params.type, 10);
		// Check if typeValue is a valid CurationEntryType
		if (
			Object.values(CurationEntryType)
				.filter((v) => typeof v === 'number')
				.includes(typeValue)
		) {
			result.type = typeValue;
		} else {
			throw new Error(`Invalid type value: ${params.type}`);
		}
	}
	if ('status' in params && params.status !== '') {
		const statusValue = parseInt(params.status, 10);
		// Check if statusValue is a valid CurationEntryStatus
		if (
			Object.values(CurationEntryStatus)
				.filter((v) => typeof v === 'number')
				.includes(statusValue)
		) {
			result.status = statusValue;
		} else {
			throw new Error(`Invalid status value: ${params.status}`);
		}
	}
	if ('position' in params && params.position !== '') {
		result.position = mapPosition(params.position);
	}
	// Boolean flags: presence means true, absence means false
	if ('draft' in params) {
		result.createAsDraft = true;
	}
	if ('auto' in params) {
		result.autoCreate = true;
	}
	removeEmptyStringValues(result);
	return result as Partial<CurationEntryCreationModel> & {
		position?: 'top' | 'bottom';
		createAsDraft?: boolean;
		autoCreate?: boolean;
	};
}

export function parseTemplateLink(link: string) {
	const regex = /\[TemplateEntry\]\(\?([^)]*)\)/;
	const match = RegExp(regex).exec(link);
	if (!match) {
		throw new Error('Invalid template link format');
	}
	const paramString = match[1];
	const params: Record<string, string> = {};
	for (const pair of paramString.split('&')) {
		if (pair.includes('=')) {
			const [key, value] = pair.split('=');
			if (key) params[key] = value ? decodeURIComponent(value) : '';
		} else if (pair) {
			// flag with no value
			params[pair] = '';
		}
	}
	return mapParamsToExpectedTypes(params);
}
