<script lang="ts">
	import Fa from 'svelte-fa';
	import { faCheck, faXmark } from '@fortawesome/free-solid-svg-icons';

	export let isValid: Boolean;
</script>

{#if isValid}
	<div class="text-success-500" title="is valid">
		<Fa icon={faCheck} />
	</div>
{:else}
	<div class="text-error-500" title="has errors">
		<Fa icon={faXmark} />
	</div>
{/if}
