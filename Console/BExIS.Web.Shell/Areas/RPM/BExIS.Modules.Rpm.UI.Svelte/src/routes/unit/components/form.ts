import { create, test, enforce, only } from 'vest';
import type { UnitListItem } from '../models';

type dataType = {
	unit: UnitListItem;
	units: UnitListItem[];
};

const suite = create((data: dataType, fieldName) => {
	only(fieldName);

	test('name', 'name is required', () => {
		enforce(data.unit.name).isNotBlank();
	});

	test('name', 'name is not unique', () => {
		return (
			data.units.find((u) => u.name.toLowerCase() === data.unit.name.toLowerCase()) == null ||
			data.units.find((u) => u.name === data.unit.name)?.id == data.unit.id
		);
	});

	test('abbreviation', 'abbreviation is required', () => {
		enforce(data.unit.abbreviation).isNotBlank();
	});

	test('abbreviation', 'abbreviation is not unique', () => {
		return (
			data.units.find((u) => u.abbreviation === data.unit.abbreviation) == null ||
			data.units.find((u) => u.abbreviation === data.unit.abbreviation)?.id == data.unit.id
		);
	});

	test('description', 'description is required', () => {
		enforce(data.unit.description).isNotBlank();
	});

	test('description', 'description is to short, it must be larger then 10 chars', () => {
		enforce(data.unit.description).longerThan(10);
	});

	test('datatype', 'at least one dataype is required', () => {
		console.log('datatype', 'check');
		enforce(data.unit.datatypes).isNotBlank();
	});

	test('dimension', 'dimension is required', () => {
		console.log('dimension', 'check');
		enforce(data.unit.dimension?.id).isNotBlank();
	});
});

export default suite;
