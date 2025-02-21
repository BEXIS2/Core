<script lang="ts">
	import Select from 'svelte-select';
	import Fa from 'svelte-fa';
	import { PaneGroup, Pane, PaneResizer } from 'paneforge';
	import { faFilter, faTable, faBars, faGripVertical } from '@fortawesome/free-solid-svg-icons';
	import { onMount } from 'svelte';
	import { writable } from 'svelte/store';
	import { RadioGroup, RadioItem, SlideToggle } from '@skeletonlabs/skeleton';
	import { Api, Facets, Page, pageContentLayoutType, Table } from '@bexis2/bexis2-core-ui';
	import type { Columns, FacetGroup, TableConfig } from '@bexis2/bexis2-core-ui';

	import ShowData from '$lib/components/ShowData.svelte';
	import Cards from '$lib/components/Cards.svelte';
	import CriteriaChip from '$lib/components/CriteriaChip.svelte';
	import { convertTableData } from '$lib/helpers';

	let columns: Columns;
	let config: TableConfig<any>;
	let categories: { [key: string]: string } = {};
	let currentCategory = 'All';
	let q = '';

	let currentView: 'table' | 'cards' = 'table';

	let facetsRef: any;

	let mapFiltersOpen = false;
	let timeFiltersOpen = false;
	let filtersOpen = false;

	const criteria = writable<{
		[key: string]: {
			values: string[];
			displayName: string;
			type: 'Facet' | 'Category';
			operation?: 'OR' | 'AND';
		};
	}>({});
	const facetGroups = writable<FacetGroup[]>([]);
	const tableStore = writable<any>([]);
	const placeholderStore = writable<any>([]);

	const handleSearch = async (init: boolean = false) => {
		const response = await Api.post(
			'/ddm/search/Query',
			init
				? {
						searchType: 'new'
					}
				: {
						autoComplete: q,
						FilterList: currentCategory,
						searchType: 'basedon'
					}
		);

		if (response.status !== 200) {
			console.error(response);
			return;
		}

		const currentCategoryDisplayName =
			Object.keys(categories).find((c) => c === currentCategory) || currentCategory;

		if (q.length !== 0) {
			$criteria = {
				...$criteria,
				[currentCategory]: {
					values: [q],
					displayName: currentCategoryDisplayName,
					type: 'Category'
				}
			};
		}

		const responseObject = response.data;

		// Mapping categories
		if (init) {
			categories = responseObject.SearchComponent.Categories.reduce((acc: any, cur: any) => {
				acc[cur.Name] = cur.DisplayName;
				return acc;
			}, {});
		}

		// Mapping and updating table data
		const {
			Rows: rows,
			Header: header,
			DefaultVisibleHeaderItem: visibleHeaders
		} = responseObject.ResultComponent;

		mapTable({ rows, header, visibleHeaders, init });
		mapFacets(
			responseObject.SearchComponent.Facets,
			responseObject.CriteriaComponent.SearchCriteriaList
		);
		mapPlaceholders(header, rows);
	};

	const toggleFacet = async (parent: string, selectedItem: string) => {
		if ($criteria[parent]?.values.includes(selectedItem)) {
			deleteCriteriaKey(parent, selectedItem);
		}

		const response = await Api.post('/ddm/search/ToggleFacet', {
			selectedItem,
			parent
		});

		if (response.status !== 200) {
			console.error(response);
			return;
		}

		const responseObject = response.data;

		// Mapping and updating table data
		const {
			Rows: rows,
			Header: header,
			DefaultVisibleHeaderItem: visibleHeaders
		} = responseObject.ResultComponent;

		mapTable({ rows, header, visibleHeaders });
		mapFacets(
			responseObject.SearchComponent.Facets,
			responseObject.CriteriaComponent.SearchCriteriaList
		);
		mapPlaceholders(header, rows);
	};

	const mapTable = ({
		rows,
		header,
		visibleHeaders,
		init = false
	}: {
		rows: any[];
		header: any[];
		visibleHeaders: any[];
		init?: boolean;
	}) => {
		const mapping = convertTableData(columns, rows, header, visibleHeaders);
		tableStore.set(mapping.data);

		if (init) {
			columns = mapping.columns;

			config = {
				id: 'search-table',
				data: tableStore,
				search: false,
				optionsComponent: ShowData as any,
				columns
			};
		}
	};

	const mapFacets = (facets: any, criteria: any) => {
		const appliedFacets = criteria.filter((criterion: any) => criterion.SearchComponent.Type === 1);

		let appliedFacetsDict: {
			[key: string]: {
				name: string;
				displayName: string;
				values: string[];
				operation: 'OR' | 'AND';
			};
		} = {};

		if (appliedFacets.length !== 0) {
			appliedFacetsDict = appliedFacets.reduce((acc: any, criterion: any) => {
				acc[criterion.SearchComponent.Name] = {
					name: criterion.SearchComponent.Name,
					displayName: criterion.SearchComponent.DisplayName,
					values: criterion.Values,
					operation: criterion.ValueSearchOperation
				};
				return acc;
			}, {});
		} else {
			$criteria = Object.keys($criteria).reduce((acc, key) => {
				if ($criteria[key].type === 'Facet') {
					delete acc[key];
				}
				return acc;
			}, $criteria);
		}

		$facetGroups = facets.map((facet: any) => {
			const children: any[] = [];
			facet.Childrens.forEach((child: any) => {
				children.push({
					name: child.Name,
					displayName: child.DisplayName || child.Text || child.Value || child.Name,
					count: child.Count,
					selected: appliedFacetsDict[facet.Name]?.values.includes(child.Name) || false
				});
			});

			console.log(appliedFacetsDict);

			if (appliedFacetsDict[facet.Name]) {
				$criteria = {
					...$criteria,
					[facet.Name]: {
						values: appliedFacetsDict[facet.Name]?.values,
						displayName: facet.DisplayName,
						type: 'Facet',
						operation: appliedFacetsDict[facet.Name]?.operation || 'OR'
					}
				};
			}

			return {
				name: facet.Name,
				displayName: facet.DisplayName,
				count: facet.Count,
				children
			};
		}, []);
	};

	const mapPlaceholders = (headers: any[], rows: any[]) => {
		const placeholders = headers
			.filter((header) => header.Placeholder && header.Placeholder !== '')
			.map((header, index) => ({
				header: header.DisplayName,
				placeholder: header.Placeholder,
				index: headers.indexOf(header)
			}));

		// For debugging
		// const placeholders = [
		// 	{ header: 'Title', placeholder: 'title', index: 2 },
		// 	{ header: 'Description', placeholder: 'description', index: 5 },
		// 	{ header: 'Author', placeholder: 'author', index: 4 },
		// 	{ header: 'Date', placeholder: 'date', index: 3 }
		// ];

		if (placeholders.length > 0) {
			const idIndex = headers.findIndex((header) => header.Name === 'ID');

			const data = rows.map((row) => ({
				...placeholders.reduce(
					(acc, item) => {
						if (item) {
							acc[item.placeholder] = row.Values[item.index];
						}
						return acc;
					},
					{} as { [key: string]: string }
				),
				id: row.Values[idIndex]
			}));
			placeholderStore.set(data);
		}
	};

	const deleteCriteriaKey = (criterion: string, value: string) => {
		const temp = { ...$criteria };
		temp[criterion].values = temp[criterion].values.filter((v) => v !== value);
		if (temp[criterion].values.length === 0) {
			delete temp[criterion];
		}
		$criteria = temp;
	};

	const removeCriterion = async (criterion: string, value: string) => {
		const response = await Api.post('/ddm/search/RemoveSearchCriteria', {
			value,
			parent: criterion
		});

		if (response.status !== 200) {
			console.error(response);
			return;
		}

		deleteCriteriaKey(criterion, value);

		const responseObject = response.data;

		// Mapping and updating table data
		const {
			Rows: rows,
			Header: header,
			DefaultVisibleHeaderItem: visibleHeaders
		} = responseObject.ResultComponent;

		mapTable({ rows, header, visibleHeaders });
		mapFacets(
			responseObject.SearchComponent.Facets,
			responseObject.CriteriaComponent.SearchCriteriaList
		);
		mapPlaceholders(header, rows);
	};

	const handleShowMoreSelect = async (e: { detail: { parent: string; selected: boolean[] }[] }) => {
		const selected = e.detail[0].selected;

		console.log(selected);

		const formBody = new URLSearchParams();
		selected.forEach((value, index) => {
			formBody.append(index.toString(), value.toString());
		});

		const response = await fetch('/ddm/search/AddFacetsToSearch', {
			method: 'POST',
			headers: {
				'Content-Type': 'application/x-www-form-urlencoded'
			},
			body: formBody
		});

		if (response.status !== 200) {
			console.error(response);
			return;
		}

		const responseObject = await response.json();

		// Mapping and updating table data
		const {
			Rows: rows,
			Header: header,
			DefaultVisibleHeaderItem: visibleHeaders
		} = responseObject.ResultComponent;

		mapTable({ rows, header, visibleHeaders });
		mapFacets(
			responseObject.SearchComponent.Facets,
			responseObject.CriteriaComponent.SearchCriteriaList
		);
		mapPlaceholders(header, rows);
	};

	const handleFacetSelect = async (e: { detail: { parent: string; selectedItem: string }[] }) => {
		await toggleFacet(e.detail[0].parent, e.detail[0].selectedItem);
	};

	const handleShowMoreOpenChange = async (e: { detail: { group: string; open: boolean } }) => {
		const { group: parent, open } = e.detail;

		if (!open) return;

		const response = await Api.post('/ddm/search/ShowMoreWindow', {
			parent
		});

		if (response.status !== 200) {
			console.error(response);
			return;
		}

		const responseObject = response.data;

		// Mapping and updating table data
		const {
			Rows: rows,
			Header: header,
			DefaultVisibleHeaderItem: visibleHeaders
		} = responseObject.ResultComponent;

		mapTable({ rows, header, visibleHeaders });
		mapFacets(
			responseObject.SearchComponent.Facets,
			responseObject.CriteriaComponent.SearchCriteriaList
		);
		mapPlaceholders(header, rows);
	};

	const setAutoCompleteValues = async (input: string) => {
		if (input.length < 3) return [];

		const response = await Api.post('/ddm/search/_AutoCompleteAjaxLoading', {
			text: input
		});

		const data = response.data;

		return data.map((item: any) => ({
			label: item.Text,
			value: item.Value
		}));
	};

	const load = async (input: string) => {

		const data = [
			{	Text: "a",	Value: "a"},
			{	Text: "b",	Value: "b"},
			{	Text: "c",	Value: "c"},
			{	Text: "d",	Value: "d"},
			{	Text: "e",	Value: "e"},
			{	Text: "f",	Value: "f"},
			{	Text: "g",	Value: "g"}
		]


		return data.map((item: any) => ({
			label: item.Text,
			value: item.Value
		}));
	};

	const handleAutoCompleteSelect = async (e: { detail: { value: string; label: string } }) => {
		q = e.detail.value;

		return null;
	};

	onMount(async () => {
		await handleSearch(true);
	});
</script>

<Page
	title="Search"
	note="Search over the data in this system."
	contentLayoutType={pageContentLayoutType.full}
>
	<PaneGroup direction="horizontal" class="w-full !text-sm lg:text-base">
		<!-- Facets -->
		<Pane defaultSize={100} class="min-w-[175px]">
			<div class="pr-2">
				<div
					class="{filtersOpen
						? 'min-w-32'
						: 'min-w-32'} overflow-hidden flex flex-col gap-4 rounded-md"
				>
					<span class="flex gap-4 items-center text-primary-700 text-2xl font-semibold">
						<Fa icon={faFilter} size="xs" />
						<span>Filters</span>
					</span>
					<div class=" overflow-auto">
						{#if $facetGroups.length > 0}
							<Facets
								groups={facetGroups}
								open
								bind:this={facetsRef}
								on:facetSelect={handleFacetSelect}
								on:showMoreSelect={handleShowMoreSelect}
								on:showMoreOpenChange={handleShowMoreOpenChange}
							/>
						{/if}
					</div>
				</div>
			</div>
		</Pane>

		<PaneResizer class="relative flex w-4 items-center justify-center bg-background h-full">
			<div class="absolute w-[1px] h-full bg-neutral-200"></div>
		</PaneResizer>

		<!-- Table/Cards -->
		<Pane defaultSize={500} class="flex grow w-full px-2">

			
			<div class="flex flex-col gap-4 w-full">
				<!-- options -->
				<div class="flex items-center justify-end w-full gap-16 p-2 bg-neutral-50 rounded-lg border-neutral-200 border px-6">
					<div class="w-min flex h-full" title="Switch between table and card view">
						<div class="flex gap-3 items-center">
							<span>Layout: </span>
							<RadioGroup
								active="variant-filled-primary"
								hover="hover:variant-soft-primary"
								padding="py-1 px-2"
								rounded="rounded-full"
							>
								<RadioItem
									bind:group={currentView}
									name="currentView"
									value="table"
									title="Table view"
								>
									<Fa icon={faTable} size="xs" />
								</RadioItem>
								<RadioItem
									bind:group={currentView}
									name="currentView"
									value="cards"
									title="Card view"
								>
									<Fa icon={faBars} size="xs" />
								</RadioItem>
							</RadioGroup>
						</div>
					</div>
				</div>


			<div class="flex flex-col gap-4 ">
				<div class="flex flex-col gap-4">
					<div class="flex gap-4 items-start">
							<div class="flex gap-4 h-min items-stretch">
								<select
									class="bg-input rounded-md px-4 pr-7 py-2 text-sm w-min border-neutral-300"
									bind:value={currentCategory}
									title="Filter categories"
								>
									{#each Object.keys(categories) as category (category)}
										<option value={category}>{categories[category]}</option>
									{/each}
								</select>
								<div class="flex gap-4 items-center grow">
									<Select
										loadOptions={setAutoCompleteValues}
										class="input rounded-md !border-neutral-300 grow max-w-[500px] min-w-[300px]"
										name="search"
										bind:filterText={q}
										on:select={handleAutoCompleteSelect}
										clearFilterTextOnBlur={false}
										hideEmptyState={true}
										clearable={false}
										value={undefined}
										placeholder="Search within selected category"
									/>
									<button
										title="Search"
										class="btn variant-filled-primary"
										on:click|preventDefault={async () => await handleSearch()}>Search</button
									>
								</div>
							</div>
						</div>
					</div>

					<!-- Criteria and applied search queries -->
					<div class="flex grow w-full item-start">
						<div class="flex gap-4 w-96 grow overflow-auto">
							{#each Object.keys($criteria) as key, index (key)}
								{#if $criteria[key].values.length > 0}
									<div class="flex items-center gap-4">
										<div class="w-min font-bold text-nowrap text-xs">
											{$criteria[key].displayName}:
										</div>

										{#if $criteria[key].values.length < 3}
											{#each $criteria[key].values as value, index (`${key}-${value}`)}
												<CriteriaChip
													title="Click to remove search term {value}"
													on:remove={async () => {
														if ($criteria[key].type === 'Facet') {
															await toggleFacet(key, value);
														} else {
															await removeCriterion(key, value);
														}
													}}
													on:click={async () => {
														if ($criteria[key].type === 'Facet') {
															await toggleFacet(key, value);
														} else {
															await removeCriterion(key, value);
														}
													}}>{value}</CriteriaChip
												>
												{#if $criteria[key].operation && index !== $criteria[key].values.length - 1}
													<div class="text-xs">{$criteria[key].operation}</div>
												{/if}
											{/each}
										{:else}
											<CriteriaChip
												title="Click to show all options for criterion {key}"
												removable={false}
												on:click={async () => {
													facetsRef && facetsRef.showMore(key);
												}}>{$criteria[key].values.length} options</CriteriaChip
											>
										{/if}
										{#if index !== Object.keys($criteria).length - 1}
											<div class="border border-r h-full border-surface-900/10 mx-3"></div>
										{/if}
									</div>
								{/if}
							{/each}
						</div>
					</div>
				</div>
				<div class="pt-8">
					{#if config}
						<div class:hidden={currentView === 'cards'} class="">
							<Table {config} />
						</div>
					{/if}
					<div class:hidden={currentView === 'table'}>
						<Cards store={placeholderStore} />
					</div>
				</div>
			</div>
		</Pane>

	</PaneGroup>
</Page>