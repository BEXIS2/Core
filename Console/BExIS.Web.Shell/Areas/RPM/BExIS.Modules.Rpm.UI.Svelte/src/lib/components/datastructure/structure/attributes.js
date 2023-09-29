import { create, test, enforce, only } from 'vest';
import { get } from 'svelte/store';
import { structureStore } from '../store';

const suite = create((data = {}, fieldName) => {
	only(fieldName);

	test('title', 'title is required', () => {
		enforce(data.title).isNotBlank();
	});

	test('title', 'title allready exist', () => {
		const listOfStructures = get(structureStore).map((e) => e.text);

		//console.log('title exist check', data.title, listOfStructures);

		return enforce(data.title).notInside(listOfStructures);
	});

	// test('description', 'description is required', () => {
	// 	enforce(data.description).isNotBlank();
	// });

	test('description', 'description is too long. you only have 255 characters.', () => {
		enforce(data.description).shorterThan(255);
	});
});

export default suite;
