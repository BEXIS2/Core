<script>
import Header from './ShowHeader.svelte'


export let id; //enityid
export let title;
export let description;
export let structureId;
export let fileReaderExist; // if filereader not exist, need to set 
export let readableFiles;   // if file reader not exist, select from this files to generate a suggestion

export let variables =[];

</script>
{#if readableFiles}
	<Header {id} {structureId} {title} {description} {fileReaderExist} {readableFiles}></Header>
{/if}

{#if variables}

<!-- Responsive Container (recommended) -->
<div class="table-container py-5 max-h-[180px] overflow-auto">
	<!-- Native Table Element -->
	<table class="table table-compact bg-tertiary-20">
		<thead>
			<tr class="bg-primary-300">
				<th class="!p-2">Id</th>
				<th class="!p-2">Name</th>
				<th class="!p-2">Unit</th>
				<th class="!p-2">Datatype</th>
			</tr>
		</thead>
		<tbody>
			{#each variables as row, i}
				<tr>
					<td>{row.id}</td>
					<td>{row.name}</td>
					<td>{row.unit}</td>
					<td>{row.dataType}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>
{/if}