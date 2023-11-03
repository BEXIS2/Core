<script lang="ts">
	import type { externalLinkType } from '$lib/components/meaning/types';
	import { externalLinksStore } from '$lib/components/meaning/stores';

	export let list: externalLinkType[] = [];
	$:list

	import { onMount } from 'svelte';
	import { MultiSelect } from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';

	import { faAdd, faCheck } from '@fortawesome/free-solid-svg-icons';
	import ExternalLinkView from './ExternalLinkView.svelte';
	import ExternalLinkForm from '$lib/components/meaning/ExternalLinkForm.svelte';

	let selectableLinks: externalLinkType[] = [];
	$: selectableLinks;

	let selectedExternalLink: externalLinkType;
	export let valid:boolean = false;
	let linkValidationStates:any = [];

	let newLinks:externalLinkType[];
	$:newLinks;
	let existingLinks:externalLinkType[];
	$:existingLinks;

	let loading = false;

	onMount(() => {
		//set selectableLinks
		updateSelectableLinks();
		setLists();


		//fillLinkValidationStates(newLinks)


	});

	function remove(i:number, type:string) {

		if(type=="new")
		{	
				newLinks = newLinks.filter((m, index) => i !== index)
		}
		else
		{
			 existingLinks = existingLinks.filter((m, index) => i !== index)
		}
		list = [...newLinks,... existingLinks];


		// remove item result from array of validation states from 
		linkValidationStates = [...linkValidationStates.splice(i,1)];

		updateSelectableLinks();

		//checkValidationState();

	}

	function add() {
		console.log('list', list);

		let newLink: externalLinkType = {
			id: 0,
			name: '',
			type: '',
			uri: ''
		};

	
		newLinks = [...newLinks, newLink];
		list = [...newLinks,...existingLinks]
		linkValidationStates = [...linkValidationStates,true];

		console.log('list', list);
	}

	function updateSelectableLinks() {
		selectableLinks = $externalLinksStore.filter((e) => !list.find(i => i.uri == e.uri));

	}

	function onSelectHandler(e) {

		loading = true;

		existingLinks = [...existingLinks, selectedExternalLink];
		selectedExternalLink = undefined;

		// selection needs to filter
		updateSelectableLinks()

		list = [...newLinks,...existingLinks];

		loading = false;
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

}

function setLists()
{
		newLinks = list.filter(l=>l.id==0)
		existingLinks = list.filter(l=>l.id>0)
}

</script>



<div class="link-container space-y-4">

	<label class="flex gap-3"><b>External Links</b>
	{#if valid}<span class="text-success-500"><Fa icon="{faCheck}"/></span>{/if}
	</label> 
	<div class="card p-5">
		<b>New</b>
		{#if newLinks}
			{#each newLinks as link, id}
				<!-- content here -->
				<ExternalLinkForm
					{id}
					index = {id}
					bind:link={link}
					bind:isValid={linkValidationStates[id]}
					on:remove={() => remove(id,"new")}
					on:link-change={checkValidationState}
					last={list.length - 1 === id}
				/>
			{/each}
	{/if}
	</div>

<div class="card p-5">
	<b>Existing</b>

	{#if existingLinks}
		{#each existingLinks as link, id}
			<!-- content here -->
			<ExternalLinkView
			 {id}
				index = {id}
				bind:link={link}
				bind:isValid={linkValidationStates[id]}
				on:remove={() => remove(id,"exist")}
				on:link-change={checkValidationState}
				last={list.length - 1 === id}
			/>
		{/each}
	{/if}
</div>
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
			{loading}
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
