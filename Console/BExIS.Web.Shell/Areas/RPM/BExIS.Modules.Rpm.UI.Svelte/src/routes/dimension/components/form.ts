import { create, test, enforce, only } from 'vest';
import type { DimensionListItem } from '../models';

type dataType = {
	dimension: DimensionListItem;
	dimensions: DimensionListItem[];
};

interface regexTestType {
	regex: RegExp;
	output?: string;
	check: boolean;
}

const suite = create((data: dataType, fieldName) => {
	only(fieldName);

	test('name', 'name is required', () => {
		enforce(data.dimension.name).isNotBlank();
	});

	test('name', 'name is not unique', () => {
		return (
			data.dimensions.find((u) => u.name.toLowerCase() === data.dimension.name.toLowerCase()) ==
				null || data.dimensions.find((u) => u.name === data.dimension.name)?.id == data.dimension.id
		);
	});

	test('description', 'description is required', () => {
		enforce(data.dimension.description).isNotBlank();
	});

	//test('description', 'description is to short, it must be larger then 10 chars', () => {
	//	enforce(data.dimension.description).longerThan(10);
	//});

	test('specification', 'specification is required', () => {
		enforce(data.dimension.specification).isNotBlank();
	});

	test(
		'specification',
		'specification has to be like this pattern L([0-9],[0-9])M([0-9],[0-9])T([0-9],[0-9])I([0-9],[0-9])Θ([0-9],[0-9])N([0-9],[0-9])J([0-9],[0-9])',
		() => {
			let regexTest: regexTestType = {
				regex: new RegExp(
					'^(L\\([0-9],[0-9]\\)M\\([0-9],[0-9]\\)T\\([0-9],[0-9]\\)I\\([0-9],[0-9]\\)Θ\\([0-9],[0-9]\\)N\\([0-9],[0-9]\\)J\\([0-9],[0-9]\\))?$'
				),
				check: false
			};
			regexTest.output = data.dimension.specification.match(regexTest.regex)?.[0];
			if (regexTest.output != undefined && regexTest.output != null && regexTest.output != '') {
				data.dimension.specification = regexTest.output;
				regexTest.check = true;
			}
			return regexTest.check;
		}
	);

	test('specification', 'add specification', () => {
		let check: boolean =
			data.dimension.specification == 'L(0,0)M(0,0)T(0,0)I(0,0)Θ(0,0)N(0,0)J(0,0)' ? false : true;
		return check;
	});
});

export default suite;
