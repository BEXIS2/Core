import { create, test, enforce, only } from 'vest';
import type { DomainConstraintListItem } from '../models';

const suite = create((data: DomainConstraintListItem, fieldName) => {
	only(fieldName);

	// test('domain', 'Domain is required', () => {
	// 	enforce(data.domain).isNotBlank();
	// });
});

export default suite;
