<script lang="ts">
import { onMount } from 'svelte';
import { TextInput, NumberInput, TextArea, DropdownKVP, helpStore, CodeEditor } from '@bexis2/bexis2-core-ui';
import ComplexComponent from './complexComponentWrapper.svelte';
import SimpleComponent from './simpleComponentWrapper.svelte';
import { getValueByPath, setValueByPath,  updateMetadataStore} from '../../utils';


export let arrayComponent: any;
export let path: string;
export let required: boolean = false;

let label = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
let requiredList = arrayComponent.items && arrayComponent.items.type === 'object' && arrayComponent.items.required ? arrayComponent.items.required : [];

let value = getValueByPath(path);
console.log('value', value);

</script>

    {#if arrayComponent.items}
    {#if required}
				<h4 class="h4">{label} *    +|- </h4>
			{:else}
			   	<h4 class="h4">{label}    +|- </h4>
			{/if}
    
    <div class="p-2" id="{path}">
        {#if arrayComponent.items.type === 'object' && arrayComponent.items.properties && !arrayComponent.items.properties['#text']}
            <ComplexComponent complexComponent={arrayComponent.items} path={path} required={requiredList.includes(label)}/>           
        {:else if arrayComponent.items.type === 'object' && arrayComponent.items.properties['#text']}
            <SimpleComponent simpleComponent={arrayComponent.items} path={path} required={requiredList.includes(label)}/>
         {/if} 
    </div> 
    {/if} 
