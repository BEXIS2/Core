<script>
	import { TextInput } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa/src/fa.svelte';
	import { faEye } from '@fortawesome/free-solid-svg-icons';
	import { SlideToggle } from '@skeletonlabs/skeleton';

	export let id;
	export let version;
	export let title = '';
</script>

<!-- Header -->
<div class="relative w-full ">

	 <div class="absolute top-0 right-0 flex text-center gap-4">
			<span>switch to view</span>
			<SlideToggle active="bg-secondary-500" name="ViewToggle" size="sm" checked={true} on:click={() => window.open('/ddm/data/Showdata?id=' + id, "_self")} />
		</div>
</div>
