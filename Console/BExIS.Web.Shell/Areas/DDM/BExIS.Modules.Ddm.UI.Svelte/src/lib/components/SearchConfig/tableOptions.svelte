<script lang="ts">
	import { faPen, faTrash, faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
	import { helpStore } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';

	export let row: any;
	export let dispatchFn: any;

	console.log(row.name+': '+row.inUseByVariable+'|'+row.inUseByMeaning);
</script>

<tableOption>
	<div class="w-32" id={row.id}>
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

		{#if row.inUseByVariable === true || row.inUseByMeaning === true}
			<!-- svelte-ignore a11y-mouse-events-have-key-events -->
			<!-- <button
				type="button"
				class="chip variant-filled-error shadow-md"
				title="Delete, {row.name}"
				id="delete"
				disabled
				on:mouseover={() => {
					helpStore.show('delete');
				}}
			>
				<Fa icon={faTrash} /></button
			> -->
			<span class="chip" title="{row.name} is in use"
				><Fa class="text-warning-500 shadow-md" icon={faTriangleExclamation} /></span
			>
		{:else}
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
		{/if}
	</div>
</tableOption>
