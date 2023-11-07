import { create, test, enforce, only } from 'vest';
import type { ConstraintListItem } from '../models';

type dataType = {
	constraint: ConstraintListItem;
	constraints: ConstraintListItem[];
};

const suite = create((data: dataType, fieldName) => {
	only(fieldName);

	test('name', 'name is required', () => {
		enforce(data.constraint.name).isNotBlank();
	});

	test('name', 'name is not unique', () => {
		return (
			data.constraints.find((u) => u.name.toLowerCase() === data.constraint.name.toLowerCase()) ==
				null || data.constraints.find((u) => u.name === data.constraint.name)?.id == data.constraint.id
		);
	});

	test('description', 'description is required', () => {
		enforce(data.constraint.description).isNotBlank();
	});

	test('description', 'description is to short, it must be larger then 10 chars', () => {
		enforce(data.constraint.description).longerThan(10);
	});
});

export default suite;
