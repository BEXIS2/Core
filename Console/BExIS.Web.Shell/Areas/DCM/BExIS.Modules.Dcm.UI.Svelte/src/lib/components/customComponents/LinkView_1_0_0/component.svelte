<script lang="ts">
	import Fa from 'svelte-fa';
	import { faArrowUpRightFromSquare } from '@fortawesome/free-solid-svg-icons';
	import { getMetadata, getRefByPath } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { metadataStore } from '$lib/components/utils/metadata/stores';
	import { convertDisplayName } from '$lib/components/utils/metadata/metadataShared';

	export let anchor: string;
	export let label: string;

	$: storeVersion = $metadataStore;
	$: value = getMetadata(anchor).value;
	$: ref = getRefByPath(anchor);
</script>

<div class="entry">
	<span class="key text-sm font-medium text-gray-600 dark:text-gray-300">{convertDisplayName(label)}</span>
	<span class="val text-sm text-gray-900 dark:text-gray-100">
		{#if value}
			{#if ref}
				<a href={ref} target="_blank" rel="noopener noreferrer" class="link-ref">
					<span>{value}</span>
					<Fa icon={faArrowUpRightFromSquare} class="link-ref-icon" />
				</a>
			{:else}
				{value}
			{/if}
		{:else}
			<span class="text-gray-500 dark:text-gray-400">—</span>
		{/if}
	</span>
</div>

<style>
	.entry {
		padding-bottom: 0.35rem;
	}
	.key {
		display: inline-block;
		flex-grow: 1;
	}
	.val {
		display: inline-block;
		width: 35vw;
	}
	.link-ref {
		display: inline-flex;
		align-items: center;
		gap: 0.25rem;
		color: rgb(37 99 235);
	}
	.link-ref:hover {
		text-decoration: underline;
	}
	.link-ref-icon {
		font-size: 0.7rem;
		opacity: 0.8;
	}

	@media (max-width: 768px) {
		.val {
			width: 50vw;
		}
	}
</style>
