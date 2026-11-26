import { create, test, enforce, only } from 'vest';
import { get } from 'svelte/store';
import { groupsStore } from '../../routes/groups/services';

const updateGroupValidation = create((data = {}, fieldName) => {
	only(fieldName);

	test('name', 'name is required', () => {
		enforce(data.name).isNotBlank();
	});

test('name', 'name allready exist', () => {
		// if the for is in edit mode, find the selected one by id
		const list = get(groupsStore)
			.filter((e) => e.id != data.id)
			.map((e) => e.name);
		enforce(data.name).notInside(list);

		return enforce(data.name).notInside(groupsStore);
	});

	test('description', 'description is required', () => {
		enforce(data.description).isNotBlank();
	});
});

export default updateGroupValidation;
