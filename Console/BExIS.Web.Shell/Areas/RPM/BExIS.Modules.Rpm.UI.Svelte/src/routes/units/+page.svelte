<script lang="ts">

import { onMount } from 'svelte';
import { SlideToggle } from '@skeletonlabs/skeleton';
import { readable, writable } from 'svelte/store';
import { createTable, Subscribe, Render } from 'svelte-headless-table';
import { addSortBy, addColumnOrder, addColumnFilters } from 'svelte-headless-table/plugins';
import { setApiConfig }  from '@bexis2/bexis2-core-ui';
import { getUnitListItems }  from '../services/unitCaller';
import {Api} from "@bexis2/bexis2-core-ui";

import type { UnitListItem } from "../../models/Models";
	import { error } from '@sveltejs/kit';
  
let units: UnitListItem[];
$:u = units;

onMount(async () => {
  setApiConfig("https://localhost:44345","*","*");
  units = await getUnitListItems();
  console.log(units);
})

let data = writable(u);  

let table = createTable(data, {
  sort: addSortBy(),
  filter: addColumnFilters(),
});

const columns = table.createColumns([
  table.column({
    header: 'ID',
    accessor: 'id',
  }),
  table.column({
    header: 'Name',
    accessor: 'name',
  }),
  table.column({
    header: 'Description',
    accessor: 'description',
  }),
]);

const {
  headerRows,
  rows,
  tableAttrs,
  tableBodyAttrs,
} = table.createViewModel(columns);

</script>
<h1>units</h1>

{#if u}
  <table {...$tableAttrs}>
    <thead>
      {#each $headerRows as headerRow (headerRow.id)}
        <Subscribe rowAttrs={headerRow.attrs()} let:rowAttrs>
          <tr {...rowAttrs}>
            {#each headerRow.cells as cell (cell.id)}
              <Subscribe attrs={cell.attrs()} let:attrs>
                <th {...attrs}>
                  <Render of={cell.render()} />
                </th>
              </Subscribe>
            {/each}
          </tr>
        </Subscribe>
      {/each}
    </thead>
    <tbody {...$tableBodyAttrs}>
      {#each $rows as row (row.id)}
        <Subscribe rowAttrs={row.attrs()} let:rowAttrs>
          <tr {...rowAttrs}>
            {#each row.cells as cell (cell.id)}
              <Subscribe attrs={cell.attrs()} let:attrs>
                <td {...attrs}>
                  <Render of={cell.render()} />
                </td>
              </Subscribe>
            {/each}
          </tr>
        </Subscribe>
      {/each}
    </tbody>
  </table>
{/if}




