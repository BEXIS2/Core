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
			data.constraints.find(
				(u) => u.name.toLowerCase().trim() === data.constraint.name.toLowerCase().trim()
			) == null ||
			data.constraints.filter(
				(u) =>
					u.name.toLowerCase().trim() === data.constraint.name.toLowerCase().trim() &&
					u.id != data.constraint.id
			).length == 0
		);
	});

	test('description', 'description is required', () => {
		enforce(data.constraint.description).isNotBlank();
	});
});

export default suite;
