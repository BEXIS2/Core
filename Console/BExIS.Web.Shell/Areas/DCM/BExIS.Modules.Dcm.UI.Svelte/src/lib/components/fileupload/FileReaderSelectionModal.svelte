<script lang="ts">
	import { createEventDispatcher } from 'svelte';

	import Fa from 'svelte-fa';
	import { faXmark } from '@fortawesome/free-solid-svg-icons';
	import type { StructureSuggestionModel } from '$models/StructureSuggestion';

	import Selection from '../structuresuggestion/Selection.svelte';
	import { latestFileReaderDate } from '../../../routes/edit/stores';

	export let model: StructureSuggestionModel;
	$: model, open();

	import { Drawer, drawerStore } from '@skeletonlabs/skeleton';
	import type { DrawerSettings } from '@skeletonlabs/skeleton';

	const dispatch = createEventDispatcher();

	function open(): void {
		const drawerSettings: DrawerSettings = {
			id: 'example-2',
			meta: { foo: 'bar', fizz: 'buzz', age: 40 }
		};
		drawerStore.open(drawerSettings);
	}

	function close() {
		console.log('close selection');

		latestFileReaderDate.set(Date.now());
		drawerStore.close();
		dispatch('close');
	}
</script>

<Drawer position="right" width="10" on:backdrop={close} on:touchend={close}>
	<div class="p-5 space-y-5">
		<div class="flex">
			<div class="grow"><h2 class="h2">File reader informations</h2></div>
			<div class="text-right flex-none w-15">
				<button class="chip variant-filled-warning" on:click={close}>
					<Fa icon={faXmark} />
				</button>
			</div>
		</div>

		<Selection {model} on:saved={close} />
	</div>
</Drawer>
