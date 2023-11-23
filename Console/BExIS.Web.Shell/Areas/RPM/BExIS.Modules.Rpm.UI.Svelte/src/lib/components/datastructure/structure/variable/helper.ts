import type { listItemType } from '@bexis2/bexis2-core-ui';
import { displayPatternStore } from '../../store';
import { get } from 'svelte/store';
import type { templateListItemType, unitListItemType } from '../../types';

export function updateDisplayPattern(type, reset = true) {
	// currently only date, date tim e and time is use with display pattern.
	// however the serve only now datetime so we need to preselect the possible display pattern to date, time and datetime
	const allDisplayPattern = get(displayPatternStore);
	let displayPattern: listItemType[];

	if (type != undefined) {
		if (type.text.toLowerCase() === 'date') {
			// date without time
			displayPattern = allDisplayPattern.filter(
				(m) =>
					m.group.toLowerCase().includes(type.text) &&
					(!m.text.toLowerCase().includes('h') || !m.text.toLowerCase().includes('s'))
			);
		} else if (type.text.toLowerCase() === 'time') {
			// time without date
			displayPattern = allDisplayPattern.filter(
				(m) =>
					m.group.toLowerCase().includes(type.text) &&
					(!m.text.toLowerCase().includes('d') || !m.text.toLowerCase().includes('y'))
			);
		} else if (type.text.toLowerCase() === 'datetime') {
			// both
			displayPattern = allDisplayPattern.filter((m) => m.group.toLowerCase().includes(type.text));
		} else {
			displayPattern = [];
		}
	} else {
		displayPattern = [];
	}

	return displayPattern;
}

export function updateGroup(value: string, phrase: string) {
	const othersText = 'other';

	if (value == othersText) {
		return phrase;
	} else {
		if (value.includes(phrase)) {
			return value;
		} else {
			return (value += ' | ' + phrase);
		}
	}
}

export function updateDatatypes(
	unit: unitListItemType | undefined,
	template: templateListItemType | undefined,
	dataTypeStore: listItemType[],
	suggestedDataType: listItemType | undefined,
	units: unitListItemType[]
) {
	const othersText = 'other';
	let dts: listItemType[] = dataTypeStore.map((o) => ({ ...o })); // set datatypes
	//console.log("TCL ~ file: helper.ts:66 ~ dts:", dts)

	//suggestions
	if (suggestedDataType) {
		suggestedDataType.group = 'detect';
		dts = dataTypeStore.filter((d) => d.id != suggestedDataType?.id).map((o) => ({ ...o }));
		dts = [suggestedDataType, ...dts];
	}

	let matchPhrase = '';
	//unit
	if (unit != null && unit != undefined && unit.dataTypes.length > 0) {
		// if unit exist
		matchPhrase = unit?.text;

		for (let index = 0; index < dts.length; index++) {
			const datatype = dts[index];
			if (unit.dataTypes.includes(datatype.text) && !datatype.group.includes(matchPhrase)) {
				datatype.group = updateGroup(datatype.group, matchPhrase);
			}
		}
	}

	// check templates
	if (template && template.units) {
		matchPhrase = template.text;

		for (let index = 0; index < template.units.length; index++) {
			// each unit in a template
			const u = units.filter((u) => u.text == template.units[index])[0];

			for (let index = 0; index < dts.length; index++) {
				// each datatype
				const datatype = dts[index];
				if (u && u.dataTypes.includes(datatype.text) && !datatype.group.includes(matchPhrase)) {
					datatype.group = updateGroup(datatype.group, matchPhrase);
				}
			}
		}
	}

	// reorder
	return [...dts.filter((d) => d.group != othersText), ...dts.filter((d) => d.group == othersText)];
}

export function updateUnits(
	datatype: listItemType | undefined,
	template: templateListItemType | undefined,
	units: unitListItemType[],
	suggestedUnits: unitListItemType[] | undefined
) {
	let _units: unitListItemType[] = units.map((o) => ({ ...o })); // set units

	if (suggestedUnits) {
		// if units are suggested, add them to the list
		_units = units.filter((d) => !suggestedUnits?.some((u) => u.id == d.id)).map((o) => ({ ...o }));
		_units = [...suggestedUnits.map((o) => ({ ...o })), ..._units];
	}

	let matchPhrase = '';
	const othersText = 'other';

	if (datatype && _units) {
		matchPhrase = datatype?.text;
		// if datatype and units exist
		_units.forEach((unit) => {
			if (unit.dataTypes.includes(datatype.text) == true) {
				unit.group = updateGroup(unit.group, matchPhrase);
			}
		});
	}

	// filter units based on template matches
	if (template && template.units) {
		for (let index = 0; index < template.units.length; index++) {
			const u = template.units[index];
			matchPhrase = template?.text;
			_units.forEach((unit) => {
				if (unit.text == u) {
					unit.group = updateGroup(unit.group, matchPhrase);
				}
			});
		}
	}

	return [
		..._units.filter((d) => d.group != othersText),
		..._units.filter((d) => d.group == othersText)
	];
}

export function updateTemplates(
	unit: unitListItemType | undefined,
	templates: templateListItemType[],
	suggestedTemplates: templateListItemType[]
) {
	let _templates: templateListItemType[] = templates.map((o) => ({ ...o }));

	//if suggestedTemplates exist, filter list by
	if (suggestedTemplates) {
		// if suggestions exist please add them to the list
		_templates = templates
			.filter((t) => !suggestedTemplates?.some((u) => u.id == t.id))
			.map((o) => ({ ...o }));

		_templates = [...suggestedTemplates.map((o) => ({ ...o })), ..._templates];
	}

	const matchPhrase = '' + unit?.text;
	const othersText = 'other';

	if (unit && _templates) {
		// if datatype and units exist
		_templates.forEach((template) => {
			if (template.units?.includes(unit.text)) {
				template.group = updateGroup(template.group, matchPhrase);
			}
		});
	}

	return [
		..._templates.filter((d) => d.group != othersText),
		..._templates.filter((d) => d.group == othersText)
	];
}
