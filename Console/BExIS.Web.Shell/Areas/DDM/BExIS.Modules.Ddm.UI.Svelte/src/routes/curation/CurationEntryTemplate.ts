import { writable } from 'svelte/store';
import type { CurationEntryCreationModel } from './types';
import { DefaultCurationEntryCreationModel } from './types';

export const entryTemplateRegex = /\[TemplateEntry\]\(\?([^)]*)\)/g;

export interface CurationEntryTemplateModel extends CurationEntryCreationModel {
	placement: 'top' | 'bottom';
	createAsDraft: boolean;
	autoCreate: boolean;
	scrollToEntry: boolean;
}

export const entryTemplatePopupState = writable<{
	show: boolean;
	template?: CurationEntryTemplateModel;
	callback?: (newTemplate: CurationEntryTemplateModel) => void;
}>({
	show: false
});

export const DefaultCurationEntryTemplate: CurationEntryTemplateModel = {
	...DefaultCurationEntryCreationModel,
	placement: 'bottom',
	createAsDraft: false,
	autoCreate: false,
	scrollToEntry: false
}; // booleans should default to false as presence means true

const nameMapping: Record<string, string> = {
	createAsDraft: 'draft',
	autoCreate: 'auto',
	scrollToEntry: 'scroll'
};

const mapName = (field: string) => {
	if (field in nameMapping) {
		return nameMapping[field];
	}
	return field;
};

const fields: (keyof CurationEntryTemplateModel)[] = [
	'topic',
	'type',
	'name',
	'description',
	'solution',
	'source',
	'comment',
	'status',
	'placement',
	'createAsDraft',
	'autoCreate'
];

function getCreationModelParams(entryCreation: CurationEntryTemplateModel) {
	const params: Record<string, string | null> = {};

	for (const field of fields) {
		const value = entryCreation[field];
		const defaultValue = DefaultCurationEntryTemplate[field];
		if (value !== undefined && value !== defaultValue) {
			if (typeof value === 'boolean') {
				params[mapName(field)] = null;
			} else if (typeof value === 'string') {
				params[mapName(field)] = encodeURIComponent(value);
			} else {
				params[mapName(field)] = encodeURIComponent(value?.toString() ?? '');
			}
		}
	}

	return params;
}

export function getTemplateLinkText(curationEntryTemplate: CurationEntryTemplateModel) {
	const params = getCreationModelParams(curationEntryTemplate);
	const paramString = Object.entries(params)
		.map(([k, v]) => (v === null ? k : `${k}=${v}`))
		.join('&');
	return `[TemplateEntry](?${paramString})`;
}

export function parseTemplateLink(link: string) {
	const regex = /\[TemplateEntry\]\(\?([^)]*)\)/;
	const match = RegExp(regex).exec(link);
	if (!match) {
		throw new Error('Invalid template link format');
	}
	const paramString = match[1];
	const params: Record<string, string | null> = {};
	for (const pair of paramString.split('&')) {
		if (pair.includes('=')) {
			const [key, value] = pair.split('=');
			if (key && value) params[key] = decodeURIComponent(value);
		} else if (pair) {
			params[pair] = null;
		}
	}

	// Map params back to model fields
	for (const [key, mappedKey] of Object.entries(nameMapping)) {
		if (params[mappedKey] !== undefined) {
			params[key] = params[mappedKey];
			delete params[mappedKey];
		}
	}

	const template: CurationEntryTemplateModel = { ...DefaultCurationEntryTemplate };

	Object.entries(params).forEach(([key, value]) => {
		const field = key as keyof CurationEntryTemplateModel;
		if (
			value !== null &&
			value !== undefined &&
			typeof DefaultCurationEntryTemplate[field] === 'string'
		) {
			(template as any)[field] = value;
		} else if (
			value !== null &&
			value !== undefined &&
			typeof DefaultCurationEntryTemplate[field] === 'number'
		) {
			if (field === 'type') {
				const intValue = parseInt(value);
				if (!isNaN(intValue)) {
					template.type = intValue;
				}
			} else if (field === 'status') {
				const intValue = parseInt(value);
				if (!isNaN(intValue)) {
					template.status = intValue;
				}
			}
		} else if (value === null && typeof DefaultCurationEntryTemplate[field] === 'boolean') {
			(template as any)[field] = true;
		}
	});

	return template;
}
