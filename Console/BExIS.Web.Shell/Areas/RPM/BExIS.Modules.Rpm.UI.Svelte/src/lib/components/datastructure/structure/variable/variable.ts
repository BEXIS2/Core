import { create, test, enforce, only, skipWhen } from 'vest';
import { isTemplateRequiredStore } from '../../store';
import { get } from 'svelte/store';

const suite = create((data = {}, fieldName) => {
	//console.log("validation", data);
	//skip('displayPattern');

	const dataTypeWithDisplaypattern = ['date', 'time', 'datetime'];
	const isTemplateRequired = get(isTemplateRequiredStore);

	//only(fieldName);
	test('name', 'name is required', () => {
		enforce(data.name).isNotBlank();
		//console.log("description");
	});

	test('description', 'description is required', () => {
		enforce(data.description).isNotBlank();
		//console.log("description");
	});

	// Datatype
	test('dataType', 'datatype is required', () => {
		//console.log("dataType",data.dataType);

		enforce(data.dataType).isNotNull();
		enforce(data.dataType.text).isNotUndefined();
		enforce(data.dataType.text).isNotEmpty();
	});

	skipWhen(
		(res) => res.hasErrors('dataType'),
		() => {
			test('dataType', 'data type not match with the unit', () => {
				if (data.unit.dataTypes.includes(data.dataType.text)) {
					return true;
				} else {
					return false;
				}
			});
		}
	);

	test('displayPattern', 'display pattern is required', () => {
		if (data.dataType && dataTypeWithDisplaypattern.includes(data.dataType.text)) {
			//console.log('display pattern test', data.displayPattern);
			enforce(data.displayPattern).isNotNull();
			enforce(data.displayPattern.text).isNotUndefined();
		} else {
			return true;
		}
	});

	// UNIT
	//console.log("before unit",data.unit);
	test('unit', 'unit is required', () => {
		//console.log("unit",data.unit);

		enforce(data.unit).isNotNull();
		enforce(data.unit.text).isNotUndefined();
		enforce(data.unit.text).isNotEmpty();
	});

	skipWhen(
		(res) => res.hasErrors('unit'),
		() => {
			test('unit', 'unit not match with the data type', () => {
				if (data.unit.dataTypes.includes(data.dataType.text)) {
					return true;
				} else {
					return false;
				}
			});

			test('unit', 'unit not match with the template', () => {
				if (!isTemplateRequired && (!data.template || data.template.id == 0)) {
					return true;
				}

				console.log('🚀 ~ file: variable.ts:84 ~ test ~ data:', data);
				if (data.template.units.includes(data.unit.text)) {
					return true;
				} else {
					return false;
				}
			});
		}
	);

	// Template
	test('variableTemplate', 'template is required', () => {
		//console.log("unit",data.unit);

		if (!isTemplateRequired) {
			return true;
		}

		enforce(data.template).isNotNull();
		enforce(data.template.text).isNotUndefined();
		enforce(data.template.text).isNotEmpty();
	});

	skipWhen(
		(res) => res.hasErrors('variableTemplate'),
		() => {
			test('variableTemplate', 'unit not match with the template', () => {
				//console.log('🚀 ~ file: variable.ts:100 ~ test ~ data.template:', data.template);
				if (!isTemplateRequired && (!data.template || data.template.id == 0)) {
					return true;
				}

				if (data.template.units.includes(data.unit.text)) {
					return true;
				} else {
					return false;
				}
			});

			test('variableTemplate', 'data type not match with the template', () => {
				// run this check only
				// 1. template is required
				// 2. template is not required but set
				// run this not if
				// 1. template is null && template is not required
				if (!isTemplateRequired && (!data.template || data.template.id == 0)) {
					return true;
				}

				if (data.template.dataTypes.includes(data.dataType.text)) {
					return true;
				} else {
					return false;
				}
			});
		}
	);
});

export default suite;
