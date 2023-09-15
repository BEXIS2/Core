<script lang="ts">
	import { onMount, afterUpdate, createEventDispatcher } from 'svelte';

	// UI Components
	import { TextInput, TextArea, MultiSelect, helpStore } from '@bexis2/bexis2-core-ui';

	//types
	import type { listItemType } from '@bexis2/bexis2-core-ui';
	import type {
		VariableTemplateModel,
		missingValueType
	} from '$lib/components/datastructure/types';

	//stores
	import { get } from 'svelte/store';
	import { displayPatternStore } from '$lib/components/datastructure/store';

	// icons
	import Fa from 'svelte-fa';
	import { faXmark, faSave, faChevronUp, faChevronDown } from '@fortawesome/free-solid-svg-icons';


	import suite from './variableTemplate';
	import MissingValues from '$lib/components/datastructure/MissingValues.svelte';

	export let variable: VariableTemplateModel;
	$: variable;

	export let datatypes: listItemType[];
	export let units: listItemType[];
	export let missingValues: missingValueType[];

	export let isValid: boolean = false;
	let help: boolean = true;

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
				console.log("dataType");
				
				updateDisplayPattern(variable.dataType);
				console.log("displaypatter", displayPattern);

			}

			setValidationState(res);
		}, 100);
	}

	function setValidationState(res) {
		isValid = res.isValid();
		// dispatch this event to the parent to check the save button
		dispatch('var-change');
	}

	function cancel()
	{
		dispatch('cancel');
	}

	function submit()
	{

	}

</script>
<form on:submit|preventDefault={submit}>
<div id="variable-{variable.id}-form" class="flex-colspace-y-5 card shadow-md p-5">

	<div class="flex gap-5">
		<div class="grow">
		<TextInput
			id="name"
			label="Name"
			bind:value={variable.name}
			on:input={onChangeHandler}
			valid={res.isValid('name')}
			invalid={res.hasErrors('name')}
			feedback={res.getErrors('name')}
			{help}
		/>
	</div>
		<!--Description-->
		<div class="grow">
		<TextArea
			id="description"
			label="Description"
			bind:value={variable.description}
			on:input={onChangeHandler}
			valid={res.isValid('description')}
			invalid={res.hasErrors('description')}
			feedback={res.getErrors('description')}
			{help}
		/>
	</div>

	</div>

	<div class="flex gap-5">
		<div class="grow w-1/2">
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
			{help}
		/>
		</div>
		<div class="grow w-1/4">
		<!--Data Type-->
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
			{help}
		/>
  </div>
		<div class="grow w-1/4">
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
				{help}
			/>
		{/if}
		</div>
	</div>
	<!--Description-->

		<!--Unit-->
		<div class="w-1/2">
		
		</div>

		<!--Missing Values-->
		<div id="missingvaluesContainer" on:mouseover={() => {
			helpStore.show('missingvaluesContainer');
		}}>
   <MissingValues bind:list={variable.missingValues}></MissingValues>
	 </div>

		<div class="py-5 text-right col-span-2">
			<!-- svelte-ignore a11y-mouse-events-have-key-events -->
			<button
				type="button"
				class="btn variant-filled-warning h-9 w-16 shadow-md"
				title="Cancel"
				id="cancel"
				on:click={() => cancel()}><Fa icon={faXmark} /></button
			>
			<!-- svelte-ignore a11y-mouse-events-have-key-events -->
			<button
				type="submit"
				class="btn variant-filled-primary h-9 w-16 shadow-md"
				title="Save Variable Template, {variable.name}"
				id="save">
				<Fa icon={faSave} /></button>
				
		</div>

</div>

</form>
