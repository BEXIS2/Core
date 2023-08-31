<script lang="ts">
	import { TextInput, TextArea, DropdownKVP, helpStore } from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';
	import { faSave, faXmark } from '@fortawesome/free-solid-svg-icons';

	import type { DataTypeListItem, DataTypeValidationResult} from '../models';
	import { onMount } from 'svelte';
	import * as apiCalls from '../services/apiCalls';
	import { fade, slide } from 'svelte/transition';

	//notifications
	import { notificationStore, notificationType } from '@bexis2/bexis2-core-ui';
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
	export let dataType: DataTypeListItem;
	export let dataTypes: DataTypeListItem[];
	
	let st: string[] = [];
	$: systemTypes = st.map((s) => ({ id: s, text: s }));
	$: console.log('System Types', systemTypes);
	

	onMount(async () => {
		st = await apiCalls.GetSystemTypes();
		if (dataType.id == 0) {
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
			res = suite({ dataType: dataType, dataTypes: dataTypes }, e.target.id);
		}, 10);
	}

	async function submit() {
		let result: DataTypeValidationResult = await apiCalls.EditDataType(dataType);
		let message: string;
		if (result.validationResult.isValid != true) {
			message = "Can't save Data Type";
			if (result.dataTypeListItem.name != '') {
				message += ' "' + result.dataTypeListItem.name + '" .';
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
			message = 'Unit "' + result.dataTypeListItem.name + '" saved.';
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
</script>

{#if dataType && systemTypes}
	<form on:submit|preventDefault={submit}>
		<div class="grid grid-cols-2 gap-5">
			<div class="pb-3">
				<TextInput
					id="name"
					label="Name"
					help={true}
					required={true}
					bind:value={dataType.name}
					on:input={onChangeHandler}
					valid={res.isValid('name')}
					invalid={res.hasErrors('name')}
					feedback={res.getErrors('name')}
				/>
			</div>
			<div class="pb-3" title="System Type">
				<DropdownKVP
					id="systemType"
					title="System Type"
					bind:target={dataType.systemType}
					source={systemTypes}
					required={true}
					complexTarget={false}
					help={true}
				/>
			</div>

			<div class="pb-3 col-span-2">
				<TextArea
					id="description"
					label="Description"
					help={true}
					required={true}
					bind:value={dataType.description}
					on:input={onChangeHandler}
					valid={res.isValid('description')}
					invalid={res.hasErrors('description')}
					feedback={res.getErrors('description')}
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
					title="Save Data Type, {dataType.name}"
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
