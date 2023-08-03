<script>
	import { host } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
import {faPen} from '@fortawesome/free-solid-svg-icons'

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

	function clickEditMetadata() {
		window.open(url, '_blank')?.focus();
	}

	function clickEditVisibility() {}

	function clickEditStatus() {}
</script>

<div class="flex-col space-y-1">
	<div class="flex gap-3 justify-left">	
		<div class="flex-none w-20"><b>Metadata</b></div>
		<button class="btn bg-secondary-500 text-white flex-none btn-sm" on:click={clickEditMetadata}><Fa icon={faPen}></Fa></button>
		<div ><span>Last Modfied</span></div>
	</div>
	<div class="flex gap-3 justify-left">	
		<b class="flex-none w-20">Visibility</b>
		<!--stats-->
		<span class="self-center"> not implemented</span>
		<!--stats-->
		<button class="btn bg-secondary-500 text-white btn-sm flex-none" on:click={clickEditVisibility}
			><Fa icon={faPen}></Fa></button>
	</div>
	<div class="flex gap-3 justify-left">	
		<b class="flex-none w-20">Status</b>
		<!--stats-->
		<span class="flex-none"> not implemented</span>
		<!--stats-->
		<button class="btn bg-secondary-500 text-white btn-sm flex-none" on:click={clickEditStatus}
			><Fa icon={faPen}></Fa></button		>
		</div>
</div>
