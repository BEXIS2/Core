import { create, test, enforce, only, each } from 'vest';
import type { validationStoretype } from '$lib/components/utils/metadata/models';
import { getValidationStore } from '$lib/components/utils/metadata/metadataComponentUtils';
import { hideStore } from '$lib/components/utils/metadata/stores';
import { get } from 'svelte/store';

// Get current values from the validation store
let validationStoreValues: validationStoretype = getValidationStore();
// if the elements hidden by hideStore are not validated, we need to get the current values of the hideStore to check in the validation if the field is hidden or not
let hideStoreValues: any = get(hideStore);


// Define a Vest test suite for validating simple component data
// based on the validation rules defined in the validation store
const suite = create((data: any, fieldName: string='') => {

    if (fieldName) {
      only(fieldName);
    }

    //console.log('Validation Store Values:', validationStoreValues);
    if (validationStoreValues.simpleTypeValidationItems.length > 0) {
        // Iterate over each validation item in the store
        each(validationStoreValues.simpleTypeValidationItems, (item) => {
            if((fieldName && fieldName == item.path) || fieldName === ''){
                // Validate required field
                if(item.required){
                    console.log('Validating required for field:', item.path);
                    test( item.path, `${item.label} is required`, () => {      
                        enforce(data).isNotBlank();
                    });
                }
                // Validate minimum length if defined
                if(item.minLength != null && item.minLength != undefined){
                    console.log('Validating minLength for field:', item.path);
                    test( item.path, `${item.label} must have a minimum length of ${item.minLength}`, () => {
                        enforce(data).longerThanOrEquals(item.minLength);
                        
                    });
                }

                // Validate maximum length if defined
                if(item.maxLength != null && item.maxLength != undefined){
                    console.log('Validating maxLength for field:', item.path);
                    test( item.path, `${item.label} must have a maximum length of ${item.maxLength}`, () => {
                        enforce(data).shorterThanOrEquals(item.maxLength);
                        
                    });
                }

                // Validate minimum if defined
                if(item.minimum != null && item.minimum != undefined){
                    console.log('Validating minimum for field:', item.path);
                    test( item.path, `${item.label} must have a minimum of ${item.minimum}`, () => {
                        enforce(data).greaterThanOrEquals(item.minimum);
                    });
                }
                
                // Validate maximum if defined
                if(item.maximum != null && item.maximum != undefined){
                    console.log('Validating maximum for field:', item.path);
                    test( item.path, `${item.label} must have a maximum of ${item.maximum}`, () => {
                        enforce(data).lessThanOrEquals(item.maximum);
                    });
                }
                // Validate regex pattern if defined
                if(item.regex != '' && item.regex != null && item.regex != undefined){
                    console.log('Validating regex pattern for field:', item.path);
                    test( item.path, `${item.label} doesn't match the required pattern (${item.regex})`, () => {
                        enforce(data).matches(new RegExp(item.regex!));
                    });
                }

                // Validate enum values if defined
                if(item.enum && item.enum.length>0){
                    console.log('Validating regex pattern for field:', item.path);
                    test( item.path, `Invalid ${item.label}. Choose from the list.`, () => {
                        enforce(data).isValueOf(item.enum);
                    });
                }
            }
        });
    }
});

export default suite;