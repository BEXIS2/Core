<script>
	import { TextInput } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa/src/fa.svelte';
	import { faEye } from '@fortawesome/free-solid-svg-icons';

	export let id;
	export let version;
	export let title = '';
</script>

<!-- Header -->
<div class="flex p-5 m-2 gap-5">
	<div class="flex-none w-48 ">
		<h4 class="h4">Title</h4>
	</div>
	<div class="w-64">
		<TextInput bind:value={title} />
	</div>
	<div class="grow text-right">
		<button
			class="btn variant-ringed-secondary"
			on:click={() => window.open('/ddm/data/Showdata?id=' + id)}
		>
			<Fa icon={faEye} /></button
		>
	</div>
</div>
