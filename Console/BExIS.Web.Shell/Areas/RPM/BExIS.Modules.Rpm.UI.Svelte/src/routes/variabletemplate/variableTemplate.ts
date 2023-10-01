import { create, test, enforce, only } from 'vest';
import { variableTemplatesStore } from './stores';
import { get } from 'svelte/store';

const suite = create((data = {},fieldName) => {

	only(fieldName);
	// const dataTypeWithDisplaypattern = ['date', 'time', 'datetime'];

	test('name', 'name is required', () => {
		enforce(data.name).isNotBlank();
	});

	test('name', 'name allready exist', () => {
		// if the for is in edit mode, find the selected one by id
		const editedObj =
			data.id > 0 ? get(variableTemplatesStore).find((e) => e.id == data.id) : { id: 0, name: '' };
		//console.log(editedObj.name);
		// get all names back, without the edited one
		 const list = get(variableTemplatesStore).map((e) => (e.name != editedObj?.name ? e.name : ''));

		 enforce(data.name).notInside(list);
	});

	test('description', 'description is required', () => {
		enforce(data.description).isNotBlank();
		//console.log("description");
	});

	test('dataType', 'datatype is required', () => {
		//console.log("dataType",data.dataType);

		enforce(data.dataType).isNotNull();
		enforce(data.dataType.text).isNotUndefined();
		enforce(data.dataType.text).isNotEmpty();
	});

	test('dataType', 'data type not match with the unit', () => {
		if (data.unit.dataTypes.includes(data.dataType.text)) {
			return true;
		} else {
			return false;
		}
	});

	// test('displayPattern', 'display pattern is required', () => {
	// 	if (data.dataType && dataTypeWithDisplaypattern.includes(data.dataType.text)) {
	// 		console.log('display pattern test', data.displayPattern);
	// 		enforce(data.displayPattern).isNotNull();
	// 		enforce(data.displayPattern.text).isNotUndefined();
	// 	} else {
	// 		return true;
	// 	}
	// });

	//console.log("before unit",data.unit);
	test('unit', 'unit is required', () => {
		//console.log("unit",data.unit);

		enforce(data.unit).isNotNull();
		enforce(data.unit.text).isNotUndefined();
		enforce(data.unit.text).isNotEmpty();
	});

	test('unit', 'unit not match with the datatype', () => {
		if (data.unit.dataTypes.includes(data.dataType.text)) {
			return true;
		} else {
			return false;
		}
	});
});

export default suite;

