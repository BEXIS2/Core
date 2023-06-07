<script lang="ts">
	import { writable } from 'svelte/store';
	import {
		createTable,
		Subscribe,
		Render,
		createRender,
		type Constructor
	} from 'svelte-headless-table';
	import {
		addSortBy,
		addPagination,
		addExpandedRows,
		addColumnFilters,
		addTableFilter
	} from 'svelte-headless-table/plugins';

	import TableFilter from './TableFilter.svelte';
	import TablePagination from './TablePagination.svelte';
	import { columnFilter, searchFilter } from './filter';
	import type { SvelteComponentDev } from 'svelte/internal';

	export let data;
	export let optionsComponent: Constructor<SvelteComponentDev> | null = null;
	export let filterComponent = TableFilter;
	export let columnFilterFn = columnFilter;
	export let excluded: AccessorType[] = [];
	export let defaultPageSize = 10;
	export let pageSizes = [5, 10, 15, 20];

	type AccessorType = keyof (typeof data)[0];

	const types = {
		id: 'numeric',
		name: 'text',
		description: 'text'
	};
	const filteredData = writable(data);

	const table = createTable(filteredData, {
		colFilter: addColumnFilters(),
		tableFilter: addTableFilter({
			fn: searchFilter
		}),
		sort: addSortBy({ disableMultiSort: true }),
		page: addPagination({ initialPageSize: defaultPageSize }),
		expand: addExpandedRows()
	});

	const accessors: AccessorType[] = Object.keys(data[0]) as AccessorType[];

	const tableColumns = [
		...accessors
			.filter((key) => !excluded.includes(key as string))
			.map((item) => {
				return table.column({
					header: item,
					accessor: item,
					plugins: {
						sort: { invert: true },
						colFilter: {
							fn: columnFilterFn,
							render: ({ filterValue, values }) =>
								createRender(filterComponent, { filterValue, values })
						}
					}
				} as any);
			})
	];

	if (optionsComponent !== null) {
		tableColumns.push(
			table.display({
				id: 'options',
				header: '',
				cell: ({ row }, _) => {
					return createRender(optionsComponent!, {
						row: row.isData() ? row.original : null
					});
				}
			}) as any
		);
	}
	const columns = table.createColumns(tableColumns);

	const { headerRows, pageRows, tableAttrs, tableBodyAttrs, pluginStates } =
		table.createViewModel(columns);
	const { filterValue } = pluginStates.tableFilter;
</script>

<div class="grid gap-2">
	<div class="table-container">
		<input
			class="input p-2 mb-2 border border-primary-500"
			type="text"
			bind:value={$filterValue}
			placeholder="Search rows..."
		/>
		<table {...$tableAttrs} class="table table-compact bg-tertiary-200">
			<thead>
				{#each $headerRows as headerRow (headerRow.id)}
					<Subscribe
						rowAttrs={headerRow.attrs()}
						let:rowAttrs
						rowProps={headerRow.props()}
						let:rowProps
					>
						<tr {...rowAttrs} class="bg-primary-300">
							{#each headerRow.cells as cell (cell.id)}
								<Subscribe attrs={cell.attrs()} props={cell.props()} let:props let:attrs>
									<th scope="col" class="!p-2" {...attrs}>
										<div class="flex w-full justify-between items-center">
											<div class="flex gap-1">
												<span
													class:underline={props.sort.order}
													on:click={props.sort.toggle}
													on:keydown={props.sort.toggle}
												>
													{cell.render()}
												</span>
												<div class="w-2">
													{#if props.sort.order === 'asc'}
														▾
													{:else if props.sort.order === 'desc'}
														▴
													{/if}
												</div>
											</div>
											{#if cell.isData()}
												{#if props.colFilter?.render}
													<div>
														<Render of={props.colFilter.render} />
													</div>
												{/if}
											{/if}
										</div>
									</th>
								</Subscribe>
							{/each}
						</tr>
					</Subscribe>
				{/each}
			</thead>

			<tbody class="" {...$tableBodyAttrs}>
				{#each $pageRows as row (row.id)}
					<Subscribe rowAttrs={row.attrs()} let:rowAttrs>
						<tr {...rowAttrs}>
							{#each row.cells as cell (cell?.id)}
								<Subscribe attrs={cell.attrs()} let:attrs>
									<td {...attrs} class="!p-2">
										<div class="flex items-center w-full h-full table-cell-fit">
											<Render of={cell.render()} />
										</div>
									</td>
								</Subscribe>
							{/each}
						</tr>
					</Subscribe>
				{/each}
			</tbody>
		</table>
	</div>

	<TablePagination pageConfig={pluginStates.page} {pageSizes} />
</div>
