<script>
import Header from './ShowHeader.svelte'


export let id; //enityid
export let title;
export let description;
export let structureId;

export let variables =[];

</script>

<Header {id} {structureId} {title} {description} ></Header>

{#if variables}

<!-- Responsive Container (recommended) -->
<div class="table-container">
	<!-- Native Table Element -->
	<table class="table table-compact">
		<thead>
			<tr>
				<th>Id</th>
				<th>Name</th>
				<th>Unit</th>
				<th>Datatype</th>
			</tr>
		</thead>
		<tbody>
			{#each variables as row, i}
				<tr>
					<td>{row.id}</td>
					<td>{row.name}</td>
					<td>{row.unit}</td>
					<td>{row.Datatype}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>
{/if}