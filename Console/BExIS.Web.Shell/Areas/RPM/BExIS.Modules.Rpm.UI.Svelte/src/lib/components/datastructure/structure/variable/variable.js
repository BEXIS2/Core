import { create, test, enforce, only, skip } from 'vest';

const suite = create((data = {}, fieldName) => {
	//console.log("validation", data.name,data.dataType.text, data);
	//skip('displayPattern');

	let dataTypeWithDisplaypattern = ['date', 'time', 'datetime'];

	//only(fieldName);
	test('name', 'name is required', () => {
		enforce(data.name).isNotBlank();
		//console.log("description");
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


		test('displayPattern', 'display pattern is required', () => {
			if (data.dataType && dataTypeWithDisplaypattern.includes(data.dataType.text)) {
				//console.log('display pattern test', data.displayPattern);
				enforce(data.displayPattern).isNotNull();
				enforce(data.displayPattern.text).isNotUndefined();
			}
			else
			{
				return true;
			}
		});

	//console.log("before unit",data.unit);
	test('unit', 'unit is required', () => {
		//console.log("unit",data.unit);

		enforce(data.unit).isNotNull();
		enforce(data.unit.text).isNotUndefined();
		enforce(data.unit.text).isNotEmpty();
	});
});

export default suite;
