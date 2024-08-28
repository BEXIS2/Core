<script lang="ts">
	import { TablePlaceholder, ErrorMessage} from "@bexis2/bexis2-core-ui";
	import { getView } from './services'

	import TableDate from "./table/tableDate.svelte";
	import type { TagInfoViewModel } from "./types";
	

	let container;
	let id: number = 0;
	let taginfos:TagInfoViewModel[] = [];
	$:taginfos
	async function reload(){

		container = document.getElementById('taginfo');
		id = Number(container?.getAttribute('dataset'));

		taginfos = await getView(id);
		console.log("🚀 ~ reload ~ taginfos:", taginfos)

		return taginfos;
	}

</script>


{#await reload()}
	<div class="table-container w-full">
		<TablePlaceholder cols={7} />
	</div>
{:then model}
<table class="table table-compact bg-tertiary-200">
	<thead>
		<tr class="bg-primary-300">
			<th class="!p-2 w-16">Tag</th>
			<th class="!p-2">Release Note</th>
		</tr>
	</thead>
	<tbody>
		{#each model as tagInfo}
			<tr>
				<td>
					<a href="/ddm/show?tag={tagInfo.version}">{tagInfo.version}</a>
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
	</tbody>
</table>

{:catch error}
		<ErrorMessage {error} />
{/await}

