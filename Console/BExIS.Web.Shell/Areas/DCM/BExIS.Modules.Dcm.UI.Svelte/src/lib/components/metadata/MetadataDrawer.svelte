<script lang="ts">
	import IFrame from '../IFrame.svelte';
	import { Drawer, getDrawerStore } from '@skeletonlabs/skeleton';
	import { createEventDispatcher, onMount } from 'svelte';
	import type { DrawerSettings } from '@skeletonlabs/skeleton';
	import { getToken } from '$services/BaseCaller';
	import Fa from 'svelte-fa';
	import { faXmark } from '@fortawesome/free-solid-svg-icons';

	export let id = 0;
	export let url = '';

	let metadataView = '';

	$: metadataView;

	const dispatch = createEventDispatcher();
	const drawerStore = getDrawerStore();
	let token = '';

	onMount(() => {
		open();
	});

	async function open() {
		token = await getToken();
		url = url + '?id=' + id + '&auth=' + token;
		const drawerSettings: DrawerSettings = {
			id: 'external-form',
			meta: { foo: 'bar', fizz: 'buzz', age: 40 }
		};
		drawerStore.open(drawerSettings);
	}

	window.addEventListener('message', function (event) {
		console.log('message', event.data);
		if (event.data === 'saved') {
			close();
		}
	});

	function close() {
		drawerStore.close();
		dispatch('close');
	}
</script>

<div id="metadata-drawer-container" >
<Drawer  position="right" width="w-11/12" on:backdrop={close} on:touchend={close} >
	<!-- <div class="">
		<div class="flex">
			<div class="grow"></div>
			<div class="text-right flex-none w-15 h-10 p-2">
				<button class="chip variant-filled-warning " on:click={close}>
					<Fa icon={faXmark} />
				</button>
			</div>
		</div> -->

		<IFrame app="test" {url} on:close={close}></IFrame>
	<!-- </div> -->
</Drawer>
</div>

