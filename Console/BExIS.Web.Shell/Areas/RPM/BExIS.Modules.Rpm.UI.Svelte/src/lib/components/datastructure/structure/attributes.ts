import { create, test, enforce, only } from 'vest';
import { get } from 'svelte/store';
import { structureStore } from '../store';

const suite = create((data = {}, fieldName) => {
	//only(fieldName);

	test('title', 'title is required', () => {
		enforce(data.title).isNotBlank();
	});

	test('title', 'title allready exist', () => {

		const listOfStructures = get(structureStore).map((e) => e.text);
		const editedObj =
		data.id > 0 ? get(structureStore).find((e) => e.id == data.id) : { id: 0, text: '' };

		const list = editedObj?listOfStructures.filter(l=>l!= editedObj.text):listOfStructures;

		enforce(data.title).notInside(list);

	});

	test('description', 'description is too long. you only have 255 characters.', () => {
		enforce(data.description).shorterThan(255);
	});
});

export default suite;
