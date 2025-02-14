<script lang="ts">
	import { TablePlaceholder, ErrorMessage, host} from "@bexis2/bexis2-core-ui";
	import { getView } from './services'
	
	import TableDate from "./table/tableDate.svelte";
	import type { TagInfoViewModel } from "./types";
	import { onMount } from "svelte";
	import { numberRangeFilter } from "svelte-headless-table/plugins";

	export let id: number = 0;
 export let date:number;
	let rows:number = 3;
	$:date && reload();

	let promise:Promise<TagInfoViewModel[]>;

	onMount(() => {
		reload();
	});

	async function reload(){
		promise =  getView(id);
		const tagInfos = await promise;
		rows	= tagInfos.length;
	}

</script>


{#await promise}
	<div class="table-container w-full">
		<TablePlaceholder cols={2} {rows} />
	</div>
{:then model}
<h2 class="h2">Tag Preview</h2>
<table class="table table-compact bg-tertiary-200">
	<thead>
		<tr class="bg-primary-300">
			<th class="!p-2 w-16">Tag</th>
			<th class="!p-2">Release Note</th>
		</tr>
	</thead>
	<tbody>
		{#if model}
			{#each model as tagInfo}
				<tr>
					<td>
						<a href="{host}/ddm/data/showdata?id={id}&tag={tagInfo.version}">{tagInfo.version}</a>
					</td>
					<td>
						<div class="flex flex-col ">
							{#each tagInfo.releaseNotes as releaseNote}
							<div>{releaseNote}</div>
							{/each}
					</div>
					</td>
				</tr>
			{/each}
		{/if}
	</tbody>
</table>

{:catch error}
		<ErrorMessage {error} />
{/await}

