<script lang="ts">
	import { TextInput, TextArea, MultiSelect, helpStore } from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';
	import { faSave, faXmark } from '@fortawesome/free-solid-svg-icons';

	import type { DimensionListItem, DimensionValidationResult } from '../models';
	import { onMount } from 'svelte';
	import * as apiCalls from '../services/apiCalls';

	//notifications
	import { notificationStore, notificationTypes } from '../../components/notifications';
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
	export let dimension: DimensionListItem;
	export let dimensions: DimensionListItem[];

	onMount(async () => {
		if (dimension.id == 0) {
			suite.reset();
		}
	});

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: any) {
		//console.log("input changed", e)
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			// check changed field
			res = suite({ dimension: dimension, dimensions: dimensions }, e.target.id);
		}, 10);
	}

	async function submit() {
		let result: DimensionValidationResult = await apiCalls.EditDimension(dimension);
		console.log('DimensionValidationResult', result);
		console.log('result.dimensionListItem.name', result.dimensionListItem.name);
		let message: string;
		if (result.validationResult.isValid != true) {
			message = "Can't save Dimension";
			if (result.dimensionListItem.name != '') {
				message += ' "' + result.dimensionListItem.name + '" .';
			}
			if (
				result.validationResult.validationItems != undefined &&
				result.validationResult.validationItems.length > 0
			) {
				result.validationResult.validationItems.forEach((validationItem) => {
					console.log('validationItem', validationItem);
					message += '<li>' + validationItem.message + '</li>';
				});
			}
			notificationStore.showNotification({ type: notificationTypes.error, message: message });
		} else {
			message = 'Unit "' + result.dimensionListItem.name + '" saved.';
			notificationStore.showNotification({ type: notificationTypes.success, message: message });
			suite.reset();
			dispatch('save');
		}
	}

	function cancel() {
		suite.reset();
		dispatch('cancel');
	}
</script>

{#if dimension}
	<form on:submit|preventDefault={submit}>
		<div class="grid grid-cols-2 gap-5">
			<div class="pb-3 col-span-2">
				<TextInput
					id="name"
					label="Name"
					help={true}
					required={true}
					bind:value={dimension.name}
					on:input={onChangeHandler}
					valid={res.isValid('name')}
					invalid={res.hasErrors('name')}
					feedback={res.getErrors('name')}
				/>
			</div>
			<div class="pb-3 col-span-2">
				<TextArea
					id="description"
					label="Description"
					help={true}
					required={true}
					bind:value={dimension.description}
					on:input={onChangeHandler}
					valid={res.isValid('description')}
					invalid={res.hasErrors('description')}
					feedback={res.getErrors('description')}
				/>
			</div>
			<div class="pb-3 col-span-2">
				<TextInput
					id="specification"
					label="Specification"
					help={true}
					required={true}
					bind:value={dimension.specification}
					on:input={onChangeHandler}
					valid={res.isValid('specification')}
					invalid={res.hasErrors('specification')}
					feedback={res.getErrors('specification')}
				/>
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
					title="Save Unit, {dimension.name}"
					id="save"
					{disabled}
					on:mouseover={() => {
						helpStore.show('save');
					}}><Fa icon={faSave} /></button
				>
			</div>
		</div>
	</form>
{/if}
