<script lang="ts">
	import Fa from 'svelte-fa';
	import { faPen, faTrash } from '@fortawesome/free-solid-svg-icons';
	import { helpStore } from '@bexis2/bexis2-core-ui';
	import type { externalLinkType } from '$lib/components/meaning/types';

	export let row: externalLinkType;
	export let dispatchFn: any;
</script>

<tableOption>
	<div class="w-18">
		<!-- {#if inUse === false} -->
		<!-- svelte-ignore a11y-mouse-events-have-key-events -->
		<button
			type="button"
			class="chip variant-filled-primary shadow-md"
			title="Edit External Link, {row.name}"
			id="edit-{row.id}"
			on:mouseover={() => {
				helpStore.show('edit');
			}}
			on:click|preventDefault={() =>
				dispatchFn({
					type: { action: 'edit', id: row.id }
				})}
		>
			<Fa icon={faPen} />
		</button>
		<!-- svelte-ignore a11y-mouse-events-have-key-events -->
		<button
			type="button"
			class="chip variant-filled-primary shadow-md"
			title="Delete External Link, {row.name}"
			id="delete-{row.id}"
			on:mouseover={() => {
				helpStore.show('delete');
			}}
			on:click|preventDefault={() =>
				dispatchFn({
					type: { action: 'delete', id: row.id }
				})}
		>
			<Fa icon={faTrash} /></button
		>
		<!-- {:else}
			<button
				type="button"
				class="chip variant-filled-primary shadow-md"
				title="Edit External Link, {row.name}"
				id="edit-{row.Id}"
				disabled
			>
				<Fa icon={faPen} />
			</button>

			<button
				type="button"
				class="chip variant-filled-error shadow-md"
				title="Delete External Link, {row.name}"
				id="delete-{row.id}"
				disabled
			>
				<Fa icon={faTrash} /></button>
		{/if} -->
	</div>
</tableOption>
