import { create, test, enforce, only, skipWhen, each } from 'vest';
import { validationStore } from '../../stores';
import type { validationStoretype } from '../../models';
import { getValidationStore } from '../../utils';

let validationStoreValues: validationStoretype = getValidationStore();

const suite = create((data: any, fieldName: string) => {
    only(fieldName);
    if (validationStoreValues.simpleTypeValidationItems.length > 0) {
        each(validationStoreValues.simpleTypeValidationItems, (item) => {
            skipWhen(!item.required && item.path != fieldName, () => {
                test( item.path, `${item.label} is required`, () => {
                    enforce(data).isNotBlank();
                });
            });
        });
    }
});

export default suite;