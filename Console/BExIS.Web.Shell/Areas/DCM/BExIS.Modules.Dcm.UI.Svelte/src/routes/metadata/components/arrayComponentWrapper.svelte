<script lang="ts">
import { onMount } from 'svelte';
import { TextInput, NumberInput, TextArea, DropdownKVP, helpStore, CodeEditor } from '@bexis2/bexis2-core-ui';
import ComplexComponent from './complexComponentWraper.svelte';


export let arrayComponent: any;
export let path: string;

</script>

    {#if arrayComponent.items}
    {path}
    <div class="p-2 grid grid-cols-2 gap-5" id="{path}">
        <ComplexComponent complexComponent={arrayComponent.items} path={path}  />
    </div> 
    {/if} 
