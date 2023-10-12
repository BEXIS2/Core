import { create, test, enforce, only } from 'vest';
import { meaningsStore } from './stores';
import { get } from 'svelte/store';

const suite = create((data = {},fieldName) => {

 only(fieldName);

 test('name', 'name is required', () => {
		enforce(data.name).isNotBlank();
	});

 test('name', 'name allready exist', () => {
		// if the for is in edit mode, find the selected one by id
		const editedObj =
			data.id > 0 ? get(meaningsStore).find((e) => e.id == data.id) : { id: 0, name: '' };
		//console.log(editedObj.name);
		// get all names back, without the edited one
		 const list = get(meaningsStore).map((e) => (e.name != editedObj?.name ? e.name : ''));

		 enforce(data.name).notInside(list);
	});

});

export default suite;