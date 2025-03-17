<script lang="ts">
	import Fa from 'svelte-fa';
	import { faCopy, faTrash, faPen, faDownload } from '@fortawesome/free-solid-svg-icons';
	import { helpStore } from '@bexis2/bexis2-core-ui';

	export let row: any;
	export let dispatchFn: any;

	let disableBtn: boolean = row.linkedTo.length > 0 ? true : false;

	console.log('row', row);
</script>

<tableOption>
	<div class="w-18">
		<!-- svelte-ignore a11y-mouse-events-have-key-events -->
		<button
			type="button"
			class="chip variant-filled-primary shadow-md"
			title="Download Structure, {row.title}"
			id="download-{row.id}"
			on:mouseover={() => {
				helpStore.show('download');
			}}
			on:click|preventDefault={() =>
				dispatchFn({
					type: { action: 'download', id: row.id, title: row.title }
				})}
		>
			<Fa icon={faDownload} /></button
		>
		<!-- svelte-ignore a11y-mouse-events-have-key-events -->
		<button
			type="button"
			class="chip variant-filled-primary shadow-md"
			title="Copy Structure, {row.title}"
			id="copy-{row.id}"
			on:mouseover={() => {
				helpStore.show('copy');
			}}
			on:click|preventDefault={() =>
				dispatchFn({
					type: { action: 'copy', id: row.id, title: row.title }
				})}
		>
			<Fa icon={faCopy} />
		</button>
		<!-- svelte-ignore a11y-mouse-events-have-key-events -->
		<button
			type="button"
			class="chip variant-filled-primary shadow-md"
			title="Edit Structure, {row.title}"
			id="edit-{row.id}"
			on:mouseover={() => {
				helpStore.show('copy');
			}}
			on:click|preventDefault={() =>
				dispatchFn({
					type: { action: 'edit', id: row.id, title: row.title }
				})}
		>
			<Fa icon={faPen} />
		</button>
		{#if disableBtn}
		<!-- svelte-ignore a11y-mouse-events-have-key-events -->
		<button
			type="button"
			class="chip variant-filled-primary shadow-md"
			title="Delete Structure, {row.title}"
			id="delete-{row.id}"
			on:mouseover={() => {
				helpStore.show('delete');
			}}
			on:click|preventDefault={() =>
				dispatchFn({
					type: { action: 'delete', id: row.id, title: row.title }
				})}
		>
			<Fa icon={faTrash} /></button
		>
		{/if}
	</div>
</tableOption>
