import { create, test, enforce, only } from 'vest';
import type { DataTypeListItem } from '../models';

type dataType = {
	dataType: DataTypeListItem;
	dataTypes: DataTypeListItem[];
};

const suite = create((data: dataType, fieldName) => {
	only(fieldName);

	test('name', 'name is required', () => {
		enforce(data.dataType.name).isNotBlank();
	});

	test('name', 'name is not unique', () => {
		return (
			data.dataTypes.find(
				(u) => u.name.toLowerCase().trim() === data.dataType.name.toLowerCase().trim()
			) == null ||
			data.dataTypes.filter(
				(u) =>
					u.name.toLowerCase().trim() === data.dataType.name.toLowerCase().trim() &&
					u.id != data.dataType.id
			).length == 0
		);
	});

	test('description', 'description is required', () => {
		enforce(data.dataType.description).isNotBlank();
	});

	// test('systemType', 'System Type is required', () => {
	// 	enforce(data.dataType.systemType).isNotBlank();
	// });
});

export default suite;
