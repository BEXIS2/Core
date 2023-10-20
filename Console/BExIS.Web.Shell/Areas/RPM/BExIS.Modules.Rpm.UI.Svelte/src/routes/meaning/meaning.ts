import { create, test, enforce, only } from 'vest';
import { meaningsStore } from '$lib/components/meaning/stores';
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


	// test('link_name', 'name allready exist, please select the link from the list below.', () => {
	// 	// if the for is in edit mode, find the selected one by id
	// 	const editedObj =
	// 		data.id > 0 ? get(externalLinksStore).find((e) => e.id == data.id) : { id: 0, name: '' };
	// 	//console.log(editedObj.name);
	// 	// get all names back, without the edited one
	// 	 const list = get(externalLinksStore).map((e) => (e.name != editedObj?.name ? e.name : ''));

	// 	 enforce(data.name).notInside(list);
	// });

	// test('link_uri', 'uri allready exist, please select the link from the list below.', () => {
	// 	// if the for is in edit mode, find the selected one by id
	// 	const editedObj =
	// 		data.id > 0 ? get(externalLinksStore).find((e) => e.id == data.id) : { id: 0, name: '' };
	// 	//console.log(editedObj.name);
	// 	// get all names back, without the edited one
	// 	 const list = get(externalLinksStore).map((e) => (e.name != editedObj?.name ? e.name : ''));

	// 	 enforce(data.name).notInside(list);
	// });

});

export default suite;