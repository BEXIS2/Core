<script lang="ts">
	import Fa from 'svelte-fa';
	import { faXmark } from '@fortawesome/free-solid-svg-icons';
import type { StructureSuggestionModel } from '$models/StructureSuggestion';

import Selection from '../structuresuggestion/Selection.svelte';
import {latestDataDescriptionDate } from '../../../routes/edit/stores';

export let model:StructureSuggestionModel;


import { Drawer, drawerStore } from '@skeletonlabs/skeleton';
import type { DrawerSettings } from '@skeletonlabs/skeleton';

open();

function open(): void {
		// const c: ModalComponent = { ref: Selection,props: { model } };

		// const modal: ModalSettings = {
		// 	type: 'component',
		// 	component: c
		// };
		// modalStore.trigger(modal);
		const drawerSettings: DrawerSettings = {
				id: 'example-2',
				meta: { foo: 'bar', fizz: 'buzz', age: 40 }
			};
			drawerStore.open(drawerSettings);
	}

  function close()
  {
    latestDataDescriptionDate.set(Date.now());
				drawerStore.close();
  }

</script>


<Drawer position="right" width="10">
	<div class="p-5 space-y-5">
		<h2 class="h2">File reader informations</h2>
	 <button class="chip variant-filled-warning" on:click={close}><Fa icon={faXmark}></Fa></button>
		<Selection {model}  on:saved={close}/>
	</div>
</Drawer>