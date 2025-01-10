<script lang='ts'>
	import { onMount } from 'svelte';
    import FormGenerator from './components/formGenerator.svelte';

    import * as apiCalls from './services/apiCalls';

    import type { schemaNode } from './models';

    import test from './test.json';
	import { Page } from '@bexis2/bexis2-core-ui';
 
    let s: any;
    let schema: any = test;

    //$: schema = s;
    //$: createObjectFromSchema(schema);

    onMount(async () => {
        //s = await apiCalls.GetMetadataScheema(1);
	});

    function renderForm(schema: any, path: string = ''): string {
        if (schema.type === 'object' && schema.properties) {
            return Object.entries(schema.properties).map(([key, value]: [string, any]) => {
                const fullPath = path ? `${path}.${key}` : key;
                return renderFormField(key, value, fullPath);
            }).join('');
        } else {
            return '';
        }
    }

    function renderFormField(key: string, schema: any, path: string): string {
        if(!key.startsWith('@'))
        {
        let label: string = '';
        if( schema.properties && schema.type === 'object' && !schema.properties['#text']) {
            label = `<label for="${path}">${key}</label><hr>`;
        }
        else if( schema.properties && schema.type === 'object' && schema.properties['#text']) {
            label = `<label for="${path}">${key}</label>`;
        }
        let input = '';

        switch (schema.type) {
            case 'string':
                input = `<input type="text" id="${path}" name="${key}" />`;
                break;
            case 'boolean':
                input = `<input type="checkbox" id="${path}" name="${key}" />`;
                break;
            case 'int':
            case 'number':
                input = `<input type="number" id="${path}" name="${key}" />`;
                break;
            case 'date':
                input = `<input type="date" id="${path}" name="${key}" />`;
                break;
            case 'object':
                input = renderForm(schema, path);
                break;
            case 'array':
                input = '<h1 class="btn">array</h1>';
                break;
            default:
                input = `<input type="text" id="${path}" name="${key}" />`;
                break;
        }

        return `<div class="form-group">${label}${input}</div>`;
        }
    }
</script>

<Page>
    <h1>Metadata</h1>
    <FormGenerator schema={schema} path= {''} />
</Page>


