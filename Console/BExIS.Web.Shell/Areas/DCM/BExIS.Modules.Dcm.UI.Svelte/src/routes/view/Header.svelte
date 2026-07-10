<script lang="ts">
 import Fa from "svelte-fa";
	import { faPen } from "@fortawesome/free-solid-svg-icons";
import Citation from "./Citation.svelte";
	import License from "./License.svelte";
	import { goTo } from "$services/BaseCaller";


	export let id;
	export let version;
	export	let tag;
	export let title = '';
	export let labels:{ [key: string]: string; }	= {};
	export	let license = "";
	export let isPartOfCollection:boolean = false;
	export let hasEditRight:boolean = false;
	export let isPublic:boolean = false;
	export let publicationDate:string	= '';

	const labelKeys = Object.keys(labels);
	console.log("🚀 ~ labels:", labels)

</script>

<div class="flex flex-col gap-2">	

	<div class="flex justify-items-center">
			{#if hasEditRight}
			<div>
			<button	class="badge variant-filled-secondary mr-5" on:click={() => window.location.href = `/dcm/edit?id=${id}`}>
				<Fa icon={faPen} /> 
				<span>edit</span>
			</button>
			</div>
		{/if}
	

		<div class="flex grow justify-end gap-1">
			{#if isPublic}
				<div class="mt-1 mr-2 italic underline">
					<span>Published {new Date(publicationDate).toLocaleDateString()}</span>
				</div>
			{/if}
			{#if labelKeys	&& labelKeys.length > 0}
					{#each labelKeys as key}
					 {#if labels[key] === 'DOI'}
							<a href={key} class="badge variant-ghost-primary" target="_blank">{key}</a>
						{:else}
							<span class="badge variant-filled-primary">{key}</span>
							{/if}
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




	<div class="flex flex-col gap-2">
		 <Citation	{id} {version} {tag} />
	</div>

	

</div>
