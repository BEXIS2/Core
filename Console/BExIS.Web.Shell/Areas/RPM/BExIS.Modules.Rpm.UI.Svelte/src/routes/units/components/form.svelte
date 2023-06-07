<script lang=ts>
    import { TextInput, TextArea, MultiSelect} from "@bexis2/bexis2-core-ui";
    import { RadioGroup,RadioItem } from "@skeletonlabs/skeleton";

    import Fa from 'svelte-fa/src/fa.svelte';
	import { faSave, faTrashAlt } from '@fortawesome/free-regular-svg-icons/index';

    import type {UnitListItem, DataTypeListItem, } from "../models";
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
    export let unit:UnitListItem;

    let dt: DataTypeListItem[];
    $:dataTypes = dt;
    let ms: string[];
    $:measurementSystems = ms;

    onMount(async () => {
        dt = await apiCalls.GetDataTypes();
        ms = await apiCalls.GetMeasurementSystems()

        if(unit.id == 0)
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
            res = suite(unit, e.target.id);
        }, 10);
     
	}

    async function submit() 
    {
        await apiCalls.EditUnit(unit);
        dispatch("save");
	}

	function clear() {
		dispatch("cancel");
	}

</script>
{#if dataTypes && measurementSystems}
<form on:submit|preventDefault={submit}>
<div class="grid">
    <div class="pb-10">
        <TextInput id="name" label="Name" required={true} bind:value={unit.name} 
            on:input={onChangeHandler} 
            valid={res.isValid('name')}
            invalid={res.hasErrors('name')}
		    feedback={res.getErrors('name')}/>
    </div>
    <div class="pb-10">
        <TextArea id="description" label="Description" required={true} bind:value={unit.description} 
            on:input={onChangeHandler} 
            valid={res.isValid('description')}
            invalid={res.hasErrors('description')}
            feedback={res.getErrors('description')}/>
    </div>
    <div class="pb-10">
        <TextInput id="abbreviation" label="Abbreviation" required={true} bind:value={unit.abbreviation} 
            on:input={onChangeHandler} 
            valid={res.isValid('abbreviation')}
            invalid={res.hasErrors('abbreviation')}
		    feedback={res.getErrors('abbreviation')}/>
    </div>
    <div class="pb-10">
        <MultiSelect
            title="Associated Data Types"
            bind:target = {unit.datatypes}
            source={dataTypes}
            itemId="id"
            label="name"
            isComplex={true}
            isTargetComplex={true}
            required={true}
        />
    </div>
  
    <div class="pb-10">
        <label for="{unit.measurementSystem}" class="text-sm">Measurement System</label>
        <RadioGroup>
            {#each measurementSystems as item}
                <RadioItem bind:group={unit.measurementSystem} name="justify" value={item}>{item}</RadioItem>
            {/each}
        </RadioGroup>
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