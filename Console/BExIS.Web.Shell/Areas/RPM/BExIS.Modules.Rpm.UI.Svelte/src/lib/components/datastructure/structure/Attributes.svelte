<script lang="ts">
	import { TextInput, TextArea } from '@bexis2/bexis2-core-ui';
	import { onMount } from 'svelte';
	import type { DataStructureModel } from '../types';

	export let model: DataStructureModel;
	export let valid = false;

	import suite from './attributes';

	// validation
	let res = suite.get();

	onMount(() => {
		suite.reset();

		model.title = '' + model.id;
		res = suite(model, '');
	});

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e) {
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			res = suite(model, e.target.id);
			valid = res.isValid();
		}, 10);
	}
</script>

{#if model}
	<div class="structure-attributes-container grid md:grid-cols-2 sm:grid-cols-1 gap-5">
		<TextInput
			id="title"
			label="Title"
			bind:value={model.title}
			on:input={onChangeHandler}
			valid={res.isValid('title')}
			invalid={res.hasErrors('title')}
			feedback={res.getErrors('title')}
			required={true}
		/>

		<div>
			<TextArea
				id="description"
				label="Description"
				bind:value={model.description}
				on:input={onChangeHandler}
				valid={res.isValid('description')}
				invalid={res.hasErrors('description')}
				feedback={res.getErrors('description')}
			/>
			{#if model.description != undefined}
				<span class="text-right" class:text-error-500={model.description.length > 255}
					>{255 - model.description.length}</span
				>
			{/if}
		</div>
	</div>
{/if}
