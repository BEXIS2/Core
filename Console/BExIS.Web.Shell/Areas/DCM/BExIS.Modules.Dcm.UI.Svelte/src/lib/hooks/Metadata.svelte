<script>
	import { host } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faPen, faCopy } from '@fortawesome/free-solid-svg-icons';

	export let id;
	export let version;

	export let status;
	export let start;

	let isEnabled = setEnable(status);
	let url = host + start + '?id=' + id + '&version=' + version;

	function setEnable(status) {
		if (status == 0 || status == 1) {
			// disabled || access denied
			return false;
		}

		return true; // every other status enable the hook
	}

	function editFn() {
		window.open(url, '_blank')?.focus();
	}

	// function copyFn() {
	// 	const copyurl = "/dcm/metadata/copy"+ '?id=' + id + '&version=' + version;
	// 	window.open(copyurl, '_blank')?.focus();
	// }

	function clickEditVisibility() {}

	function clickEditStatus() {}


</script>

<div class="flex-col space-y-1">
	<div class="flex gap-3 justify-left">
		<button class="chip variant-outline-secondary flex-none" on:click={editFn}
			><Fa icon={faPen} /></button>

		<!-- <button class="chip variant-outline-secondary flex-none " on:click={copyFn}
			><Fa icon={faCopy} /></button> -->
	</div>
</div>
