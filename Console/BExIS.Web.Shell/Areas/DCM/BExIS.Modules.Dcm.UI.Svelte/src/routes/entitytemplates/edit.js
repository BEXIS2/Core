import { create, test, enforce, only } from 'vest';
import { get } from 'svelte/store';
import { entityTemplatesStore } from './store';

const suite = create((data = {}, fieldName) => {
	only(fieldName);

	test('name', 'name is required', () => {
		enforce(data.name).isNotBlank();
	});

	test('name', 'name allready exist', () => {
		// if the for is in edit mode, find the selected one by id
		const editedObj =
			data.id > 0 ? get(entityTemplatesStore).find((e) => e.id == data.id) : { id: 0, name: '' };
		console.log(editedObj.name);
		// get all names back, without the edited one
		const listOfEntityTemplates = get(entityTemplatesStore).map((e) =>
			e.name != editedObj.name ? e.name : ''
		);

		return enforce(data.name).notInside(listOfEntityTemplates);
	});

	test('description', 'description is required', () => {
		enforce(data.description).isNotBlank();
	});

	test('metadataStructure', 'metadatastructure is required', () => {
		enforce(data.metadataStructure).isNotNull();
		enforce(data.metadataStructure).isNotUndefined();
	});

	test('entityType', 'entity is required', () => {
		enforce(data.entityType).isNotNull();
		enforce(data.entityType).isNotUndefined();
	});
});

export default suite;
