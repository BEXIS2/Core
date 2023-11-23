import { create, test, enforce, only } from 'vest';
import { externalLinksStore } from './stores';
import { get } from 'svelte/store';

const suite = create((data = {}, fieldName) => {
	only(fieldName);

	test('link_name', 'name is required', () => {
		enforce(data.name).isNotBlank();
	});

	test('link_name', 'name allready exist.', () => {
		const existingLinks = get(externalLinksStore).map((e) => e.name);
		// if the form is in edit mode, find the selected one by id
		const editedObj =
			data.id > 0 ? get(externalLinksStore).find((e) => e.id == data.id) : { id: 0, name: '' };

		//console.log(editedObj.name);
		// get all names back, without the edited one
		const list = editedObj ? existingLinks.filter((l) => l != editedObj.name) : existingLinks;

		enforce(data.name).notInside(list);
	});

	test('link_uri', 'uri is required', () => {
		enforce(data.uri).isNotBlank();
	});

	test('link_uri', 'uri allready exist.', () => {
		// if the for is in edit mode, find the selected one by id
		const editedObj =
			data.id > 0
				? get(externalLinksStore).find((e) => e.id == data.id)
				: { id: 0, name: '', type: '', uri: '' };
		//console.log(editedObj.name);
		// get all names back, without the edited one
		const list = get(externalLinksStore).map((e) => (e.uri != editedObj?.uri ? e.uri : ''));

		enforce(data.uri).notInside(list);
	});
});

export default suite;
