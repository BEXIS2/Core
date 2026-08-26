<script lang="ts">
	import { faFileLines, faUpRightFromSquare } from "@fortawesome/free-solid-svg-icons";
	import Fa from "svelte-fa";
	import MetadataView from "./MetadataView.svelte";

	export let id = 0;
	export let version = 1;
	export let tag = 0;
	export let hook;
	export let description = '';

	const url = `/dcm/view/metadata?id=${id}&version=${version}&tag=${tag}`;

	let showInline = false;
</script>

<div class="flex justify-between items-center mb-2 mt-0">
	<h3 class="h3 mt-0">Metadata</h3>
	<div class="flex gap-2">
		{#if !showInline}
			<button class="btn btn-sm variant-soft-primary" on:click={() => (showInline = true)} title="Show metadata inline">
				<Fa icon={faFileLines} />
				<span class="ml-1">Show</span>
			</button>
		{:else}
			<button class="btn btn-sm variant-soft-surface" on:click={() => (showInline = false)} title="Hide metadata">
				<Fa icon={faFileLines} />
				<span class="ml-1">Hide</span>
			</button>
		{/if}
		<a href={url} target="_blank" class="btn btn-sm variant-soft-primary" title="Open full metadata view in new window">
			<Fa icon={faUpRightFromSquare} />
			<span class="ml-1">Open</span>
		</a>
	</div>
</div>

{#if description && !showInline}
	<p class="text-sm text-surface-600 dark:text-surface-300 dark:text-surface-500 dark:text-surface-400 mb-2">{description}</p>
{/if}

{#if showInline}
	<div class="mb-4">
		<MetadataView {id} {version} {tag} />
	</div>
{/if}
