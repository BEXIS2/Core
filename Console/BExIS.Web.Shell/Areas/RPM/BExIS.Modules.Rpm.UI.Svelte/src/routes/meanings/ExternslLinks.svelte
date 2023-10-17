<script lang="ts">
	import type { externalLinkType } from './types';
	import { externalLinksStore } from './stores';

	import Link from './ExternalLink.svelte';
	export let list: externalLinkType[] = [];

	import { onMount } from 'svelte';
	import { MultiSelect } from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';

	import { faAdd, faCheck } from '@fortawesome/free-solid-svg-icons';

	let selectableLinks: externalLinkType[] = [];
	$: selectableLinks;

	let selectedExternalLink: externalLinkType;
	export let valid:boolean = false;
	let linkValidationStates:any = [];

	onMount(() => {
		//set selectableLinks
		updateSelectableLinks();
		fillLinkValidationStates(list)

		if (list.length === 0) {
		
			add();
		}
	});

	function remove(i) {
		list = [...list.filter((m, index) => i !== index)];

		if (list.length === 0) {
			add();
		}

		updateSelectableLinks();
		checkValidationState();
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
		selectableLinks = $externalLinksStore.filter((e) => !list.find(i => i.uri == e.uri));

	}

	function onSelectHandler(e) {
		list = [...list, selectedExternalLink];

		updateSelectableLinks()
		selectedExternalLink = null;
	}

function fillLinkValidationStates(l) {

linkValidationStates = [];

for (let index = 0; index < l.length; index++) {
	linkValidationStates.push(false);
}
}

// every time when validation state of a varaible is change,
// this function triggered an check wheter save button can be active or not
function checkValidationState() {
	
valid = linkValidationStates.every((v) => v === true);
console.log("🚀 ~ file: ExternslLinks.svelte:83 ~ checkValidationState ~ linkValidationStates:", linkValidationStates)
//console.log("TCL ~ file: Variables.svelte:63 ~ checkValidationState ~ linkValidationStates:", linkValidationStates)
}
</script>



<div class="link-container space-y-4">

	<label class="flex gap-3"><b>External Links</b>
	{#if valid}<span class="text-success-500"><Fa icon="{faCheck}"/></span>{/if}
	</label> 

	{#if list}
		{#each list as link, index}
			<!-- content here -->
			<Link
				{index}
				{link}
				
				bind:isValid={linkValidationStates[index]}
				on:remove={() => remove(index)}
				on:link-change={checkValidationState}
				last={list.length - 1 === index}
			/>
		{/each}
	{/if}


</div>
<div class="flex items-center gap-3 w-1/2">
	<div class="grow">
		<MultiSelect
			id="links"
			bind:source={selectableLinks}
			itemId="id"
			itemLabel="name"
			itemGroup="type"
			complexSource={true}
			complexTarget={true}
			bind:target={selectedExternalLink}
			isMulti={false}
			placeholder="-- Please select --"
			on:change={(e) => onSelectHandler(e)}
		/>
	</div>
	<span class="span w-14 text-center">or</span>
	<div class="inline-block align-bottom">	
		<button type="button" class="btn variant-filled-primary" on:click={add}><Fa icon={faAdd} /></button>
</div>

</div>

<style>
	.link-container {
		padding-bottom: 1em;
	}
</style>
