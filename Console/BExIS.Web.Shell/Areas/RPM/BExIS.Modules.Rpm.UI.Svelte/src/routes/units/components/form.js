import { create, test, enforce, only } from 'vest';

const suite = create((data = {}, fieldName) => {
	only(fieldName);

	test('name', 'name is required', () => {
		enforce(data.name).isNotBlank();
	});

	test('description', 'description is required', () => {
		enforce(data.description).isNotBlank();
	});

	test('description', 'description is to short, it must be larger then 160 chars', () => {
		enforce(data.description).longerThan(160);
	});
});

export default suite;