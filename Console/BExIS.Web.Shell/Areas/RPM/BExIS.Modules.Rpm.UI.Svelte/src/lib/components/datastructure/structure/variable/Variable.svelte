<script lang="ts">
	import { onMount, afterUpdate, createEventDispatcher } from 'svelte';

	// UI Components
	import { TextInput, TextArea, MultiSelect } from '@bexis2/bexis2-core-ui';

	//types
	import type { listItemType } from '@bexis2/bexis2-core-ui';
	import {
		VariableInstanceModel,
		type missingValueType,
		type unitListItemType,
		type templateListItemType
	} from '../../types';

	//stores
	import { get } from 'svelte/store';
	import { displayPatternStore, unitStore, dataTypeStore, templateStore } from '../../store';

	// icons
	import Fa from 'svelte-fa';
	import {
		faAdd,
		faTrash,
		faAngleUp,
		faAngleDown,
		faCopy
	} from '@fortawesome/free-solid-svg-icons';

	import {updateDisplayPattern, updateGroup} from './helper'

	import DataTypeDescription from './DataTypeDescription.svelte';
	import Container from './Container.svelte';
	import Header from './Header.svelte';
	import Overview from './Overview.svelte';

	import suite from './variable';

	export let variable: VariableInstanceModel = new VariableInstanceModel();
	$: variable;
	export let data: string[];
	$: data;

	export let index: number;

	let datatypes: listItemType[] = []
	$: datatypes;
	let units: unitListItemType[] = [];
	$: units;
	let variableTemplates: templateListItemType[] = [];
	$: variableTemplates;

	export let missingValues: missingValueType[];

	export let isValid: boolean = false;
	export let last: boolean = false;

	export let expand: boolean;

	$: isValid;
	// validation
	let res = suite.get();

	let loaded = false;

	//displaypattern
	let displayPattern: listItemType[];
	$: displayPattern;


	const dispatch = createEventDispatcher();

	onMount(() => {

		// set lists
		datatypes = [...$dataTypeStore]
		units = [...$unitStore]
		variableTemplates = [...$templateStore]

		loaded = true;
		// reset & reload validation
		suite.reset();

		setTimeout(async () => {

			displayPattern = updateDisplayPattern(variable.dataType);

		 // when type has change, reset value, but after copy do not reset
		 // thats why reset need to set
			variable.displayPattern = undefined;

			if (displayPattern.length > 0) {
				res = suite(variable, 'displayPattern');
			}

			res = suite(variable);
			setValidationState(res);

			console.log(variable);

			if (variable.id > 0) {
				res = suite(variable, '');
			} // run validation only if start with an existing
		
		}, 10);

	});

	afterUpdate(() => {
		displayPattern = updateDisplayPattern(variable.dataType, false);
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

			updateLists();
		}, 100);
	}

	function setValidationState(res) {
		isValid = res.isValid();
		// dispatch this event to the parent to check the save button
		dispatch('var-change');
	}

	function cutData(d) {
		for (let index = 0; index < d.length; index++) {
			let v = d[index];

			if (v.length > 10) {
				d[index] = v.slice(0, 10) + '...';
			}
		}

		return d;
	}

	function updateLists() {
		console.log('filter lists');


		// datatypes based on unit selection
		datatypes = updateDatatypes(variable.unit, variable.template);
		//console.log("updated datatypes",datatypes);

		// // units based on Datatype selection
		units = updateUnits(variable.dataType, variable.template);
		// //console.log("updated units",units);

		variableTemplates = updateTemplates(variable.unit);
	}

	function updateDatatypes(
		unit: unitListItemType | undefined,
		template: templateListItemType | undefined
	) {
		//console.log("-->", unit.text,unit.dataTypes);
		let dts = $dataTypeStore.map(o=>({...o}));

		let matchPhrase = '';

		let othersText = 'other';

		if (unit != null && unit != undefined && unit.dataTypes.length > 0) {
			// if unit exist
			matchPhrase = unit?.text;

			for (let index = 0; index < dts.length; index++) {
				const datatype = dts[index];
				if (unit.dataTypes.includes(datatype.text) && !datatype.group.includes(matchPhrase)) {
					datatype.group = updateGroup(datatype.group, matchPhrase);
				}
			}
		}

		// check templates
		if (template && template.units) {
			matchPhrase = template.text;

			for (let index = 0; index < template.units.length; index++) {
				// each unit in a template
				const u = units.filter((u) => u.text == template.units[index])[0];
				// console.log("t-unit",u);
				
				for (let index = 0; index < dts.length; index++) {
					// each datatype
					const datatype = dts[index];
					if (u.dataTypes.includes(datatype.text) && !u.group.includes(matchPhrase)) {
						// console.log(matchPhrase,datatype.text, u.text);
						
						datatype.group = updateGroup(datatype.group, matchPhrase);
					}
				}
			}
		}

		// reorder
		return  [
			...dts.filter((d) => d.group != othersText),
			...dts.filter((d) => d.group == othersText)
		];
	}

	function updateUnits(
		datatype: listItemType | undefined,
		template: templateListItemType | undefined
	) {

		let _units = $unitStore.map(o=>({...o}));

		let matchPhrase = '';
		let othersText = 'other';

		if (datatype && _units) {
			matchPhrase = datatype?.text;
			// if datatype and units exist
			_units.forEach((unit) => {
				if (unit.dataTypes.includes(datatype.text) == true) {
					unit.group = matchPhrase;
				}
			});
		}

		// filter units based on template matches
		if (template && template.units) {
			for (let index = 0; index < template.units.length; index++) {
				const u = template.units[index];
				matchPhrase = template?.text;
				_units.forEach((unit) => {
					if (unit.text == u) {
						unit.group = updateGroup(unit.group, matchPhrase);
					}
				});
			}
		}

		return [
			..._units.filter((d) => d.group != othersText),
			..._units.filter((d) => d.group == othersText)
		];
	}

	function updateTemplates(unit: unitListItemType | undefined) {
		let _templates =  $templateStore.map(o=>({...o}));
		let matchPhrase = '' + unit?.text;
		let othersText = 'other';

		if (unit && _templates) {
			// if datatype and units exist
			_templates.forEach((template) => {
				template.group = template.units.includes(unit.text) == true ? matchPhrase : othersText;
			});
			return [
				..._templates.filter((d) => d.group != othersText),
				..._templates.filter((d) => d.group == othersText)
			];
		}
	}

</script>

<div id="variable-{variable.id}-container" class="flex gap-5">
	{#if loaded && variable}
		<div id="variable-{variable.id}-container-info" class="grow">
			{#if expand}
				<div class="card">
					<header id="header_{index}" class="card-header">
						<Header
							{index}
							name={variable.name}
							bind:isKey={variable.isKey}
							bind:isOptional={variable.isOptional}
							bind:isValid
							bind:expand
						>
							<TextInput
								id="name"
								label="Name"
								bind:value={variable.name}
								on:input={onChangeHandler}
								valid={res.isValid('name')}
								invalid={res.hasErrors('name')}
								feedback={res.getErrors('name')}
							/>
						</Header>
					</header>

					<section class="py-2 px-10">
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
								show all information about the units in a table Nothing found? Make a new
								suggestion.
							</div>
						</Container>

						<!--Meaning-->
						<Container>
							<div slot="property">
								<MultiSelect
									id="variableTemplate"
									title="Template"
									source={variableTemplates}
									itemId="id"
									itemLabel="text"
									itemGroup="group"
									complexSource={true}
									complexTarget={true}
									isMulti={false}
									clearable={false}
									bind:target={variable.template}
									placeholder="-- Please select --"
									invalid={res.hasErrors('variableTemplate')}
									feedback={res.getErrors('variableTemplate')}
									on:change={(e) => onSelectHandler(e, 'variableTemplate')}
								/>
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
					isOptional={variable.isOptional}
					isKey={variable.isKey}
					bind:expand
					{isValid}
					unit={variable.unit?.text}
					datatype={variable.dataType?.text}
					template={variable.template?.text}
					description={variable.description}
					datapreview={cutData(data).join(', ')}
				/>
			{/if}
		</div>
		<div
			id="variable-{variable.id}-container-options"
			class="flex-none w-24 space-y-2 content-center"
		>
			<slot name="list-options" />
		</div>
	{/if}
</div>
