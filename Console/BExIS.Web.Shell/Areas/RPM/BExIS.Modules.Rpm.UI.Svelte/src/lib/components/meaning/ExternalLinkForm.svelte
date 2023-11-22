<script lang="ts">
	import { TextInput } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faCheck, faXmark } from '@fortawesome/free-solid-svg-icons';

	import { createEventDispatcher, onMount, afterUpdate } from 'svelte';

	import suite from './externalLinkForm';
	import type { externalLinkType } from './types';

	export let id: number = 0;
	export let index: number = 1;
	export let link: externalLinkType;
	export let last = true;

	export let isValid: boolean = false;
	// validation
	let res = suite.get();
	$: isValid = res.isValid();

	let exist: boolean = false;
	$: exist;

	// block if id exist

	const dispatch = createEventDispatcher();

	onMount(() => {
		// reset & reload validation
		suite.reset();

		// if link is allready stored, no need for validation
		console.log('🚀 ~ file: ExternalLink.svelte:37 ~ onMount ~ link.id:', link.id);
		isValid = link.id > 0 ? true : false; // set is valid to true if its allready exist
		exist = link.id > 0 ? true : false; // set exist to true if its allready exist
	});

	afterUpdate(() => {
		console.log('lin after update', link);
		// reset & reload validation
		//suite.reset();

		// if link is allready stored, no need for validation
		console.log('🚀 ~ file: ExternalLink.svelte:37 ~ onMount ~ link.id:', link.id);

		if (link.id == 0) {
			// validate only new links with id == 0
			res = suite(link, '');
			setValidationState(res);
		} else {
			isValid == true;
		}
		exist = link.id > 0 ? true : false;
	});

	function remove() {
		dispatch('remove');
	}

	function onChangeFn(e) {
		if (link.id == 0) {
			// validate only new links with id == 0
			res = suite(link, e.target.id);
			setValidationState(res);
		}
	}

	function setValidationState(res) {
		isValid = res.isValid();
		console.log(
			'🚀 ~ file: ExternalLink.svelte:55 ~ setValidationState ~ isValid:',
			isValid,
			index
		);
		// dispatch this event to the parent to check the save button
		dispatch('link-change');
	}
</script>

<div id={'' + index} class="flex space-x-3 items-baseline">
	<TextInput
		id="link_name"
		bind:value={link.name}
		on:change
		on:input={onChangeFn}
		placeholder="Name"
		valid={res.isValid('link_name')}
		invalid={res.hasErrors('link_name')}
		feedback={res.getErrors('link_name')}
		disabled={exist}
	/>

	<TextInput id="link_type" bind:value={link.type} on:change placeholder="Type" disabled={exist} />

	<TextInput
		id="link_uri"
		bind:value={link.uri}
		on:change
		on:input={onChangeFn}
		placeholder="Uri"
		valid={res.isValid('link_uri')}
		invalid={res.hasErrors('link_uri')}
		feedback={res.getErrors('link_uri')}
		disabled={exist}
	/>

	<div class="flex text-xl w-12 gap-2">
		<button title="remove" type="button" on:click={remove}
			><Fa class="text-error-500" icon={faXmark} /></button
		>
		{#if isValid && !exist}
			<Fa class="text-success-500" icon={faCheck} />
		{/if}
	</div>
</div>
