<script lang="ts">
	import { Spinner } from '@bexis2/bexis2-core-ui';

	export let label: string | undefined = undefined;
</script>

<div
	class="absolute right-0 top-0 z-10 flex h-full w-full items-center justify-center bg-surface-200 opacity-50"
>
	<div class="flex h-full w-full items-center justify-center">
		<Spinner {label} />
	</div>
</div>
