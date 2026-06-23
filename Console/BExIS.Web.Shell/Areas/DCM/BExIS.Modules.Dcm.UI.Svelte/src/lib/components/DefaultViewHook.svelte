<!-- 
  Disabled = 0, //Function behind the hook is deactivated, but shown       
  AccessDenied = 1, // User has no access to the function behind the hook
  Open = 2, // Function behind the hook is ready for use
  Ready = 3, // Function behind the hook was successfully completed
  Exist = 4 // Data behind the hook exists
  InActive = 5 // Data behind the hook exists
-->

<script lang="ts">

  import Fa from 'svelte-fa';
  import { faPen, faCopy } from '@fortawesome/free-solid-svg-icons';

		//prepare drawer
		import { Drawer, getDrawerStore } from '@skeletonlabs/skeleton';
		const drawerStore = getDrawerStore();
		import type { DrawerSettings } from '@skeletonlabs/skeleton';

		export let id = 0;
		export let version = 1;
		export let start='';


	export let status = 0;
  export let description = '';

	const isEnabled = setEnable(status);
	const active = setActive(status);

	// console.log(active);

	// let url = hosturl+start+"?id="+id+"&version="+version;

	function setEnable(status) {
		if (status == 0 || status == 1) {
			// disabled || access denied
			return false;
		}

		return true; // every other status enable the hook
	}

	function setActive(status) {
		if (status == 0 || status == 1 || status == 5) {
			// disabled || access denied || inactive
			return false;
		}

		return true; // every other status enable the hook
	}

  function editFn() {
		let url = start;
		let defaultFormUrl = url + '?id=' + id + '&version=' + version;
			window.open(defaultFormUrl, '_blank')?.focus();

	}

</script>

{#if isEnabled}
	<div class="mb-3 hook-status-{status} hook" class:inactive={!active} title={description} on:click>
    <button class="chip variant-filled-secondary flex-none" on:click={editFn}><Fa icon={faPen} /></button>
	</div>



{/if}
