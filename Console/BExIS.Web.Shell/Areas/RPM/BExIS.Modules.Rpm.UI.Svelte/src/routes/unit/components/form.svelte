<script lang=ts>
    import { TextInput, TextArea, MultiSelect, DropdownKVP, Spinner, ErrorMessage, helpStore } from "@bexis2/bexis2-core-ui";
    import { RadioGroup,RadioItem } from "@skeletonlabs/skeleton";

    import Fa from 'svelte-fa';
    import { faSave, faXmark } from '@fortawesome/free-solid-svg-icons';
    
	import { onMount } from "svelte";
    import * as apiCalls  from '../services/apiCalls';
    

    import type {UnitListItem, DataTypeListItem, DimensionListItem} from "../models";

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
    let listItem = (unit=== undefined || unit.dimension === undefined) ? undefined:{id:unit.dimension.id, text:unit.dimension.name};
    $:dataTypes = dt;
    $:measurementSystems = ms;
    $:dimensions = ds.map(({id, name}) => ({'id': id, 'text': name}));
    $:unit.dimension = (listItem === undefined) ? undefined:ds.find(d => d.id === listItem?.id);
    
    //Help


    onMount(async () => {
        if(unit.id == 0)
        {
           suite.reset();
        }
    })

    async function load() {
        suite.reset();
        dt = await apiCalls.GetDataTypes();
        ms = await apiCalls.GetMeasurementSystems();
        ds = await apiCalls.GetDimensions();
    }

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
        console.log('Unit', unit);
        await apiCalls.EditUnit(unit);
        dispatch("save");
	}

	function cancel() {
        suite.reset();
		dispatch("cancel");
	}

</script>
{#await load()}
    <div class="grid grid-cols-2 gap-5">
        <div class="pb-10">
            <div class="h-9 placeholder animate-pulse"></div>
        </div>
        <div class="pb-10">
            <div class="h-9 placeholder animate-pulse"></div>
        </div>
        <div class="pb-10 col-span-2">
            <div class="h-14 placeholder animate-pulse"></div>
        </div>
        <div class="pb-10">
            <div class="h-9 placeholder animate-pulse"></div>
        </div>
        <div class="pb-10">
            <div class="h-9 placeholder animate-pulse"></div>
        </div>
    
        <div class="pb-10 col-span-2">
            <div class="h-9 placeholder animate-pulse"></div>
        </div>
    
        <div class="py-5 flex justify-end col-span-2">
            <div class="placeholder animate-pulse h-9 w-16"></div>
            <div class="placeholder animate-pulse h-9 w-16"></div>
        </div>
    
    </div>
{:then} 
    <form on:submit|preventDefault={submit}>
        <div class="grid grid-cols-2 gap-5">
            <div class="pb-10" title="Name">
                <TextInput id="name" label="Name" help={true} required={true} bind:value={unit.name} 
                    on:input={onChangeHandler}
                    valid={res.isValid('name')}
                    invalid={res.hasErrors('name')}
                    feedback={res.getErrors('name')}/>
            </div>
            <!-- svelte-ignore a11y-mouse-events-have-key-events -->
            <div class="pb-10" title="Abbreviation">
                <TextInput id="abbreviation" label="Abbreviation" help={true} required={true} bind:value={unit.abbreviation} 
                    on:input={onChangeHandler} 
                    valid={res.isValid('abbreviation')}
                    invalid={res.hasErrors('abbreviation')}
                    feedback={res.getErrors('abbreviation')}/>
            </div>
            <!-- svelte-ignore a11y-mouse-events-have-key-events -->
            <div class="pb-10 col-span-2" title="Description">
                <TextArea id="description" label="Description" help={true} required={true} bind:value={unit.description} 
                    on:input={onChangeHandler} 
                    valid={res.isValid('description')}
                    invalid={res.hasErrors('description')}
                    feedback={res.getErrors('description')}/>
            </div>
        
            <div class="pb-10" title="Data Types">
                <MultiSelect
                    title="Data Types"
                    id="dataTypes"
                    bind:target = {unit.datatypes}
                    source={dataTypes}
                    itemId="id"
                    itemLabel="name"
                    complexSource={true}
                    complexTarget={true}
                    required={true}
                    help={true}
                />
            </div>
            <div class="pb-10" title="Dimension">
                <DropdownKVP
                    id="dimension"
                    title="Dimension"
                    bind:target = {listItem}
                    source={dimensions}
                    required={true}
                    complexTarget={true}
                    help={true}
                />
            </div>
        
            <!-- svelte-ignore a11y-mouse-events-have-key-events -->
            <div class="pb-10" title="Measurement System" on:mouseover={() => {helpStore.show('measurementSystem');}}>
                <label for="{unit.measurementSystem}" class="text-sm">Measurement System</label>
                <RadioGroup help={true}>
                    {#each measurementSystems as item}
                        <RadioItem bind:group={unit.measurementSystem} name="measurementSystem" id="measurementSystem" value={item}>{item}</RadioItem>
                    {/each}
                </RadioGroup>
            </div>
        
            <div class="py-5 text-right col-span-2">
                <!-- svelte-ignore a11y-mouse-events-have-key-events -->
                <button type="button" class="btn variant-filled-warning h-9 w-16 shadow-md" title="Cancel" id="cancel" on:mouseover={() => {helpStore.show('cancel');}} on:click={() => cancel()}><Fa icon={faXmark} /></button>
                <!-- svelte-ignore a11y-mouse-events-have-key-events -->
                <button type="submit"  class="btn variant-filled-primary h-9 w-16 shadow-md" title="Save Unit, {unit.name}" id="save" on:mouseover={() => {helpStore.show('save');}}><Fa icon={faSave} /></button>
            </div>
        
        </div>
    </form>
{:catch error}
    <div class="flex justify-center">
        <ErrorMessage {error}/>
    </div>
{/await}