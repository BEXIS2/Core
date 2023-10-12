<script lang="ts">

 import type {externalLinkType} from './types'


	import Link from './ExternalLink.svelte';
	export let list:externalLinkType[] = [];

	import { onMount } from 'svelte';

	onMount(() => {
		if (list.length === 0) {
			add();
		}
	});

	function remove(i) {
		list = [...list.filter((m, index) => i !== index)];

		if (list.length === 0) {
			add();
		}
	}

	function add() {
		console.log('list', list);

		let newLink:externalLinkType = {
			name: "",
			type:"",
			uRI:""
		};

		list = [...list, newLink];
		console.log('list', list);
	}
</script>

<label><b>External Links</b></label>

<div class="link-container">
	{#if list}
		{#each list as item, i}
			<!-- content here -->
			<Link
				bind:name={item.name}
				bind:type={item.type}
				bind:uRI={item.uRI}
				on:add={add}
				on:remove={() => remove(i)}
				last={list.length - 1 === i}
			/>
		{/each}
	{/if}
</div>

<style>
	.link-container {
		padding-bottom: 1em;
	}
</style>
