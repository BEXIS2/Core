<script>
	import Fa from 'svelte-fa';
	import { faXmark, faAdd } from '@fortawesome/free-solid-svg-icons';

	import { onMount } from 'svelte';
	import { getCreate, create } from './services';

	// ui components
	import { Spinner, positionType } from '@bexis2/bexis2-core-ui';
	import InputEntry from './InputEntry.svelte';
	import ContentContainer from '$lib/components/ContentContainer.svelte';

	export let id;
	let titleField;
	let descriptionField;
	let metadataFields = [];

	// events
	import { createEventDispatcher } from 'svelte';
	const dispatch = createEventDispatcher();

	//validation
	import suite from './form';
	let result = suite.get();
	$: disabled = !result.isValid();

	let model = null;
	let onSaving = false;

	onMount(async () => {
		console.log('form');
		const res = await getCreate(id);
		console.log('res', res);
		if (res != false) model = res;

		filterInputs();
		suite.reset();

		console.log();
		disabled = model.inputFields.length == 0 ? false : true;
	});

	async function handleSubmit() {
		// check if form is valid
		if (result.isValid() || metadataFields.length === 0) {
			onSaving = true;
			console.log('before submit', model);
			const res = await create(model);
			if (res.success != false) {
				console.log('save', res);
				dispatch('save', res.id);
			}

			onSaving = false;
		}
	}

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e) {
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			result = suite(model.inputFields, e.target.id);
		}, 10);
	}

	function onCancel() {
		suite.reset();
		dispatch('cancel');
	}

	// set title and description and remove from other fields
	// because of getting a good form structure
	function filterInputs() {
		for (let index = 0; index < model.inputFields.length; index++) {
			const element = model.inputFields[index];
			if (element.name.toLowerCase() == 'title') {
				titleField = element;
			} else if (element.name.toLowerCase() == 'description') {
				descriptionField = element;
			} else {
				metadataFields = [...metadataFields, element];
			}
		}
	}
</script>

<div>
	{#if model}
		<ContentContainer>
			<div class="pb-10">
				<h2 class="h2">New {model.name}</h2>
				<p class="pt-5">{model.description}</p>
			</div>

			<form on:submit|preventDefault={handleSubmit}>
				{#if titleField}
					<InputEntry
						label={titleField.name}
						required={true}
						type={titleField.type}
						bind:value={titleField.value}
						valid={result.isValid(titleField.name)}
						invalid={result.hasErrors(titleField.name)}
						feedback={result.getErrors(titleField.name)}
						on:input={onChangeHandler}
					/>
				{/if}
				{#if descriptionField}
					<InputEntry
						label={descriptionField.name}
						type="Text"
						required={true}
						bind:value={descriptionField.value}
						valid={result.isValid(descriptionField.name)}
						invalid={result.hasErrors(descriptionField.name)}
						feedback={result.getErrors(descriptionField.name)}
						on:input={onChangeHandler}
					/>
				{/if}
				{#each metadataFields as item}
					<InputEntry
						label={item.name}
						type={item.type}
						bind:value={item.value}
						required={true}
						valid={result.isValid(item.name)}
						invalid={result.hasErrors(item.name)}
						feedback={result.getErrors(item.name)}
						on:input={onChangeHandler}
					/>
				{/each}

				{#if model.datastructures && model.datastructures.length > 0}
					<!--data structures exist-->
					<b>Usable structures</b>
					<ul>
						{#each model.datastructures as item}
							<li>{item}</li>
						{/each}
					</ul>
				{/if}

				{#if model.fileTypes && model.fileTypes.length > 0}
					<!--file types exist-->
					<b>Supported file types</b>
					<ul>
						{#each model.fileTypes as item}
							<li>{item}</li>
						{/each}
					</ul>
				{/if}

				<div class=" flex gap-1 pt-5 text-right">
					<div class="grow text-xs">
						{#if onSaving}
							<Spinner position={positionType.end} />
						{/if}
					</div>
					<button
						title="cancel"
						type="button"
						class="btn variant-filled-warning"
						on:click={onCancel}><Fa icon={faXmark} /></button
					>

					<button title="create" type="submit" class="btn variant-filled-primary" {disabled}
						><Fa icon={faAdd} /></button
					>
				</div>
			</form>
		</ContentContainer>
	{:else}
		<!-- while data is not loaded show a loading information -->
		<Spinner />
	{/if}
</div>
