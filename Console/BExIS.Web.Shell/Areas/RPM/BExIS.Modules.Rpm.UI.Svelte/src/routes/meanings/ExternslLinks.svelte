<script lang="ts">
	import type { externalLinkType } from './types';
	import { externalLinksStore } from './stores';

	import Link from './ExternalLink.svelte';
	export let list: externalLinkType[] = [];

	import { onMount } from 'svelte';
	import { MultiSelect } from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';

	import { faAdd } from '@fortawesome/free-solid-svg-icons';

	let selectableLinks: externalLinkType[] = [];
	$: selectableLinks;

	let selectedExternalLink: externalLinkType;

	onMount(() => {
		//set selectableLinks
		updateSelectableLinks();

		if (list.length === 0) {
			add();
		}
	});

	function remove(i) {
		list = [...list.filter((m, index) => i !== index)];

		if (list.length === 0) {
			add();
		}
	}

	function add() {
		console.log('list', list);

		let newLink: externalLinkType = {
			id: 0,
			name: '',
			type: '',
			uri: ''
		};

		list = [...list, newLink];
		console.log('list', list);
	}

	function updateSelectableLinks() {
		selectableLinks = $externalLinksStore.filter((e) => !list.includes(e));
	}

	function onSelectHandler(e) {
		list = [...list, selectedExternalLink];
		selectedExternalLink = null;
	}
</script>



<div class="link-container space-y-2">

	<label><b>External Links</b></label>

	{#if list}
		{#each list as item, i}
			<!-- content here -->
			<Link
				bind:name={item.name}
				bind:type={item.type}
				bind:uRI={item.uri}
				on:add={add}
				on:remove={() => remove(i)}
				last={list.length - 1 === i}
			/>
		{/each}
	{/if}


</div>
<div class="flex items-center gap-3 w-3/4">
	<div class="grow">
		<MultiSelect
			id="links"
			bind:source={selectableLinks}
			itemId="id"
			itemLabel="name"
			itemGroup="group"
			complexSource={true}
			complexTarget={true}
			bind:target={selectedExternalLink}
			isMulti={false}
			placeholder="-- Please select --"
			on:change={(e) => onSelectHandler(e)}
		/>
	</div>
	<span class="span w-4 text-center">Or</span>
	<div class="inline-block align-bottom">	
		<button type="button" class="btn variant-filled-primary" on:click={add}><Fa icon={faAdd} /></button>
</div>

</div>

<style>
	.link-container {
		padding-bottom: 1em;
	}
</style>
