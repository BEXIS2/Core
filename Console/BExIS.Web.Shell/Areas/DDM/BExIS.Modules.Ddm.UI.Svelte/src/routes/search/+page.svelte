<script lang="ts">
	import Select from 'svelte-select';
	import { onMount } from 'svelte';
	import { writable } from 'svelte/store';
	import { RadioGroup, RadioItem } from '@skeletonlabs/skeleton';
	import { Api, Facets, Page, pageContentLayoutType, Table } from '@bexis2/bexis2-core-ui';
	import type { Columns, FacetGroup, TableConfig } from '@bexis2/bexis2-core-ui';

	import ShowData from '$lib/components/ShowData.svelte';
	import Cards from '$lib/components/Cards.svelte';
	import CriteriaChip from '$lib/components/CriteriaChip.svelte';
	import { convertTableData } from '$lib/helpers';

	let columns: Columns;
	let config: TableConfig<any>;
	let categories: { name: string; displayName: string }[] = [];
	let currentCategory = 'All';
	let q = '';

	let currentView: 'table' | 'cards' = 'table';

	let facetsRef: any;

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
			categories.find((c) => c.name === currentCategory)?.displayName || currentCategory;

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
		init &&
			responseObject.SearchComponent.Categories.forEach((category: any) => {
				categories = [
					...categories,
					{
						name: category.Name,
						displayName: category.DisplayName
					}
				];
			});

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
			console.log("🚀 ~ columns:", columns)


			config = {
				id: 'search-table',
				data: tableStore,
				search: false,
				optionsComponent: ShowData as any,
				columns:
					{
						iD: {
							header: 'Id',
							instructions: {
								toSortableValueFn: (id: string) => parseInt(id)
							}
					}
				}
		}
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
				index:headers.indexOf(header)
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

	const handleAutoCompleteSelect = async (e: { detail: { value: string; label: string } }) => {
		q = e.detail.value;

		return null;
	};

	onMount(async () => {
		await handleSearch(true);
	});

	let links: any[] = [
		{
			label: 'public search',
			url: '/ddm/publicsearch'
		}]
</script>

<Page
	title="Search"
	note="Search over the data in this system."
	contentLayoutType={pageContentLayoutType.full}
	{links}
>
	<div class="flex gap-8 overflow-auto">
		<div class="min-w-64">
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
		<div class="flex flex-col gap-4">
			<div class="flex flex-col gap-4">
				<div class="flex gap-4">
					<div class="w-min flex" title="Switch between table and card view">
						<RadioGroup active="variant-filled-primary" hover="hover:variant-soft-primary">
							<RadioItem
								bind:group={currentView}
								name="currentView"
								value="table"
								title="Table view">Table</RadioItem
							>
							<RadioItem bind:group={currentView} name="currentView" value="cards" title="Card view"
								>Cards</RadioItem
							>
						</RadioGroup>
					</div>

					<select class="input w-max" bind:value={currentCategory} title="Filter categories">
						{#each categories as category (category.name)}
							<option value={category.name}>{category.displayName}</option>
						{/each}
					</select>
					<Select
						loadOptions={setAutoCompleteValues}
						class="input grow max-w-[500px] min-w-[500px]"
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

				<!-- Criteria and applied search queries -->
				<div class="flex grow w-full">
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

			<div class="pt-8 grow">
				{#if config}
					<div class:hidden={currentView === 'cards'}>
						<Table {config} />
					</div>
				{/if}
				<div class:hidden={currentView === 'table'}>
					<Cards store={placeholderStore} />
				</div>
			</div>
		</div>
	</div>
</Page>
