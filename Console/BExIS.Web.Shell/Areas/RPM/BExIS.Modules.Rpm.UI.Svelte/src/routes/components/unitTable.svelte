<script lang="ts">

    import { writable } from 'svelte/store';
    import { createTable, Subscribe, Render } from 'svelte-headless-table';
    import { addSortBy, addColumnOrder, addColumnFilters } from 'svelte-headless-table/plugins';
    
    import type { UnitListItem } from "../../models/Models";
    
    export let units:UnitListItem[];

    let data = writable(units);  
    
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
      table.column({
        header: 'Abbreviation',
        accessor: 'abbreviation',
      }),
      table.column({
        header: 'Dimension',
        accessor: 'dimension',
      }),
      table.column({
        header: 'Datatypes',
        accessor: 'datatypes',
      }),
      table.column({
        header: 'Measurement System',
        accessor: 'measurementSystem',
      }),
    ]);
    
    const {
      headerRows,
      rows,
      tableAttrs,
      tableBodyAttrs,
    } = table.createViewModel(columns);
    
</script>

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