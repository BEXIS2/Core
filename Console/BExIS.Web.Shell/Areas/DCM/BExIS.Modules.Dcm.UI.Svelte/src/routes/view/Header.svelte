<script lang="ts">
 import Fa from "svelte-fa";
	import { faPen } from "@fortawesome/free-solid-svg-icons";
import Citation from "./Citation.svelte";
	import License from "./License.svelte";

	export let id;
	export let version;
	export let title = '';
	export let labels:{ [key: string]: string; }	= {};
	export	let license = "";
	export let isPartOfCollection:boolean = false;
	export let hasEditRight:boolean = false;

	const labelKeys = Object.keys(labels);


</script>

<div class="flex pb-5">	
	<div class="flex flex-col flex-grow gap-2">
		 <Citation	{id} {version} />
			{#if hasEditRight}
			<div>
			<button	class="badge variant-filled-secondary mr-5" on:click={() => window.location.href = `/dcm/edit?id=${id}`}>
				<Fa icon={faPen} /> 
				<span>edit</span>
			</button>
			</div>
		{/if}
	</div>
	
	<div>


		{#if labelKeys	&& labelKeys.length > 0}
				{#each labelKeys as key}
					<span class="badge variant-filled-primary">{key}</span>
				{/each}
		{/if}

		{#if isPartOfCollection}
			<span class="badge variant-filled-success">part of collection</span>
		{/if}

		{#if license}
			<License {license} />
		{/if}
	</div>
</div>
