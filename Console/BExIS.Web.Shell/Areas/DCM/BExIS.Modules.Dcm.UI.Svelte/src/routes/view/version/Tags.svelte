<script lang="ts">
	import { onMount } from "svelte";
	import { getTags } from "../services";
	import type { TagInfoViewModel } from "../types";
	import { fade } from "svelte/transition";

	export let id: number;
	export let version: number;
	export let tag: number;
	export let tags: TagInfoViewModel[] = [];

	let currentTag: TagInfoViewModel | undefined = undefined;
	let isBeyondLatestTag: boolean = false;
	let showTags: boolean = false;

	function sortTags(list: TagInfoViewModel[]): TagInfoViewModel[] {
		return [...list].sort((a, b) => a.version - b.version);
	}

	$: sortedTags = sortTags(tags);
	$: otherTags = isBeyondLatestTag ? sortedTags : sortedTags.filter(v => v.version !== currentTag?.version);

	onMount(async () => {
		console.log('id', id);
		console.log('version', version);

		const res = await getTags(id, version);
		tags = res;

		console.log("🚀 ~ tags:", tags);

		const sorted = sortTags(res);
		currentTag = sorted.find(t => t.version === tag);

		if (currentTag === undefined && sorted.length > 0) {
			currentTag = sorted[sorted.length - 1];
			isBeyondLatestTag = true;
		}
	});
</script>

<div class="flex flex-col gap-3">
	<h4 class="h4">Releases</h4>

	{#if tags.length === 0}
		<span><b>No releases available.</b></span>
	{:else}
		<div class="flex justify-between items-center">
			{#if isBeyondLatestTag}
				<span class="text-warning-500 font-bold">Working version (beyond latest Release: {currentTag?.version})</span>
			{:else}
				<span class="font-bold">Tag {currentTag?.version}</span>
			{/if}
			<span class="text-sm text-surface-800 semi-bold italic" title="Release Date">
				{currentTag?.releaseDate ? new Date(currentTag.releaseDate).toLocaleDateString('en-US') : 'N/A'}
			</span>
		</div>

		{#if isBeyondLatestTag}
			<div class="text-sm text-warning-500 border-l-2 border-warning-500 pl-3 bg-warning-50 dark:bg-warning-900/20 p-2 rounded font-bold">
				The current version has not been tagged yet.
			</div>
		{:else if currentTag?.releaseNotes?.length > 0}
			<div class="flex flex-col gap-1 pl-3 border-l-2 border-surface-300">
				{#each currentTag.releaseNotes as note}
					<div class="text-sm">{note}</div>
				{/each}
			</div>
		{/if}

		<div class="flex justify-end">
			<button class="chip p-0 semi-bold" on:click={() => showTags = !showTags}>
				{showTags ? 'Hide releases' : 'Show more releases'}
			</button>
		</div>

		{#if showTags}
			<div class="flex flex-col gap-3" transition:fade>
				{#each otherTags as v}
					<div class="flex flex-col gap-1">
						<div class="flex justify-between items-center">
							<a href="/dcm/view?id={id}&tag={v.version}" 
								class="font-bold underline hover:text-primary-500 cursor-pointer"
								title="Switch to Tag {v.version}">
								Tag {v.version}
							</a>
							<span class="text-sm text-surface-800 semi-bold italic" title="Release Date">
								{v.releaseDate ? new Date(v.releaseDate).toLocaleDateString('en-US') : 'N/A'}
							</span>
						</div>
						{#if v.releaseNotes?.length > 0}
							<div class="flex flex-col gap-1 pl-3 border-l-2 border-surface-300">
								{#each v.releaseNotes as note}
									<div class="text-sm">{note}</div>
								{/each}
							</div>
						{/if}
					</div>
				{/each}
			</div>
		{/if}
	{/if}
</div>
