import { create, test, enforce, only } from 'vest';
import type { SearchConfigSchema } from '$lib/components/SearchConfig/SearchConfigModel';



const suite = create((data: SearchConfigSchema, fieldName?: string) => {
    if (fieldName) {
        only(fieldName);
    }
	
    // Dummy test for facets_to_index
    test('facets_to_index', 'value is true', () => {
        // Example validation: facets_to_index must be true
        enforce(data.global.search_components.facets_to_index).isTruthy();
    
		//enforce(data.global.search_components.facets_to_index).isNotBlank();
	});

});

export default suite;