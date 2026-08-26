<script lang="ts">
		import Fa from 'svelte-fa';
		import { faPen, faCopy } from '@fortawesome/free-solid-svg-icons';

	export let id = 0;
	export let version = 1;
	export let hook;


	function editFn() {
		let url = hook?.start;
		let defaultFormUrl = url + '?id=' + id + '&version=' + version;
			window.open(defaultFormUrl, '_blank')?.focus();

	}


</script>

<div class="flex-col space-y-1">
	<div class="flex gap-3 justify-left">
		<button class="chip variant-filled-secondary flex-none" on:click={editFn}
			><Fa icon={faPen} /></button
		>
	</div>
</div>