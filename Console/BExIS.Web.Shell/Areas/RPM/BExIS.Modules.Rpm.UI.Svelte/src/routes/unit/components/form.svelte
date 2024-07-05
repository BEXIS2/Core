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
		UnitValidationResult,
		LinkItem,
		ListItem
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

	let toggle = {dataTypes: false};

	let el: LinkItem[] =[]; 
	let dt: DataTypeListItem[];
	let ms: string[];
	let ds: DimensionListItem[] = [];	
	let linkUrl: URL | null = null;
	let dimensionlistItem: ListItem | undefined;
	let linklistItem: ListItem | undefined;
	setListItems()

	$: dataTypes = dt;
	$: measurementSystems = ms;
	$: dimensions = ds.map(({ id, name }) => ({ id: id, text: name }));
	$: externalLinks = el.map(({ id, name }) => ({ id: id, text: name }));
	$: unit.dimension = dimensionlistItem === undefined ? undefined : ds.find((d) => d.id === dimensionlistItem?.id);
	$: unit.link = linklistItem === undefined ? undefined : el.find((e) => e.id === linklistItem?.id);
	$: linkUrl = unit.link === undefined ? null : converttoURL (unit.link.uri);
	$: unit.link, unit.description, setListItems(), setValidation(); 

	onMount(async () => {
		setListItems()
		setValidation();
	});

	async function load() {
		ms = await apiCalls.GetMeasurementSystems();
		dt = await apiCalls.GetDataTypes();
		ds = await apiCalls.GetDimensions();
		el = await apiCalls.GetLinks();
	}

	function setValidation() {
		if (measurementSystems != undefined && measurementSystems.length > 0) {
			if (unit.id == 0) {
				suite.reset();
			} else {
				setTimeout(async () => {
					res = suite({ unit: unit, units: units, measurementSystems: measurementSystems }, undefined);
				}, 100);
			}
		}
	}

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: any) {
		//console.log("input changed", e)
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			// check changed field
			res = suite(
				{ unit: unit, units: units, measurementSystems: measurementSystems },
				e.target.id
			);
		}, 100);
	}

	async function submit() {
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

	function converttoURL(uri:string): URL | null
	{
		try{
			return new URL(uri,undefined );
		}
		catch
		{
			return null
		}
	}

	function setListItems()
	{
		if(unit)
		{
			if(unit.dimension)
			{
				dimensionlistItem = { id: unit.dimension.id, text: unit.dimension.name };
				console.log('dimensionlistItem', dimensionlistItem);
			}
			if (unit.link)
			{
				linklistItem = { id: unit.link.id, text: unit.link.name };
				console.log('linklistItem', linklistItem);
			}
		}
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
										<!-- svelte-ignore a11y-no-static-element-interactions -->
										<div
											transition:fade
											class="badge variant-filled-tertiary rounded-xl shadow-md w-7 h-7"
											title="Show datatypes details"
											id="create"
											on:mouseover={() => {
												helpStore.show('datatypesDetails');
											}}
											on:click={() => {
												toggle.dataTypes = !toggle.dataTypes;
											}}
										>
											{#if toggle.dataTypes}<Fa icon={faChevronUp} />{:else}<Fa
													icon={faChevronDown}
												/>{/if}
										</div>
									</div>
								</div>
							{/if}
							{#if toggle.dataTypes || unit.datatypes.length === 1}
								<div class="table-container my-2 w-full" transition:slide>
									<table class="table table-compact bg-tertiary-500/30">
										<tr class="bg-primary-300">
											<th class="text-left px-1">Name</th>
											<th class="text-left px-1">System Type</th>
											<th class="text-left px-1">Description</th>
										</tr>
										{#each unit.datatypes as datatype}
											<tr>
												<td class="text-left px-1">{datatype.name}</td>
												<td class="text-left px-1">{datatype.systemType}</td>
												<td class="text-left px-1">{datatype.description}</td>
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
						bind:target={dimensionlistItem}
						source={dimensions}
						required={true}
						complexTarget={true}
						help={true}
					/>
					{#if unit.dimension != undefined && unit.dimension != null}
						<div class="card p-2 my-1 shadow-md" transition:slide>
							<table class="table table-compact bg-tertiary-500/30">
								<tr class="bg-primary-300">
									<th class="text-left px-1">Name</th>
									<th class="text-left px-1">Specification</th>
									<th class="text-left px-1">Description</th>
								</tr>
								<tr>
									<td class="text-left px-1">{unit.dimension.name}</td>
									<td class="text-left px-1">{unit.dimension.specification}</td>
									<td class="text-left px-1">{unit.dimension.description}</td>
								</tr>
							</table>
						</div>
					{/if}
				</div>

				<!-- svelte-ignore a11y-mouse-events-have-key-events -->
				<!-- svelte-ignore a11y-no-static-element-interactions -->
				<div
					class="pb-3"
					title="Measurement System"
					on:mouseover={() => {
						helpStore.show('measurementSystem');
					}}
				>
					<label for={unit.measurementSystem} class="text-sm">Measurement System</label>
					<RadioGroup help={true} id="measurementSystems">
						{#each measurementSystems as item}
							<RadioItem
								on:change={() => {
									setTimeout(async () => {
										res = suite(
											{ unit: unit, units: units, measurementSystems: measurementSystems },
											undefined
										);
									}, 100);
								}}
								bind:group={unit.measurementSystem}
								name={item}
								id={item}
								value={item}>{item}</RadioItem
							>
						{/each}
					</RadioGroup>
				</div>

				<div class="pb-3" title="External Links">
					<DropdownKVP
						id="externalLinks"
						title="External Links"
						bind:target={linklistItem}
						source={externalLinks}
						complexTarget={true}
						help={true}
						on:change={() => {
							setTimeout(async () => {
								res = suite(
									{ unit: unit, units: units, measurementSystems: measurementSystems },
									undefined
								);
							}, 100);
						}}
					/>
					{#if unit.link != undefined && unit.link != null}
					<div class="card p-2 my-1 shadow-md" transition:slide>
						<table class="table table-compact bg-tertiary-500/30">
							<tr class="bg-primary-300">
								<th class="text-left px-1">Name</th>
								<th class="text-left px-1">Link</th>
							</tr>
							<tr>
								<td class="text-left px-1">{unit.link.name}</td>
								{#if linkUrl != null && (linkUrl.protocol == 'http:' ||  linkUrl.protocol == 'https:')}
									<!-- svelte-ignore a11y-missing-content -->
									<td class="text-left px-1"><a href="{unit.link.uri}" target="_blank">{unit.link.uri}</a></td>
								{:else}
									<td class="text-left px-1">{unit.link.uri}</td>
								{/if}
							</tr>
						</table>
					</div>
					{/if}
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
