<script lang="ts">
	import { onMount, afterUpdate, createEventDispatcher } from 'svelte';
	import { fade } from 'svelte/transition';
	// UI Components
	import { TextInput, TextArea, MultiSelect } from '@bexis2/bexis2-core-ui';

	//types
	import type { listItemType } from '@bexis2/bexis2-core-ui';
	import type { VariableModel, missingValueType } from '$models/StructureSuggestion';

	//stores
	import { get } from 'svelte/store';
	import { displayPatternStore } from '../store';

	import DataTypeDescription from './DataTypeDescription.svelte';
	import Container from './Container.svelte';
	import Header from './Header.svelte';
	import Overview from './Overview.svelte';

	import suite from './variable';


	export let variable: VariableModel;
	$: variable;
	export let data: string[];
	$:data;
	
	export let index: number;

	export let datatypes: listItemType[];
	export let units: listItemType[];
	export let missingValues: missingValueType[];

	export let isValid: boolean = false;
	export let last: boolean = false;

	export let expand:boolean ;


	$: isValid;
	// validation
	let res = suite.get();

	let loaded = false;

	//displaypattern
	let displayPattern: listItemType[];
	$: displayPattern;

	function updateDisplayPattern(type, reset = true) {
		// currently only date, date tim e and time is use with display pattern.
		// however the serve only now datetime so we need to preselect the possible display pattern to date, time and datetime
		let allDisplayPattern = get(displayPatternStore);

		if (type != undefined) {
			if (type.text.toLowerCase() === 'date') {
				// date without time
				displayPattern = allDisplayPattern.filter(
					(m) =>
						m.group.toLowerCase().includes(type.text) &&
						(!m.text.toLowerCase().includes('h') || !m.text.toLowerCase().includes('s'))
				);
			} else if (type.text.toLowerCase() === 'time') {
				// time without date
				displayPattern = allDisplayPattern.filter(
					(m) =>
						m.group.toLowerCase().includes(type.text) &&
						(!m.text.toLowerCase().includes('d') || !m.text.toLowerCase().includes('y'))
				);
			} else if (type.text.toLowerCase() === 'datetime') {
				// both
				displayPattern = allDisplayPattern.filter((m) => m.group.toLowerCase().includes(type.text));
			} else {
				displayPattern = [];
			}
		} else {
			displayPattern = [];
		}
		// when type has change, reset value, but after copy do not reset
		// thats why reset need to set
		if (reset) variable.displayPattern = undefined;

		if (displayPattern.length > 0) {
			res = suite(variable, 'displayPattern');
		}
	}

	const dispatch = createEventDispatcher();

	onMount(() => {
		datatypes = [...datatypes.filter((d) => d.id != variable.dataType.id)];
		datatypes = [variable.dataType, ...datatypes];

		units = [...units.filter((d) => !variable.possibleUnits.some((u) => u.id == d.id))];
		units = [...variable.possibleUnits, ...units];

		loaded = true;
		// reset & reload validation
		suite.reset();

		

		setTimeout(async () => {
			updateDisplayPattern(variable.dataType);
			res = suite(variable);
			setValidationState(res);
		}, 10);
	});

	afterUpdate(() => {
		updateDisplayPattern(variable.dataType, false);
		res = suite(variable);
		setValidationState(res);
		// console.log("u",variable.name);
		// console.log("--------------------");
	});

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e) {
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			res = suite(variable, e.target.id);
			setValidationState(res);

			//console.log(res);
			//console.log(res.isValid());
		}, 100);
	}

	//change event: if select change check also validation only on the field
	// *** is the id of the input component
	function onSelectHandler(e, id) {
		setTimeout(async () => {
			res = suite(variable, id);

			//console.log(res);
			//console.log(res.isValid());
			// update display patter and reset it if it changed
			if (id == 'dataType') {
				updateDisplayPattern(variable.dataType);
			}

			setValidationState(res);
		}, 100);
	}

	function setValidationState(res) {
		isValid = res.isValid();
		// dispatch this event to the parent to check the save button
		dispatch('var-change');
	}



	function cutData(d)
	{

			for (let index = 0; index < d.length; index++) {
				let v = d[index];

				if(v.length>10)
				{
					 d[index] = v.slice(0, 10)+"...";
				}
			}

			return d;
	}

</script>

{#if loaded && variable}
	{#if expand}
	<div class="card">
		<header id="header_{index}" class="card-header" >
			<Header
				{index}
				name={variable.name}
				bind:isKey={variable.isKey}
				bind:isOptional={variable.isOptional}
				bind:isValid
				bind:expand
			/>
		</header>
		
			
		<section class="p-4">
			<!--Description-->
			<Container>
				<div slot="property">
					<TextArea
						id="description"
						label="Description"
						bind:value={variable.description}
						on:input={onChangeHandler}
						valid={res.isValid('description')}
						invalid={res.hasErrors('description')}
						feedback={res.getErrors('description')}
					/>
				</div>
				<div slot="description">
					{#if data}
						<b>Data preview: </b> {cutData(data).join(', ')}
					{/if}
				</div>
			</Container>

			<!--Datatype-->
			<Container>
				<div slot="property">
					<MultiSelect
						id="dataType"
						title="Data Type"
						source={datatypes}
						itemId="id"
						itemLabel="text"
						itemGroup="group"
						complexSource={true}
						complexTarget={true}
						isMulti={false}
						bind:target={variable.dataType}
						placeholder="-- Please select --"
						invalid={res.hasErrors('dataType')}
						feedback={res.getErrors('dataType')}
						clearable={false}
						on:change={(e) => onSelectHandler(e, 'dataType')}
					/>
				</div>

				<div slot="displaypattern">
					<!--Show only when display pattern exists-->
					{#if displayPattern != undefined && displayPattern.length > 0}
						<MultiSelect
							id="displayPattern"
							title="Display Pattern"
							source={displayPattern}
							itemId="id"
							itemLabel="text"
							itemGroup="group"
							complexSource={true}
							complexTarget={true}
							isMulti={false}
							clearable={false}
							bind:target={variable.displayPattern}
							placeholder="-- Please select --"
							invalid={res.hasErrors('displayPattern')}
							feedback={res.getErrors('displayPattern')}
							on:change={(e) => onSelectHandler(e, 'displayPattern')}
						/>
					{/if}
				</div>
				<div slot="description">
					{#if variable.dataType}
						<DataTypeDescription type={variable.dataType.text} {missingValues} />
					{/if}
				</div>
			</Container>

			<!--Unit-->
			<Container>
				<div slot="property">
					<MultiSelect
						id="unit"
						title="Unit"
						source={units}
						itemId="id"
						itemLabel="text"
						itemGroup="group"
						complexSource={true}
						complexTarget={true}
						isMulti={false}
						clearable={false}
						bind:target={variable.unit}
						placeholder="-- Please select --"
						invalid={res.hasErrors('unit')}
						feedback={res.getErrors('unit')}
						on:change={(e) => onSelectHandler(e, 'unit')}
					/>
				</div>
				<div slot="description">
					show all information about the units in a table Nothing found? Make a new suggestion.
				</div>
			</Container>

			<!--Meaning-->
			<Container>
				<div slot="property">
					<TextInput id="template" label="Variable Template" bind:value={variable.template.text} />
				</div>
				<div slot="description">...</div>
			</Container>
		</section>


		<footer class="card-footer">
			<div class="flex">
				<div class="grow" />
				<div class=" flex-none text-right">
					<slot name="options" />
				</div>
			</div>
		</footer>
	</div>
		{:else}
		<Overview 
			{index} 
			name={variable.name} 
			isOptional= {variable.isOptional} 
			isKey={variable.isKey} 
			bind:expand={expand}
			{isValid}
			unit={variable.unit.text}
			datatype={variable.dataType.text}
			description={variable.description}
			datapreview={cutData(data).join(', ')}
			/>
		{/if}

{/if}
