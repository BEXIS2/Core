<script lang="ts">
	import { onMount } from "svelte";
	import { getVersions, getTags } from "../services";

	import type { versionListItemType, TagInfoViewModel } from "../types";
	import { fade } from "svelte/transition";

	export let id: number;
	export let version: number;
	export let useTags: boolean = false;

	let currentVersion: versionListItemType | undefined = undefined;
	let versions: versionListItemType[] = [];
	let showVersions: boolean = false;

	let tags: TagInfoViewModel[] = [];
	let showReleases: boolean = false;
	let isBeyondLatestTag: boolean = false;

	function sortTags(list: TagInfoViewModel[]): TagInfoViewModel[] {
		return [...list].sort((a, b) => a.version - b.version);
	}

	$: latestTagNr = useTags ? Math.max(0, ...versions.map(v => v.tagNr || 0)) : 0;

	onMount(async () => {
		console.log('id', id);
		console.log('version', version);

		const res = await getVersions(id, version);
		versions = res;
		console.log("🚀 ~ versions:", versions);

		currentVersion = versions.find(v => v.id === version);

		if (useTags) {
			const tagRes = await getTags(id, version);
			tags = sortTags(tagRes);
			console.log("🚀 ~ tags:", tags);

			isBeyondLatestTag = currentVersion != undefined && currentVersion.tagNr === 0 && latestTagNr > 0;
		}
	});
</script>

<div class="flex flex-col gap-3">
	<h4 class="h4">Versions</h4>

	<div class="flex justify-between items-center">
		<b>Version {currentVersion?.id}</b>
		<div class="flex items-center gap-2">
			{#if useTags && currentVersion?.tagNr}
				<a href="/dcm/view?id={id}&tag={currentVersion.tagNr}" target="_blank"
					class="badge variant-soft-primary text-xs hover:variant-filled-primary cursor-pointer"
					title="Switch to Tag {currentVersion.tagNr}">
					Tag {currentVersion.tagNr}
				</a>
			{:else if useTags}
				<span class="badge variant-soft-surface text-xs" title="No tag assigned">untagged</span>
			{/if}
			<span class="text-sm text-surface-800 semi-bold italic">{currentVersion?.date}</span>
		</div>
	</div>

	{#if isBeyondLatestTag}
		<div class="text-sm text-warning-500 border-l-2 border-warning-500 pl-3">
			The current version is beyond the latest release (Tag {latestTagNr}).
		</div>
	{/if}

	{#if currentVersion?.changeDescription}
		<div class="text-sm pl-3 border-l-2 border-surface-300">
			{currentVersion.changeDescription}
		</div>
	{/if}

	<div class="flex justify-end gap-2">
		{#if useTags && tags.length > 0}
			<button class="chip p-0" on:click={() => showReleases = !showReleases}>
				{showReleases ? 'Hide releases' : 'Show releases'}
			</button>
		{/if}
		<button class="chip p-0" on:click={() => showVersions = !showVersions}>
			{showVersions ? 'Hide versions' : 'Show other versions'}
		</button>
	</div>

	{#if useTags && showReleases && tags.length > 0}
		<div class="flex flex-col gap-3" transition:fade>
			{#each tags as t}
				<div class="flex flex-col gap-1">
					<div class="flex justify-between items-center">
						<a href="/dcm/view?id={id}&tag={t.version}" target="_blank"
							class="font-bold underline hover:text-primary-500 cursor-pointer"
							title="Switch to Tag {t.version}">
							Tag {t.version}
						</a>
						<span class="text-sm text-surface-800 semi-bold italic">
							{t.releaseDate ? new Date(t.releaseDate).toLocaleDateString('en-US') : 'N/A'}
						</span>
					</div>
					{#if t.releaseNotes?.length > 0}
						<div class="flex flex-col gap-1 pl-3 border-l-2 border-surface-300">
							{#each t.releaseNotes as note}
								<div class="text-sm">{note}</div>
							{/each}
						</div>
					{/if}
				</div>
			{/each}
		</div>
	{/if}

	{#if showVersions}
		<div class="flex flex-col gap-3" transition:fade>
			{#each versions.filter(v => v.id !== currentVersion?.id) as v}
				<div class="flex flex-col gap-1">
					<div class="flex justify-between items-center">
						<a href="/dcm/view?id={id}&version={v.id}" target="_blank"
							class="font-bold underline hover:text-primary-500 cursor-pointer"
							title="Switch to Version {v.id}">
							Version {v.id}
						</a>
						<div class="flex items-center gap-2">
							{#if useTags && v.tagNr}
								<a href="/dcm/view?id={id}&tag={v.tagNr}" target="_blank"
									class="badge variant-soft-primary text-xs hover:variant-filled-primary cursor-pointer"
									title="Switch to Tag {v.tagNr}">
									Tag {v.tagNr}
								</a>
							{:else if useTags}
								<span class="badge variant-soft-surface text-xs" title="No tag assigned">untagged</span>
							{/if}
							<span class="text-sm text-surface-800 semi-bold italic">{v.date}</span>
						</div>
					</div>
					{#if v.changeDescription}
						<div class="text-sm pl-3 border-l-2 border-surface-300">
							{v.changeDescription}
						</div>
					{/if}
				</div>
			{/each}
		</div>
	{/if}
</div>
