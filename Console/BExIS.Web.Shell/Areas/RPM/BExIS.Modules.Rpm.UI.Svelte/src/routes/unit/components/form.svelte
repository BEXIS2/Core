<script lang="ts">
	import {
		TextInput,
		TextArea,
		MultiSelect,
		DropdownKVP,
		ErrorMessage,
		helpStore,
		notificationStore,
		notificationType
	} from '@bexis2/bexis2-core-ui';
	import { RadioGroup, RadioItem } from '@skeletonlabs/skeleton';

	import Fa from 'svelte-fa';
	import { faXmark, faSave, faChevronUp, faChevronDown } from '@fortawesome/free-solid-svg-icons';

	import { onMount } from 'svelte';
	import * as apiCalls from '../services/apiCalls';

	import type {
		UnitListItem,
		DataTypeListItem,
		DimensionListItem,
		UnitValidationResult
	} from '../models';

	// event
	import { createEventDispatcher } from 'svelte';
	const dispatch = createEventDispatcher();

	// validation
	import suite from './form';
	import { fade, slide } from 'svelte/transition';

	// load form result object
	let res = suite.get();

	// use to actived save if form is valid
	$: disabled = !res.isValid();

	// init unit
	export let unit: UnitListItem;
	export let units: UnitListItem[];

	let showDatatypes: boolean = false;
	let dt: DataTypeListItem[];
	let ms: string[];
	let ds: DimensionListItem[] = [];
	let listItem =
		unit === undefined || unit.dimension === undefined
			? undefined
			: { id: unit.dimension.id, text: unit.dimension.name };
	$: dataTypes = dt;
	$: measurementSystems = ms;
	$: dimensions = ds.map(({ id, name }) => ({ id: id, text: name }));
	$: unit.dimension = listItem === undefined ? undefined : ds.find((d) => d.id === listItem?.id);

	onMount(async () => {
		if (unit.id == 0) {
			suite.reset();
		}
		else{
			setTimeout(async () => {	
				res = suite({ unit: unit, units: units }, "");
			}, 10);
		}

	});

	async function load() {
		dt = await apiCalls.GetDataTypes();
		ms = await apiCalls.GetMeasurementSystems();
		ds = await apiCalls.GetDimensions();
	}

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: any) {
		//console.log("input changed", e)
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			// check changed field
			res = suite({ unit: unit, units: units }, e.target.id);
		}, 10);
	}

	async function submit() {
		console.log("🚀 ~ submit ~ unit:", unit)
		let result: UnitValidationResult = await apiCalls.EditUnit(unit);
		let message: string;
		if (result.validationResult.isValid != true) {
			message = "Can't save Unit";
			if (result.unitListItem.name != '' && result.unitListItem.abbreviation != '') {
				message +=
					' "' + result.unitListItem.name + '" (' + result.unitListItem.abbreviation + ').';
			}
			if (
				result.validationResult.validationItems != undefined &&
				result.validationResult.validationItems.length > 0
			) {
				result.validationResult.validationItems.forEach((validationItem) => {
					message += '<li>' + validationItem.message + '</li>';
				});
			}
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: message
			});
		} else {
			message =
				'Unit "' + result.unitListItem.name + '" (' + result.unitListItem.abbreviation + ') saved.';
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: message
			});
			suite.reset();
			dispatch('save');
		}
	}

	function cancel() {
		suite.reset();
		dispatch('cancel');
	}

	function toggleDatatypes() {
		showDatatypes = !showDatatypes;
	}
</script>

<div class="card p-5 shadow-md my-3">
	{#await load()}
		<div class="grid grid-cols-2 gap-5">
			<div class="pb-3">
				<div class="h-9 placeholder animate-pulse" />
			</div>
			<div class="pb-3">
				<div class="h-9 placeholder animate-pulse" />
			</div>
			<div class="pb-3 col-span-2">
				<div class="h-14 placeholder animate-pulse" />
			</div>
			<div class="pb-3">
				<div class="h-9 placeholder animate-pulse" />
			</div>
			<div class="pb-3">
				<div class="h-9 placeholder animate-pulse" />
			</div>

			<div class="pb-3 col-span-2">
				<div class="h-9 placeholder animate-pulse" />
			</div>

			<div class="py-5 flex justify-end col-span-2">
				<div class="placeholder animate-pulse h-9 w-16" />
				<div class="placeholder animate-pulse h-9 w-16" />
			</div>
		</div>
	{:then}
		<form on:submit|preventDefault={submit}>
			<div class="grid grid-cols-2 gap-5">
				<div class="pb-3" title="Name">
					<TextInput
						id="name"
						label="Name"
						help={true}
						required={true}
						bind:value={unit.name}
						on:input={onChangeHandler}
						valid={res.isValid('name')}
						invalid={res.hasErrors('name')}
						feedback={res.getErrors('name')}
					/>
				</div>
				<!-- svelte-ignore a11y-mouse-events-have-key-events -->
				<div class="pb-3" title="Abbreviation">
					<TextInput
						id="abbreviation"
						label="Abbreviation"
						help={true}
						required={true}
						bind:value={unit.abbreviation}
						on:input={onChangeHandler}
						valid={res.isValid('abbreviation')}
						invalid={res.hasErrors('abbreviation')}
						feedback={res.getErrors('abbreviation')}
					/>
				</div>
				<!-- svelte-ignore a11y-mouse-events-have-key-events -->
				<div class="pb-3 col-span-2" title="Description">
					<TextArea
						id="description"
						label="Description"
						help={true}
						required={true}
						bind:value={unit.description}
						on:input={onChangeHandler}
						valid={res.isValid('description')}
						invalid={res.hasErrors('description')}
						feedback={res.getErrors('description')}
					/>
				</div>

				<div class="pb-3" title="Data Types">
					<MultiSelect
						title="Data Types"
						id="dataTypes"
						bind:target={unit.datatypes}
						source={dataTypes}
						itemId="id"
						itemLabel="name"
						complexSource={true}
						complexTarget={true}
						required={true}
						help={true}
					/>
					{#if unit.datatypes != undefined && unit.datatypes != null && unit.datatypes.length > 0}
						<div class="card p-2 my-1 shadow-md" transition:slide>
							{#if unit.datatypes.length > 1}
								<div class="grid grid-cols-2 gap-5 pb-1" transition:slide>
									<div class="h4">Details</div>
									<div class="text-right">
										<!-- svelte-ignore a11y-click-events-have-key-events -->
										<!-- svelte-ignore a11y-mouse-events-have-key-events -->
										<div
											transition:fade
											class="badge variant-filled-tertiary rounded-xl shadow-md w-7 h-7"
											title="Show datatypes details"
											id="create"
											on:mouseover={() => {
												helpStore.show('datatypesDetails');
											}}
											on:click={() => toggleDatatypes()}
										>
											{#if showDatatypes}<Fa icon={faChevronUp} />{:else}<Fa
													icon={faChevronDown}
												/>{/if}
										</div>
									</div>
								</div>
							{/if}
							{#if showDatatypes || unit.datatypes.length === 1}
								<div class="table-container my-2 w-full" transition:slide>
									<table class="table table-compact bg-tertiary-200">
										<tr class="bg-primary-300">
											<th class="text-left">Name</th>
											<th class="text-left">System Type</th>
											<th class="text-left">Description</th>
										</tr>
										{#each unit.datatypes as datatype}
											<tr>
												<td>{datatype.systemType}</td>
												<td>{datatype.name}</td>
												<td>{datatype.description}</td>
											</tr>
										{/each}
									</table>
								</div>
							{/if}
						</div>
					{/if}
				</div>
				<div class="pb-3" title="Dimension">
					<DropdownKVP
						id="dimension"
						title="Dimension"
						bind:target={listItem}
						source={dimensions}
						required={true}
						complexTarget={true}
						help={true}
					/>
					{#if unit.dimension != undefined && unit.dimension != null}
						<div class="card p-2 my-1 shadow-md" transition:slide>
							<table class="table table-compact bg-tertiary-200">
								<tr class="bg-primary-300">
									<th class="text-left">Name</th>
									<th class="text-left">Specification</th>
									<th class="text-left">Description</th>
								</tr>
								<tr>
									<td>{unit.dimension.name}</td>
									<td>{unit.dimension.specification}</td>
									<td>{unit.dimension.description}</td>
								</tr>
							</table>
						</div>
					{/if}
				</div>

				<!-- svelte-ignore a11y-mouse-events-have-key-events -->
				<div
					class="pb-3"
					title="Measurement System"
					on:mouseover={() => {
						helpStore.show('measurementSystem');
					}}
				>
					<label for={unit.measurementSystem} class="text-sm">Measurement System</label>
					<RadioGroup help={true} id="test">
						{#each measurementSystems as item, i}
							<RadioItem
								bind:group={unit.measurementSystem}
								name="measurementSystem"
								id="measurementSystem+{i}"
								value={item}>{item}</RadioItem
							>
						{/each}
					</RadioGroup>
				</div>

				<div class="py-5 text-right col-span-2">
					<!-- svelte-ignore a11y-mouse-events-have-key-events -->
					<button
						type="button"
						class="btn variant-filled-warning h-9 w-16 shadow-md"
						title="Cancel"
						id="cancel"
						on:mouseover={() => {
							helpStore.show('cancel');
						}}
						on:click={() => cancel()}><Fa icon={faXmark} /></button
					>
					<!-- svelte-ignore a11y-mouse-events-have-key-events -->
					<button
						type="submit"
						class="btn variant-filled-primary h-9 w-16 shadow-md"
						title="Save Unit, {unit.name}"
						id="save"
						{disabled}
						on:mouseover={() => {
							helpStore.show('save');
						}}><Fa icon={faSave} /></button
					>
				</div>
			</div>
		</form>
	{:catch error}
		<div class="flex justify-center">
			<ErrorMessage {error} />
		</div>
	{/await}
</div>
