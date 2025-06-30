<script lang="ts">
	import { faFilter } from '@fortawesome/free-solid-svg-icons';
	import { curationStore } from './stores';
	import {
		CurationEntryStatus,
		CurationEntryStatusDetails,
		CurationEntryType,
		CurationEntryTypeNames
	} from './types';
	import Fa from 'svelte-fa';
	import { writable } from 'svelte/store';
	import type { CurationEntryClass } from './CurationEntries';

	const statusFilterId = 'status';

	const statusFilter = curationStore.getEntryFilterData(statusFilterId);

	const statusFilterFn = (entry: CurationEntryClass, data: Set<CurationEntryStatus>) =>
		data.size === 0 || data.has(entry.status);

	function toggleStatusFilter(status: CurationEntryStatus) {
		curationStore.updateEntryFilter(
			statusFilterId,
			(data: Set<CurationEntryStatus> | undefined) => {
				if (!data) return new Set([status]);
				if (data.has(status)) data.delete(status);
				else data.add(status);
				return data;
			},
			statusFilterFn,
			(data) => data?.size === 0 || data?.size === 4
		);
	}

	// Type Filter

	const filterEntryTypes = [
		CurationEntryType.MetadataEntryItem,
		CurationEntryType.DatastructureEntryItem,
		CurationEntryType.PrimaryDataEntryItem
	];

	const typeFilterId = 'type';

	const typeFilter = curationStore.getEntryFilterData(typeFilterId);

	const typeFilterGroup = writable<CurationEntryType | undefined>(undefined);

	typeFilter.subscribe((tf) => typeFilterGroup.set(tf?.data));

	const typeFilterFn = (entry: CurationEntryClass, data: CurationEntryType | undefined) =>
		data === undefined || entry.type === data;

	typeFilterGroup.subscribe((type) => {
		curationStore.updateEntryFilter(
			typeFilterId,
			(data: CurationEntryType | undefined) => {
				if (data === undefined || data !== type) return type;
				return undefined;
			},
			typeFilterFn,
			(data) => !data
		);
	});

	// Search

	const searchFilterId = 'search';

	const searchFilter = curationStore.getEntryFilterData(searchFilterId);

	let timer: NodeJS.Timeout | undefined = undefined;
	const searchInput = writable('');

	searchFilter.subscribe((sf) => searchInput.set(sf?.data || ''));

	const searchFilterFn = (entry: CurationEntryClass, data: string | undefined) => {
		if (!data || data.trim().length === 0) return true;
		return [
			entry.name,
			entry.description,
			...entry.visibleNotes.map((vn) => vn.readableComment)
		].some((s) => s.toLowerCase().includes(data.toLowerCase()));
	};

	searchInput.subscribe((si) => {
		clearTimeout(timer);
		timer = setTimeout(() => {
			curationStore.updateEntryFilter(
				searchFilterId,
				(_: string | undefined) => si,
				searchFilterFn,
				(data) => !data || data.trim().length === 0
			);
		}, 500);
	});
</script>

<div class="flex flex-wrap items-center gap-2 p-2">
	<Fa icon={faFilter} class="inline-block" />
	<span>Filter:</span>
	<div class="flex flex-wrap items-center gap-1 rounded border border-surface-400 p-1">
		<span>Status:</span>
		{#key $statusFilter?.data.toString}
			{#each CurationEntryStatusDetails as statusDetails, index}
				<label
					class="button-label cursor-pointer rounded border border-surface-500 bg-surface-300 px-2 py-0.5"
				>
					<input
						class="pointer-events-none absolute opacity-0"
						type="checkbox"
						title="Hide/Show '{statusDetails.name}' entries"
						checked={!!$statusFilter?.data.has(index)}
						on:click={() => toggleStatusFilter(index)}
					/>
					<span>{statusDetails.name}</span>
				</label>
			{/each}
		{/key}
	</div>
	<div class="flex flex-wrap items-center gap-1 rounded border border-surface-400 p-1">
		<span>Type:</span>
		<label
			class="button-label cursor-pointer rounded border border-surface-500 bg-surface-300 px-2 py-0.5"
		>
			<input
				class="pointer-events-none absolute opacity-0"
				type="radio"
				title="Remove entry type filter"
				bind:group={$typeFilterGroup}
				value={undefined}
			/>
			<span>All</span>
		</label>
		{#each filterEntryTypes as i}
			<label
				class="button-label cursor-pointer rounded border border-surface-500 bg-surface-300 px-2 py-0.5"
			>
				<input
					class="pointer-events-none absolute opacity-0"
					type="radio"
					title="Hide/Show '{CurationEntryTypeNames[i]}' entries"
					bind:group={$typeFilterGroup}
					value={i}
				/>
				<span>{CurationEntryTypeNames[i]}</span>
			</label>
		{/each}
	</div>
	<label class="flex items-center gap-1 rounded border border-surface-400 p-1">
		<span>Search:</span>
		<input class="rounded-sm px-1 py-0.5 text-sm" type="text" bind:value={$searchInput} />
	</label>
</div>

<style lang="postcss">
	.button-label:has(:focus-visible) {
		@apply shadow-sm ring-2 ring-blue-500;
	}

	.button-label:hover {
		@apply bg-surface-300 text-surface-900 ring-2;
	}

	.button-label:has(:checked) {
		@apply border-surface-800 bg-surface-500 text-surface-900 ring-1 ring-surface-800;
	}
</style>
