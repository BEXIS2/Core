<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { host } from '@bexis2/bexis2-core-ui';

	//entity infos
	export let id: number;
	export let version: number;

	//entity view infos
	export let name: string;
	export let displayName: string;
	export let start: string;

	$: ExtComponent = null;

	onMount(async () => {
		//load javascript from server
		const urlscript = host + start + '?id=' + id + '&&version=' + version;
		console.log("🚀 ~ urlscript:", urlscript)

		import(urlscript).then((resp) => {
			ExtComponent = resp.default;
		});
	});
</script>

<div id="{name}_view">
	<div id={name} dataset={id} {version} />
	<svelte:component this={ExtComponent} />
</div>
