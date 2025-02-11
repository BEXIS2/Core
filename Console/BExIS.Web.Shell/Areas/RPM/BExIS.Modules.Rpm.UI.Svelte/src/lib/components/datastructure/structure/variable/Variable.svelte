<script lang="ts">
	import { onMount, afterUpdate, createEventDispatcher } from 'svelte';
	import { get } from 'svelte/store';

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

	import {
		unitStore,
		dataTypeStore,
		templateStore,
		meaningsStore,
		constraintsStore,
		setByTemplateStore
	} from '../../store';

	import { updateDisplayPattern, updateDatatypes, updateUnits, updateTemplates } from './helper';

	import DataTypeDescription from './DataTypeDescription.svelte';
	import Container from './Container.svelte';
	import Header from './Header.svelte';
	import Overview from './Overview.svelte';

	import suite from './variable';
	import ConstraintsDescription from './ConstraintsDescription.svelte';
	import MeaningsDescription from './MeaningsDescription.svelte';
	import Status from './Status.svelte';
	import MissingValues from '../../MissingValues.svelte';

	export let variable: VariableInstanceModel = new VariableInstanceModel();
	$: variable;
	export let data: string[] = [];
	$: data;

	export let index: number;

	let datatypes: listItemType[] = [];
	$: datatypes;
	let units: unitListItemType[] = [];
	$: units;
	let variableTemplates: templateListItemType[] = [];
	$: variableTemplates;

	let meanings: listItemType[] = [];
	$: meanings;

	let constraints: listItemType[] = [];
	$: constraints;

	let suggestedDataType: listItemType | undefined;
	let suggestedUnits: unitListItemType[] | undefined;
	let suggestedTemplates: templateListItemType[];

	export let missingValues: missingValueType[];
	export let isValid: boolean = false;
	export let last: boolean = false;
	export let expand: boolean;
	export let blockDataRelevant: boolean = false;

	$: isValid;
	// validation
	let res = suite.get();

	let loaded = false;

	//displaypattern
	let displayPattern: listItemType[];
	$: displayPattern;

	const dispatch = createEventDispatcher();
	const setByTemplate = get(setByTemplateStore);

	let x: listItemType = { id: 0, text: '', group: '', description: '' };

	onMount(() => {
		// set suggestions
		setList();
		suggestedDataType = variable.dataType;
		suggestedUnits = variable.possibleUnits;
		suggestedTemplates = variable.possibleTemplates;
		console.log('🚀 ~ onMount ~ variable:', variable);

		// reset & reload validation
		suite.reset();

		setTimeout(async () => {
			updateLists();

			//displayPattern = updateDisplayPattern(variable.dataType, true);

			// when type has change, reset value, but after copy do not reset
			// thats why reset need to set

			if (displayPattern.length > 0) {
				res = suite(variable, 'displayPattern');
			}

			//res = suite(variable,"");
			setValidationState(res);

			//console.log(variable);

			if (variable.id > 0) {
				res = suite(variable, '');
			} // run validation only if start with an existing

			loaded = true;
		}, 10);
	});

	afterUpdate(() => {
		displayPattern = updateDisplayPattern(variable.dataType, false);
		res = suite(variable, '');
		setValidationState(res);
	});

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: any) {
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			res = suite(variable, e.target.id);
			setValidationState(res);
		}, 100);
	}

	//change event: if select change check also validation only on the field
	// *** is the id of the input component
	function onSelectHandler(e: any, id: string) {
		setTimeout(async () => {
			res = suite(variable, id);

			// update display patter and reset it if it changed

			if (id.includes('dataType')) {
				updateDisplayPattern(variable.dataType);
			}

			console.log(id, e.detail, variable);
			if (id.includes('variableTemplate')) {

				if (setByTemplate) {
					// if true, update unit & datatype based on settings
					if (variable.dataType == undefined || variable.dataType == '') {
						variable.dataType = updateDataType(e.detail);
					}

					if (variable.unit == undefined || variable.unit == '') {
						variable.unit = updateUnit(e.detail);
					}

					if (variable.description == undefined || variable.description == '') {
						variable.description = e.detail.description;
					}
				}

				variable.meanings = updateMeanings(variable, e.detail);
				variable.constraints = updateConstraints(variable, e.detail?.constraints);
			}

			if (id.includes('meanings')) {
				var last = e.detail[e.detail.length - 1];
				variable.constraints = updateConstraints(variable, last.constraints);
			}
			// console.log("🚀 ~ update var ~ variable:", variable)

			setValidationState(res);

			updateLists();
		}, 100);
			console.log("🚀 ~ setTimeout ~ e.detail:", e.detail)
	}

	// use the store to reset the lists for the dropdowns
	/// reset means mostly reset the groups
	function setList() {
		datatypes = $dataTypeStore.map((o) => ({ ...o })).sort(); // set datatypes
		units = $unitStore.map((o) => ({ ...o })).sort(); // set units
		variableTemplates = $templateStore.map((o) => ({ ...o })).sort();
		meanings = $meaningsStore.map((o) => ({ ...o })).sort();
		constraints = $constraintsStore.map((o) => ({ ...o })).sort();
	}

	function updateLists() {
		//console.log("variable",variable);
		datatypes = updateDatatypes(
			variable.unit,
			variable.template,
			$dataTypeStore,
			suggestedDataType,
			$unitStore
		);

		//console.log("updated datatypes",datatypes);

		// units based on Datatype selection
		units = updateUnits(variable.dataType, variable.template, $unitStore, suggestedUnits);
		units.sort();

		//console.log("updated units",units);
		variableTemplates = updateTemplates(variable.unit, $templateStore, suggestedTemplates);
		variableTemplates.sort();
	}

	function updateUnit(_variableTemplate: templateListItemType): unitListItemType | undefined {
		if (_variableTemplate.units) {
			var firstUnit = _variableTemplate.unit;
			var us = [...$unitStore.filter((u) => u.text == firstUnit)];
			if (us != undefined) {
				var u = us[0];
				return u;
			}
		}

		return undefined;
	}

	function updateDataType(_variableTemplate: templateListItemType): listItemType | undefined {
		if (_variableTemplate.units) {
			var ds = [...$dataTypeStore.filter((d) => _variableTemplate.dataType == d.text)];
			if (ds != undefined) {
				return ds[0];
			}
		}

		return undefined;
	}

	function updateMeanings(
		_variable: VariableInstanceModel,
		_variableTemplate: templateListItemType
	): listItemType[] {
		if (_variableTemplate && _variableTemplate.meanings) {
			if (_variable.meanings) {
				return [
					..._variable.meanings,
					...$meaningsStore.filter((m) => _variableTemplate.meanings.includes(m.text))
				];
			} else {
				return [...$meaningsStore.filter((m) => _variableTemplate.meanings.includes(m.text))];
			}
		}

		return [];
	}

	function updateConstraints(
		_variable: VariableInstanceModel,
		constraints: string[]
	): listItemType[] {
		if (constraints) {
			// get names of ids
			if (_variable.constraints) {
				return [
					..._variable.constraints,
					...$constraintsStore.filter((m) => constraints.includes(m.text))
				];
			} else {
				return [...$constraintsStore.filter((m) => constraints.includes(m.text))];
			}
		}

		return [];
	}

	function setValidationState(res: any) {
		isValid = res.isValid();
		// dispatch this event to the parent to check the save button
		dispatch('var-change');
	}

	function cutData(d: any) {

		console.log("🚀 ~ cutData ~ d:", d)

		if(d != undefined	&& d != null && d[0] !=	undefined && d[0] != null)
		{
			 console.log("🚀 ~ cutData in if ", d)
				for (let index = 0; index < d.length; index++) {
					let v = d[index];

					if (v.length > 10) {
						d[index] = v.slice(0, 10) + '...';
					}
				}
				return d;
			}
		return [];
	}
</script>

<div id="variable-{variable.id}-container" class="flex gap-5">
	{#if loaded && variable && $templateStore}
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
							{blockDataRelevant}
						>
							<Container name="Title">
								<div class="flex" slot="property">
									<div class="grow">
										<TextInput
											id="name-{index}"
											label="Name"
											bind:value={variable.name}
											on:input={onChangeHandler}
											valid={res.isValid('name')}
											invalid={res.hasErrors('name')}
											feedback={res.getErrors('name')}
											disabled={blockDataRelevant}
										/>
									</div>
									<Status {isValid}></Status>
								</div>

								<div slot="template">
									<div class="flex w-full gap-1 py-1">
										<div class="grow">Template (data is copied and changeable!)</div>
										{#each suggestedTemplates.slice(0, 3) as t}
											<button
												title="Click to select"
												class="badge"
												class:variant-filled-primary={t.text == variable.template?.text}
												class:variant-ghost-primary={t.text != variable.template?.text}
												on:click={() => (variable.template = t)}>{t.text}</button
											>
										{/each}
									</div>
									<MultiSelect
										id="variableTemplate-{index}"
										title="Variable Template"
										source={variableTemplates}
										itemId="id"
										itemLabel="text"
										itemGroup="group"
										complexSource={true}
										complexTarget={true}
										isMulti={false}
										clearable={true}
										bind:target={variable.template}
										placeholder="-- Please select --"
										invalid={res.hasErrors('variableTemplate') && !blockDataRelevant}
										feedback={res.getErrors('variableTemplate')}
										on:change={(e) => onSelectHandler(e, 'variableTemplate-' + index)}
										disabled={blockDataRelevant}
									/>
								</div>

								<div slot="description">...</div>
							</Container>
						</Header>
					</header>

					<section class="py-2 px-10">
						<!--Description-->
						<Container>
							<div slot="property">
								<TextArea
									id="description-{index}"
									label="Variable description"
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
						<Container name="DataType">
							<div slot="property">
								<MultiSelect
									id="dataType-{index}"
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
									clearable={true}
									on:change={(e) => onSelectHandler(e, `dataType-${index}`)}
									disabled={blockDataRelevant}
								/>
							</div>

							<div slot="displaypattern">
								<!--Show only when display pattern exists-->
								{#if displayPattern != undefined && displayPattern.length > 0}
									<MultiSelect
										id="displayPattern-{index}"
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
										on:change={(e) => onSelectHandler(e, `displayPattern-${index}`)}
									/>
								{/if}
							</div>

							<div slot="unit">
								<div class="flex w-full gap-1">
									<div class="grow">Unit</div>
									{#if suggestedUnits && suggestedUnits.length > 1}
										{#each suggestedUnits?.slice(0, 3) as u}
											<button
												class="badge"
												title="Click to select"
												class:variant-filled-primary={u?.text == variable.unit?.text}
												class:variant-ghost-primary={u?.text != variable.unit?.text}
												on:click={() => (variable.unit = u)}>{u.text}</button
											>
										{/each}
									{/if}
								</div>
								<MultiSelect
									id="unit-{index}"
									title=""
									source={units}
									itemId="id"
									itemLabel="text"
									itemGroup="group"
									complexSource={true}
									complexTarget={true}
									isMulti={false}
									clearable={true}
									bind:target={variable.unit}
									placeholder="-- Please select --"
									invalid={res.hasErrors('unit')}
									feedback={res.getErrors('unit')}
									on:change={(e) => onSelectHandler(e, `unit-${index}`)}
								/>
							</div>

							<div slot="description">
								{#if variable.dataType}
									<DataTypeDescription type={variable.dataType.text} {missingValues} />
								{/if}
							</div>
						</Container>

						<Container>
							<div slot="property">
								<div class="flex w-full gap-1 py-1">
									<!--<div class="grow">Meanings</div>-->
								</div>
								<MultiSelect
									id="meanings-{index}"
									source={meanings}
									title="Meanings"
									itemId="id"
									itemLabel="text"
									itemGroup="group"
									complexSource={true}
									complexTarget={true}
									isMulti={true}
									clearable={true}
									bind:target={variable.meanings}
									placeholder="-- Please select --"
									invalid={res.hasErrors('meanings')}
									feedback={res.getErrors('meanings')}
									on:change={(e) => onSelectHandler(e, `meanings-${index}`)}
								/>
							</div>
							<div slot="description">
								<MeaningsDescription bind:list={variable.meanings} />
							</div>
						</Container>

						<Container>
							<div slot="property">
								<div class="flex w-full gap-1 py-1">
									<div class="grow">Constraints</div>
								</div>
								<MultiSelect
									id="constraints-{index}"
									title=""
									source={constraints}
									itemId="id"
									itemLabel="text"
									itemGroup="group"
									complexSource={true}
									complexTarget={true}
									isMulti={true}
									clearable={true}
									bind:target={variable.constraints}
									placeholder="-- Please select --"
									invalid={res.hasErrors('constraints')}
									feedback={res.getErrors('constraints')}
									on:change={(e) => onSelectHandler(e, `constraints-${index}`)}
									disabled={blockDataRelevant}
								/>
							</div>
							<div slot="description">
								<ConstraintsDescription bind:list={variable.constraints} />
							</div>
						</Container>
						<Container>
							<div slot="property">
								<div class="flex w-full gap-1 py-1">
									<div class="grow">Missing Values</div>
								</div>
								<MissingValues
									bind:list={variable.missingValues}
									showTitle={false}
									disabled={blockDataRelevant}
								></MissingValues>
							</div>
							<div slot="description"></div>
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
				>
					<TextInput
						id="name-{index}"
						label="Name"
						bind:value={variable.name}
						on:input={onChangeHandler}
						valid={res.isValid('name')}
						invalid={res.hasErrors('name')}
						feedback={res.getErrors('name')}
						disabled={blockDataRelevant}
					/>
				</Overview>
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
