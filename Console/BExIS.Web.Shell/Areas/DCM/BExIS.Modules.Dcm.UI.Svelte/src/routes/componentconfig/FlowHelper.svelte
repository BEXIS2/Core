<script lang="ts">
	import { useSvelteFlow } from '@xyflow/svelte';
	import { onMount } from 'svelte';

	export let onReady: (setCenter: (x: number, y: number, zoom?: number) => void) => void = () => {};

	const { setCenter } = useSvelteFlow();

	onMount(() => {
		onReady((x: number, y: number, zoom?: number) => {
			setCenter(x, y, { zoom: zoom ?? 1, duration: 300 });
		});
	});
</script>
