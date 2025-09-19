<script lang="ts">
	import Fa from 'svelte-fa';
	import { curationStore } from './stores';
	import {
		CurationEntryStatusDetails,
		CurationEntryType,
		CurationEntryTypeNames,
		type CurationEntryCreationModel
	} from './types';
	import { faMessage, faRotateLeft } from '@fortawesome/free-solid-svg-icons';

	export let inputData: Partial<CurationEntryCreationModel>;
	export let isDraft: boolean = false;
	export let positionHidden: boolean = false;
	export let resetValues:
		| {
				type: () => void;
				position: () => void;
				name: () => void;
				description: () => void;
		  }
		| undefined = undefined;

	const { curation } = curationStore;

	let showCommentField: boolean = (inputData.comment?.trim().length ?? 0) > 0;
</script>

<label class="min-w-32 grow basis-2/5">
	<span>Category:</span>
	<div class="flex items-stretch">
		<select bind:value={inputData.type} class="input rounded-r-none" required>
			<option value="" disabled>Select category</option>
			<option value={CurationEntryType.None}>None (Hidden)</option>
			<!-- Status Entry should not be created created this way -->
			{#each $curation?.curationEntryTypes ?? [] as type}
				<option value={type}>
					{CurationEntryTypeNames[type]}
				</option>
			{/each}
		</select>
		<button
			class="btn rounded-l-none border-y border-r border-surface-500 px-3 ring-0"
			disabled={resetValues?.type === undefined}
			on:click|preventDefault={resetValues?.type}
			title="Undo changes"
		>
			<Fa icon={faRotateLeft} />
		</button>
	</div>
</label>

{#if !positionHidden}
	<label class="grow basis-1/6">
		<span>Position:</span>
		<div class="flex items-stretch">
			<input
				type="number"
				bind:value={inputData.position}
				class="input rounded-r-none"
				placeholder="Enter position"
				min="1"
				required
			/>
			<button
				class="btn rounded-l-none border-y border-r border-surface-500 px-3 ring-0"
				disabled={resetValues?.position === undefined}
				on:click|preventDefault={resetValues?.position}
				title="Undo changes"
			>
				<Fa icon={faRotateLeft} />
			</button>
		</div>
	</label>
{/if}

<label class="grow basis-full">
	<span>Title:</span>
	<div class="flex items-stretch">
		<input
			type="text"
			bind:value={inputData.name}
			class="input rounded-r-none"
			placeholder="Enter name"
			maxLength={255}
			required
		/>
		<button
			class="btn rounded-l-none border-y border-r border-surface-500 px-3 ring-0"
			disabled={resetValues?.name === undefined}
			on:click|preventDefault={resetValues?.name}
			title="Undo changes"
		>
			<Fa icon={faRotateLeft} />
		</button>
	</div>
</label>

<label class="grow basis-full">
	<span>Description:</span>
	<div class="flex items-stretch">
		<textarea
			bind:value={inputData.description}
			class="input rounded-r-none"
			placeholder="Enter description"
			required
		></textarea>
		<button
			class="btn rounded-l-none border-y border-r border-surface-500 px-3 ring-0"
			disabled={resetValues?.description === undefined}
			on:click|preventDefault={resetValues?.description}
			title="Undo changes"
		>
			<Fa icon={faRotateLeft} />
		</button>
	</div>
</label>

{#if isDraft}
	<!-- Comment Field -->
	<div class="my-2 flex w-full flex-wrap gap-2 border-t border-surface-300 pt-2">
		{#if !showCommentField}
			<button
				class="variant-outline-surface btn min-w-40 grow px-2 py-1 text-sm text-surface-700"
				on:click={() => (showCommentField = true)}
			>
				<Fa icon={faMessage} class="mr-1 inline-block" />
				Add Initial Comment
			</button>
		{:else}
			<label class="min-w-40 grow">
				<span>Initial Comment:</span>
				<div class="flex items-stretch">
					<textarea
						bind:value={inputData.comment}
						class="input rounded-r-none"
						placeholder="Enter initial comment"
					></textarea>
				</div>
			</label>
		{/if}

		<!-- Change Status -->
		<label>
			<span>Initial Status:</span>
			<br />
			<select
				bind:value={inputData.status}
				class="input w-full px-2 py-1 sm:w-32"
				title="Change Initial Status"
			>
				{#each CurationEntryStatusDetails as { status, name }}
					<option value={status}>
						{name}
					</option>
				{/each}
			</select>
		</label>
	</div>
{/if}
