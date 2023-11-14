<script lang="ts">
	import { createEventDispatcher } from 'svelte';

	import Fa from 'svelte-fa';
	import { faXmark } from '@fortawesome/free-solid-svg-icons';
	import type { DataStructureCreationModel } from '@bexis2/bexis2-rpm-ui';

	import {Selection} from '@bexis2/bexis2-rpm-ui';
	import { latestFileReaderDate } from '../../../routes/edit/stores';

	export let model: DataStructureCreationModel;
	$: model, open();

	import { Drawer, getDrawerStore } from '@skeletonlabs/skeleton';
	const drawerStore = getDrawerStore();
	import type { DrawerSettings } from '@skeletonlabs/skeleton';


	const dispatch = createEventDispatcher();

	function open(): void {
		console.log("open",model)
		const drawerSettings: DrawerSettings = {
			id: 'example-2',
			meta: { foo: 'bar', fizz: 'buzz', age: 40 }
		};
		drawerStore.open(drawerSettings);
	}

	function close() {
		console.log('close selection',$latestFileReaderDate);

		latestFileReaderDate.set(Date.now());
		drawerStore.close();
		dispatch('close');
		console.log('close selection after update',$latestFileReaderDate);

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

		<Selection {model} on:saved={close} on:error={alert("error")} />
	</div>
</Drawer>
