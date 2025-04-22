<script lang="ts">
	import { onMount, afterUpdate, onDestroy } from 'svelte';

	export let app;
	export let url;

	let loaded = false;

	onMount(() => {
		console.log(`Mounted ${app}`);
		loaded = true;
		// if window change, resize the iframe
		window.addEventListener('resize', sendHeight);
	});

	afterUpdate(() => {
		console.log(`Updated ${app}`);
	});

	onDestroy(() => {
		console.log(`Destroyed ${app}`);
	});

	function sendHeight() {
		// get windows height
		const height = window.innerHeight;
		// get iframe id
		const iframeid = 'microapp-' + app;
		// set iframe height
		document.getElementById(iframeid).style.height = height + 'px';
	}
</script>

{#if loaded}
	<iframe
		title={app}
		id="microapp-{app}"
		frameborder="0"
		scrolling="auto"
		src={url}
		on:load={sendHeight}
	></iframe>
{/if}

<style>
	iframe {
		width: 100%;
	}
</style>
