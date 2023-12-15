<script lang="ts">
	import { meaningEntryType, externalLinkTypeEnum } from '$lib/components/meaning/types';
	import MeaningEntry from './MeaningEntry.svelte';
	import { externalLinksStore } from '$lib/components/meaning/stores';
	// icons
	import Fa from 'svelte-fa';
	import { faAdd, faXmark } from '@fortawesome/free-solid-svg-icons';

	export let entries: meaningEntryType[];
	$: entries;

	// filter fulle list by releations
	let releationList = $externalLinksStore.filter(
		(e) => e.type?.id == externalLinkTypeEnum.relationship
	).map((l)=> {
		return {id:l.id,text:l.name,group:""}
	})

	console.log('🚀 ~ file: MeaningEntry.svelte:10 ~ releationList:', releationList);
	let othersList = $externalLinksStore.filter(
		(e) => e.type?.id != externalLinkTypeEnum.relationship && e.type?.id != externalLinkTypeEnum.prefix
	).map((l)=> {
		return {id:l.id,text:l.name,group:l.type?.text}
	});
	console.log('🚀 ~ file: MeaningEntry.svelte:12 ~ othersList:', othersList);

	function addFn() {
		entries = [...entries, new meaningEntryType()];
	}

	function removeFn(i: number) {
		entries = entries.filter((e) => e != entries[i]);
	}
</script>

<div class="card p-5 space-y-3">

	{#if releationList.length>0 && othersList.length>0}

	<div class="flex">
		<!--Header-->
		<div class="w-12" />
		<div class="w-1/4 h3">Relation</div>
		<div class="grow h3">Objects</div>
	</div>
	<hr />
	{#if entries}
		{#each entries as entry, i}
			<div class="flex items-center">
				<div class=" w-12 inline-block align-bottom">
					<button type="button" class="chip variant-filled-error" on:click={() => removeFn(i)}
						><Fa icon={faXmark} /></button
					>
				</div>
				<div class="grow">
					<MeaningEntry bind:entry={entry} bind:othersList={othersList} bind:releationList={releationList} />
				</div>
			</div>
		{/each}
	{/if}

	<div class="inline-block align-bottom">
		<button type="button" class="chip variant-filled-primary" on:click={addFn}
			><Fa icon={faAdd} /></button
		>
	</div>
	{:else}

	<b>to give the meaning a context you need at least one relation link and one another external link type
	</b>
	<a href="/rpm/externallink">go here</a>

{/if}
</div>
