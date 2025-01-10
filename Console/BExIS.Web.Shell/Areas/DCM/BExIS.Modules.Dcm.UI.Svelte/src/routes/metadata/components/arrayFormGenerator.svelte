<script lang="ts">
import { onMount } from 'svelte';
import { TextInput, NumberInput, TextArea, DropdownKVP, helpStore, CodeEditor } from '@bexis2/bexis2-core-ui';
import FormGenerator from './formGenerator.svelte';
import ArrayFormGenerator from './arrayFormGenerator.svelte';

export let schema: any;
export let path: string;
export let colorToggle: boolean = false;

console.log('schema',schema);

console.log('min',schema.minItems);
console.log('max',schema.maxItems);
</script>

<div id="{path}">
    {#if schema.items}
        <FormGenerator schema={schema.items} path={path} colorToggle={colorToggle}  /> 
    {/if} 
</div>
