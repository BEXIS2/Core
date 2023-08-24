import { create, test, enforce, only } from 'vest';
import type { DataTypeListItem } from '../models';

type dataType = {
	dataType: DataTypeListItem;
	dataTypes: DataTypeListItem[];
};

interface regexTestType {
	regex: RegExp;
	output?: string;
	check: boolean;
}

const suite = create((data: dataType, fieldName) => {
	only(fieldName);

	test('name', 'name is required', () => {
		enforce(data.dataType.name).isNotBlank();
	});

	test('name', 'name is not unique', () => {
		return (
			data.dataTypes.find((u) => u.name.toLowerCase() === data.dataType.name.toLowerCase()) ==
				null || data.dataTypes.find((u) => u.name === data.dataType.name)?.id == data.dataType.id
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
