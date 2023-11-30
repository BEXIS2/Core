import { create, test, enforce, only } from 'vest';
import type { PatternConstraintListItem } from '../models';

const suite = create((data: PatternConstraintListItem, fieldName) => {
	only(fieldName);

	// test('pattern', 'Pattern is required', () => {
	// 	enforce(data.pattern).isNotBlank();
	// });
});

export default suite;
