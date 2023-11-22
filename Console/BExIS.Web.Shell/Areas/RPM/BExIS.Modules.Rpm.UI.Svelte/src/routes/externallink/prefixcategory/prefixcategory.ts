import { create, test, enforce, only } from 'vest';
import { prefixCategoryStore } from '$lib/components/meaning/stores';
import { get } from 'svelte/store';

const suite = create((data = {},fieldName) => {

 only(fieldName);

	test('name', 'name is required', () => {
		enforce(data.name).isNotBlank();
	});

	test('name', 'name allready exist.', () => {

		const prefixCategories = get(prefixCategoryStore).map(e=>e.name);
		// if the form is in edit mode, find the selected one by id
		const editedObj =
		data.id > 0 ? get(prefixCategoryStore).find((e) => e.id == data.id) : { id: 0, name: '' };

		//console.log(editedObj.name);
		// get all names back, without the edited one
		 const list = editedObj?prefixCategories.filter(l=>l!= editedObj.name):prefixCategories;

		 enforce(data.name).notInside(list);
	});


});

export default suite;