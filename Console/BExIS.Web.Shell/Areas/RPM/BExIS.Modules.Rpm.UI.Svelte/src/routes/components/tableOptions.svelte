<script lang="ts">
	import { faPen, faTrash } from '@fortawesome/free-solid-svg-icons';
	import { helpStore } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';

	export let row: any;
	export let dispatchFn: any;

	console.log('tableOption row',row);
</script>

<tableOption>
	<div class="w-24" id={row.id}>
		{#if row.inUse === false}
			<!-- svelte-ignore a11y-mouse-events-have-key-events -->
			<button
				type="button"
				class="chip variant-filled-primary shadow-md"
				title="Edit, {row.name}"
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
				title="Delete, {row.name}"
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
		<!-- {:else} -->
			<!-- svelte-ignore a11y-mouse-events-have-key-events -->
			<!-- <button
				type="button"
				class="chip variant-filled-primary shadow-md bg-opacity-40 text-opacity-40"
				title="Edit, {row.name}"
				id="edit"
				disabled
				on:mouseover={() => {
					helpStore.show('edit');
				}}
			>
				<Fa icon={faPen} />
			</button> -->
			<!-- svelte-ignore a11y-mouse-events-have-key-events -->
			<!-- <button
				type="button"
				class="chip variant-filled-error shadow-md bg-opacity-40 text-opacity-40"
				title="Delete, {row.name}"
				id="delete"
				disabled
				on:mouseover={() => {
					helpStore.show('delete');
				}}
			>
				<Fa icon={faTrash} /></button
			> -->
		{/if}
	</div>
</tableOption>
