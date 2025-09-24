import { writable, get } from 'svelte/store';
import type { CurationEntryCreationModel } from './types';
import { CurationEntryType, DefaultCurationEntryCreationModel } from './types';
import { curationStore } from './stores';

export const entryTemplateRegex = /\[TemplateEntry\]\(\?([^)]*)\)/g;

export interface CurationEntryTemplateModel extends CurationEntryCreationModel {
	placement: 'top' | 'bottom';
	createAsDraft: boolean;
	autoCreate: boolean;
	scrollToEntry: boolean;
}

export const entryTemplatePopupState = writable<{
	show: boolean;
	template?: Partial<CurationEntryTemplateModel>;
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

function getCreationModelParams(entryCreation: Partial<CurationEntryTemplateModel>) {
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

export function getTemplateLinkText(curationEntryTemplate: Partial<CurationEntryTemplateModel>) {
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

export function createEntryFromTemplate(
	template: CurationEntryTemplateModel,
	scrollToEntry = true
): Promise<number | void> {
	const type = template.type ?? DefaultCurationEntryCreationModel.type;
	const highestPosition = get(curationStore.curation)?.highestPositionPerType?.[type];
	let position: number;
	if (template.placement === 'top' || highestPosition === undefined) {
		position = 1;
	} else {
		position = highestPosition + 1;
	}
	const entryModel = {
		...template,
		position
	};
	return curationStore.addEmptyEntry(
		entryModel,
		false,
		template.createAsDraft ?? false,
		scrollToEntry
	);
}

export const entriesFromTemplatesProgress = writable(-1);

entriesFromTemplatesProgress.subscribe((value) => {
	if (value === 100) {
		setTimeout(() => entriesFromTemplatesProgress.set(-1), 500);
	}
});

export function createEntriesFromTemplates(
	templates: CurationEntryTemplateModel[],
	asDraft = true
): Promise<void> {
	entriesFromTemplatesProgress.set(0);
	const total = templates.length;
	let completed = 0;
	const filteredTemplates = templates.filter((t) => t.type !== CurationEntryType.StatusEntryItem);
	const run = async () => {
		for (const [i, template] of filteredTemplates.entries()) {
			await createEntryFromTemplate(
				{ ...template, createAsDraft: asDraft },
				i === filteredTemplates.length - 1
			);
			completed++;
			entriesFromTemplatesProgress.set((completed / total) * 100);
		}
		entriesFromTemplatesProgress.set(1);
	};
	return run();
}

export function getAllAutoTemplates(markdown: string) {
	const matches = markdown.matchAll(entryTemplateRegex);
	const templates: CurationEntryTemplateModel[] = [];
	for (const match of matches) {
		const template = parseTemplateLink(match[0]);
		if (template.autoCreate) {
			templates.push(template);
		}
	}
	return templates;
}
