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
			data.units.find((u) => u.name.toLowerCase().trim() === data.unit.name.toLowerCase().trim()) == null ||
			data.units.filter((u) => u.name.toLowerCase().trim() === data.unit.name.toLowerCase().trim() && u.id != data.unit.id).length == 0
		);
	});

	test('abbreviation', 'abbreviation is required', () => {
		enforce(data.unit.abbreviation).isNotBlank();
	});

	test('abbreviation', 'abbreviation is not unique', () => {
		return (
			data.units.find((u) => u.abbreviation.trim() === data.unit.abbreviation.trim()) == null ||
			data.units.filter((u) => u.abbreviation.toLowerCase().trim() === data.unit.abbreviation.toLowerCase().trim() && u.id != data.unit.id).length == 0
		);
	});

	test('description', 'description is required', () => {
		enforce(data.unit.description).isNotBlank();
	});



	test('test', 'measurementSystem is required', () => {
		console.log("🚀 ~ test ~ data.unit.measurementSystem:", data.unit.measurementSystem)
		enforce(data.unit.measurementSystem).isNotBlank();
	});
		


	// test('datatype', 'at least one dataype is required', () => {
	// 	console.log('datatype', 'check');
	// 	enforce(data.unit.datatypes).isNotBlank();
	// });

	// test('dimension', 'dimension is required', () => {
	// 	console.log('dimension', 'check');
	// 	enforce(data.unit.dimension?.id).isNotBlank();
	// });
});

export default suite;
