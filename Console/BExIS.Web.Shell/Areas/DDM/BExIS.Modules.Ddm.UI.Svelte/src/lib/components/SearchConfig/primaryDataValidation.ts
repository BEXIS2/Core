import { create, test, enforce } from 'vest';
import type { CalcBlockListItem } from '$lib/components/SearchConfig/SearchConfigModel';



const suite = create((data: CalcBlockListItem) => {
    // Always declare tests in the same order so Vest can track them reliably.
    test('entityTemplate', 'entity template is required', () => {
        enforce(data.template_name).isNotBlank();
    });

    test('operation', 'operation is required', () => {
        enforce(data.operation).isNotBlank();
    });

    test('meanings', 'at least one meaning must be selected', () => {
       // const count = data.allowed_meanings.length;
       // console.log('meanings count for validation:', count);
       // enforce(count).isGreaterThan(0);
       // Updated to check that allowed_meanings is an array and has at least one item
       //enforce(Array.isArray(data.allowed_meanings) && data.allowed_meanings.length).isGreaterThan(0);

       // test now only is array
       enforce(data.allowed_meanings).isArray().isNotEmpty();
       //enforce(data.operation).isNotBlank();
    });


});
export default suite;