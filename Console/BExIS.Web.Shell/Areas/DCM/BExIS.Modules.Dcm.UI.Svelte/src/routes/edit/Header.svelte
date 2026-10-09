<script>
	import { TextInput } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faEye } from '@fortawesome/free-solid-svg-icons';
	import { SlideToggle } from '@skeletonlabs/skeleton';

	export let id;
	export let version;
	export let title = '';
</script>

<!-- Header -->
<div class="relative w-full">
	<div class="absolute top-0 right-0 flex text-center gap-4">
		<button
			class="btn btn-sm variant-filled-secondary text-center"
			on:click={() => window.open('/dcm/view?id=' + id, '_self')}
		>
			<span><Fa icon={faEye} /></span>
			<span>View</span>
		</button>
	</div>
</div>
