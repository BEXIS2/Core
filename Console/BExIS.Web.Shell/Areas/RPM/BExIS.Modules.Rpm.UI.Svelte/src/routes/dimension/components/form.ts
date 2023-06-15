import { create, test, enforce, only } from 'vest';
import type { DimensionListItem } from "../models"; 

type dataType = {
	dimension:DimensionListItem,
	dimensions: DimensionListItem[]
}

const suite = create((data:dataType, fieldName) => {
	only(fieldName);

	test('name', 'name is required', () => {
		enforce(data.dimension.name).isNotBlank();
	});

	test('name', 'name is not unique', () => {
		return data.dimensions.find(u => u.name.toLowerCase() === data.dimension.name.toLowerCase())== null || data.dimensions.find(u => u.name === data.dimension.name)?.id == data.dimension.id;
	});

	test('description', 'description is required', () => {
		enforce(data.dimension.description).isNotBlank();
	});

	test('description', 'description is to short, it must be larger then 10 chars', () => {
		enforce(data.dimension.description).longerThan(10);
	});

	test('specification', 'specification is required', () => {
		enforce(data.dimension.specification).isNotBlank();
	});
});

export default suite;