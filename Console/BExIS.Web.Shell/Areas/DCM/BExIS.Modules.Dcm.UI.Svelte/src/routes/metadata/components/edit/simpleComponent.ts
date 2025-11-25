import { create, test, enforce, only, skipWhen, each } from 'vest';
import { validationStore } from '../../stores';
import type { validationStoretype } from '../../models';
import { getValidationStore } from '../../utils';

// Get current values from the validation store
let validationStoreValues: validationStoretype = getValidationStore();
// Define a Vest test suite for validating simple component data
// based on the validation rules defined in the validation store
const suite = create((data: any, fieldName: string) => {
    only(fieldName);
    if (validationStoreValues.simpleTypeValidationItems.length > 0) {
        // Iterate over each validation item in the store
        each(validationStoreValues.simpleTypeValidationItems, (item) => {
            if(fieldName == item.path){
                // Validate required field
                if(item.required){
                    test( item.path, `${item.label} is required`, () => {      
                        enforce(data).isNotBlank();
                    });
                }
                // Validate minimum length if defined
                if(item.minLength != null && item.minLength != undefined){
                    test( item.path, `${item.label} must have a minimum length of ${item.minLength}`, () => {
                        enforce(data).longerThanOrEquals(item.minLength);
                        
                    });
                }
                // Validate maximum length if defined
                if(item.maxLength != null && item.maxLength != undefined){
                    test( item.path, `${item.label} must have a minimum length of ${item.maxLength}`, () => {
                        enforce(data).shorterThanOrEquals(item.maxLength);
                        
                    });
                }
                // Validate regex pattern if defined
                if(item.regex != '' && item.regex != null && item.regex != undefined){
                    test( item.path, `${item.label} dosn't match the required pattern (${item.regex})`, () => {
                        enforce(data).matches(new RegExp(item.regex!));
                    });
                }
            }
        });
    }
});

export default suite;