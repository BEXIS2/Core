<script lang="ts">
	import { SlideToggle } from '@skeletonlabs/skeleton';

	// services
	import { update, create } from './services';

	// icons
	import Fa from 'svelte-fa';
	import { faXmark, faSave } from '@fortawesome/free-solid-svg-icons';

	import suite from './meaning';
	import { onMount, createEventDispatcher } from 'svelte';
	import { TextArea, TextInput, helpStore, DropdownKVP, MultiSelect } from '@bexis2/bexis2-core-ui';

	// data
	import { helpInfoList } from './help';
	import MeaningEntries from './MeaningEntries.svelte';
	import type { MeaningModel } from '$lib/components/meaning/types';
	import { constraintsStore } from '$lib/components/meaning/stores';
	import ConstraintsDescription from '$lib/components/datastructure/structure/variable/ConstraintsDescription.svelte';

	export let meaning: MeaningModel;
	$: meaning;

	let help: boolean = true;

	let loading: boolean = false; // if true preloader of selection list unit and datatype will be displayed
	let loaded = false;

	// validation
	let res = suite.get();
	$: isValid = res.isValid();

	let linksValid: boolean = true;

	const dispatch = createEventDispatcher();

	onMount(() => {
		loaded = true;

		// set help
		helpStore.setHelpItemList(helpInfoList);

		console.log('meaning', meaning);

		// reset & reload validation
		suite.reset();

		// run validation only if start with an existing
		if (meaning.id == 0) {
			suite.reset();
		} else {
			setTimeout(async () => {
				res = suite(meaning, undefined);
			}, 10);
		}
	});

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e) {
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			res = suite(meaning, e.target.id);

			//console.log(res);
			//console.log(res.isValid());
		}, 100);
	}

	function cancel() {
		suite.reset();
		dispatch('cancel');
	}

	async function submit() {
		console.log('🚀 ~ file: Meaning.svelte:77 ~ submit ~ meaning:', meaning);
		var s = (await (meaning.id == 0)) ? create(meaning) : update(meaning);
		console.log('🚀 ~ file: Meaning.svelte:67 ~ submit ~ res:', res);

		if ((await s).status === 200) {
			dispatch('success');
		} else {
			dispatch('fail');
		}
	}
</script>

<form on:submit|preventDefault={submit}>
	<div id="meaning-{meaning.id}-form" class="flex-col space-y-5 card shadow-md p-5">
		<div class="flex gap-5">
			<div class="grow">
				<TextInput
					id="name"
					label="Name"
					bind:value={meaning.name}
					on:input={onChangeHandler}
					valid={res.isValid('name')}
					invalid={res.hasErrors('name')}
					feedback={res.getErrors('name')}
					required={true}
					{help}
				/>
			</div>
			<!--Description-->
			<div class="grow">
				<TextArea
					id="description"
					label="Description"
					bind:value={meaning.description}
					on:input={onChangeHandler}
					valid={res.isValid('description')}
					invalid={res.hasErrors('description')}
					feedback={res.getErrors('description')}
					{help}
					required={false}
				/>
			</div>
		</div>

		<div class="py-5">
			<MeaningEntries bind:entries={meaning.externalLinks} />
		</div>

		<div class="flex gap-3 items-center">
			<div>
				<SlideToggle
					active="bg-secondary-500"
					size="sm"
					id="approved"
					name="Approved"
					bind:checked={meaning.approved}
					on:change>Approved</SlideToggle
				>
			</div>
			<div>
				<SlideToggle
					active="bg-secondary-500"
					size="sm"
					id="selectable"
					name="Selectable"
					bind:checked={meaning.selectable}
					on:change>Selectable</SlideToggle
				>
			</div>
		</div>
		<div class="flex gap-5 items-center">
			<div class="w-1/2">
				<MultiSelect
					id="constraints"
					title="Constraints"
					source={$constraintsStore}
					itemId="id"
					itemLabel="text"
					itemGroup="group"
					complexSource={true}
					complexTarget={true}
					isMulti={true}
					clearable={true}
					bind:target={meaning.constraints}
					placeholder="-- Please select --"
					invalid={res.hasErrors('constraints')}
					feedback={res.getErrors('constraints')}
				/>
			</div>
			<div class="w-1/2">
				<ConstraintsDescription bind:list={meaning.constraints} />
			</div>
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
				title="Save Meaning Template, {meaning.name}"
				id="save"
				disabled={!isValid || !linksValid}
			>
				<Fa icon={faSave} /></button
			>
		</div>
	</div>
</form>
