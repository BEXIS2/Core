<script lang=ts>
    import { TextInput, TextArea, MultiSelect, DropdownKVP} from "@bexis2/bexis2-core-ui";
    import { RadioGroup,RadioItem } from "@skeletonlabs/skeleton";

    import Fa from 'svelte-fa/src/fa.svelte';
	import { faSave, faTrashAlt } from '@fortawesome/free-regular-svg-icons/index';

    import type {UnitListItem, DataTypeListItem, DimensionListItem} from "../models";
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
    export let units:UnitListItem[];

    let dt: DataTypeListItem[];
    let ms: string[];
    let ds: DimensionListItem [] = [];
    let listItem = {id:unit.dimension.id, text:unit.dimension.name};
    $:dataTypes = dt;
    $:measurementSystems = ms;
    $:dimensions = ds.map(({id, name}) => ({'id': id, 'text': name}));;
    $:unit.dimension = {id:listItem.id || 0, name:listItem.text || ""};

    onMount(async () => {
        dt = await apiCalls.GetDataTypes();
        ms = await apiCalls.GetMeasurementSystems();
        ds = await apiCalls.GetDimensions();

        console.log('ds',ds)

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
            res = suite({unit:unit, units:units}, e.target.id);
        }, 10);
     
	}

    async function submit() 
    {
        let test = await apiCalls.EditUnit(unit);
        console.log('valid', test);
        dispatch("save");
	}

	function clear() {
		dispatch("cancel");
	}

</script>
{#if dataTypes && measurementSystems && dimensions.length > 0}
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
            title="datatype"
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
        <DropdownKVP
            id="dimension"
            title="Dimension"
            bind:target = {listItem}
            source={dimensions}
            required={true}
            targetIsComplex={true}
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