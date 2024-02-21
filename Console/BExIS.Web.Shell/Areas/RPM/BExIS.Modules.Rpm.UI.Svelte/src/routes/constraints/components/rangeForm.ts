import { create, test, enforce, only } from 'vest';
import type { RangeConstraintListItem } from '../models';

const suite = create((data: RangeConstraintListItem, fieldName) => {
	only(fieldName);

	test('lowerbound', 'lowerbound is greater then upperbound', () => {
		return data.lowerbound <= data.upperbound;
	});

	test('upperbound', 'upperbound is less then lowerbound', () => {
		return data.upperbound >= data.lowerbound;
	});
});

export default suite;
