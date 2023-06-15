<script lang=ts>
    import { TextInput, TextArea, MultiSelect, DropdownKVP} from "@bexis2/bexis2-core-ui";
    import { RadioGroup,RadioItem } from "@skeletonlabs/skeleton";

    import Fa from 'svelte-fa/src/fa.svelte';
	import { faSave, faTrashAlt } from '@fortawesome/free-regular-svg-icons/index';

    import type {DimensionListItem} from "../models";
	import { onMount } from "svelte";
    import * as apiCalls  from '../services/apiCalls';

    // event
    import { createEventDispatcher } from 'svelte';
    const dispatch = createEventDispatcher();


    // validation
	import suite from './form';
    
    // load form result object
    let res = suite.get();

    // use to actived save if form is valid
    $: disabled = !res.isValid();

    // init unit
    export let dimension:DimensionListItem;
    export let dimensions:DimensionListItem[];

    
    onMount(async () => {
        if(dimension.id == 0)
        {
           suite.reset();
        }
    })

    //change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e:any) {

        //console.log("input changed", e)
        // add some delay so the entityTemplate is updated
        // otherwise the values are old
        setTimeout(async () => {
            // check changed field
            res = suite({dimension:dimension, dimensions:dimensions}, e.target.id);
        }, 10);
     
	}

    async function submit() 
    {
        let test = await apiCalls.EditDimension(dimension);
        console.log('valid', test);
        dispatch("save");
	}

	function clear() {
		dispatch("cancel");
	}

</script>
{#if dimension}
<form on:submit|preventDefault={submit}>
<div class="grid">
    <div class="pb-10">
        <TextInput id="name" label="Name" required={true} bind:value={dimension.name} 
            on:input={onChangeHandler} 
            valid={res.isValid('name')}
            invalid={res.hasErrors('name')}
		    feedback={res.getErrors('name')}/>
    </div>
    <div class="pb-10">
        <TextArea id="description" label="Description" required={true} bind:value={dimension.description} 
            on:input={onChangeHandler} 
            valid={res.isValid('description')}
            invalid={res.hasErrors('description')}
            feedback={res.getErrors('description')}/>
    </div>
    <div class="pb-10">
        <TextArea id="specification" label="Specification" required={true} bind:value={dimension.specification} 
            on:input={onChangeHandler} 
            valid={res.isValid('specification')}
            invalid={res.hasErrors('specification')}
            feedback={res.getErrors('specification')}/>
    </div>
    <div class="py-5">
		<button type="submit"  class="btn variant-filled-primary"
            ><Fa icon={faSave} /></button
        >
		<button type="button" class="btn variant-filled-warning" on:click={() => clear}
			><Fa icon={faTrashAlt} /></button
		>
	</div>

</div>
</form>
{/if}