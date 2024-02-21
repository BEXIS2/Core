<script lang="ts">
	import Fa from 'svelte-fa';
	import { faPen, faTrash, faLink } from '@fortawesome/free-solid-svg-icons';
	import { helpStore } from '@bexis2/bexis2-core-ui';
	import type { externalLinkType } from '$lib/components/meaning/types';

	export let row: externalLinkType;
	export let dispatchFn: any;

	let enableLink = false;
	let url = '';
	if (row.uri != '') {
		url = row.uri; // set url
		enableLink = true;

		if (row.prefix) {
			// if prefix exist, add prefix to url
			url = '' + row.prefix.url + row.uri;
		}
	}
</script>

<tableOption>
	<div class="w-18">
		<!-- svelte-ignore a11y-mouse-events-have-key-events -->
		<button
			type="button"
			class="chip variant-filled-primary shadow-md"
			title="Link, {row.name}"
			id="link-{row.id}"
			on:mouseover={() => {
				helpStore.show('link');
			}}
			on:click|preventDefault={() =>
				dispatchFn({
					type: { action: 'link', url: url }
				})}
		>
			<Fa icon={faLink} />
		</button>
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
			class="chip variant-filled-error shadow-md"
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
	</div>
</tableOption>
