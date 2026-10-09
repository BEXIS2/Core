import { create, test, enforce, only } from 'vest';
import { get } from 'svelte/store';
import { usersStore } from '../../routes/users/services';

const createUserValidation = create((data = {}, fieldName) => {
	only(fieldName);

	test('userName', 'userName is required', () => {
		enforce(data.userName).isNotBlank();
	});

	test('userName', 'userName already exist', () => {
		// if the for is in edit mode, find the selected one by id
		const list = get(usersStore)
			.filter((e) => e.id != data.id)
			.map((e) => e.userName);
		enforce(data.userName).notInside(list);

		return enforce(data.userName).notInside(usersStore);
	});

	test('email', 'email is required', () => {
		enforce(data.email).isNotBlank();
	});

	test('email', 'email is invalid', () => {
		enforce(data.email).matches(/^\S+@\S+\.\S+$/);
	});

	test('email', 'email already exist', () => {
		// if the for is in edit mode, find the selected one by id
		const list = get(usersStore)
			.filter((e) => e.id != data.id)
			.map((e) => e.email);
		enforce(data.email).notInside(list);

		return enforce(data.email).notInside(usersStore);
	});
});

export default createUserValidation;
