import { create, test, enforce, only } from 'vest';
import { prefixCategoryStore } from '$lib/components/meaning/stores';
import { get } from 'svelte/store';
import type { prefixCategoryType } from '$lib/components/meaning/types';

const suite = create((data: prefixCategoryType, fieldName) => {
	only(fieldName);

	test('name', 'name is required', () => {
		enforce(data.name).isNotBlank();
	});

	test('name', 'name is not unique', () => {
		const prefixCategories = get(prefixCategoryStore);
		return (
			prefixCategories.find(
				(u) => u.name.toLowerCase().trim() === data.name.toLowerCase().trim()
			) == null ||
			prefixCategories.filter(
				(u) =>
					u.name.toLowerCase().trim() === data.name.toLowerCase().trim() &&
					u.id != data.id
			).length == 0
		);
	});
});

export default suite;
