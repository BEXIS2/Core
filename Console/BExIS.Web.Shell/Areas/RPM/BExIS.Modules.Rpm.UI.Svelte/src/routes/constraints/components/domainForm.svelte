<script lang="ts">
	import { CodeEditor, Table, helpStore, MultiSelect } from '@bexis2/bexis2-core-ui';
	import { slide } from 'svelte/transition';
	import type { DatasetInfo, DomainConstraintListItem } from '../models';
	import { writable } from 'svelte/store';
	import * as apiCalls from '../services/apiCalls';
	import { createEventDispatcher, onMount } from 'svelte';
	import suite from './domainForm';
	import { Drawer, FileButton, getDrawerStore, type DrawerSettings, type DrawerStore } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import { faArrowUpFromBracket, faFileImport } from '@fortawesome/free-solid-svg-icons';
	import papa from 'papaparse';
	import DatasetImport from './datasetImport.svelte';
	import { variableTemplateHelp } from '../../variabletemplate/help';

	export let domainConstraint: DomainConstraintListItem;

	const dispatch = createEventDispatcher();

	let drawerStore: any;
	let ds: DatasetInfo[] = [];
	let drawerSettings: DrawerSettings;

	const domainItemsTableStore = writable<string[]>([]);
	
	drawerStore = $drawerStore ? $drawerStore : getDrawerStore();
	$drawerStore.meta = $drawerStore.meta ? $drawerStore.meta : { datasets: undefined, dataset: undefined, import: false};
	
	$: $drawerStore.meta.datasets = ds;
	$: drawerSettings = {position: 'right', width: 'w-6/12', meta: $drawerStore.meta};
	$: domainConstraint, $drawerStore.meta.dataset = undefined, $drawerStore.meta.import = false;
	$: $drawerStore.meta.import, importDomainItems();
	$: domainItemsTableStore.set(setDomainItems(domainConstraint.domain));
	$: dispatch(String(disabled));

	// load form result object
	let res = suite.get();

	// use to actived save if form is valid
	$: disabled = !res.isValid();

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: any) {
		//console.log("input changed", e)
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			// check changed field
			res = suite(domainConstraint , e);
		}, 10);
	}

	onMount(async () => {
		ds = await apiCalls.GetStruturedDatasetsByUserPermission();
		if (domainConstraint.id == 0) {
			suite.reset();
		}
		else{
			setTimeout(async () => {	
				res = suite(domainConstraint, "");
			}, 10);
		}
	});

	function setDomainItems(domain: string): string[] {
		let dis: string[] = [];
		let lines = domain.split('\n');
		lines.forEach(function (line) {
			if(line != undefined && line != '' && dis.filter((di) => di === line).length == 0)	
			{
				dis.push(line);
			}
		});
		disabled = false;
		return dis;
	}

	async function importDomainItems()
	{
		if($drawerStore.meta.import && $drawerStore.meta.dataset && $drawerStore.meta.dataset.id > 0)
		{
			$drawerStore.meta.import = false;
			let data: string[] = [];
			let column = await apiCalls.GetData($drawerStore.meta.dataset.id, 0, $drawerStore.meta.dataset.varId);
			let provider: string[] = await apiCalls.GetProvider();
			
			domainConstraint.provider = provider[0];
			domainConstraint.selectionPredicate = { datasetId: $drawerStore.meta.dataset.id, datasetVersionId:  $drawerStore.meta.dataset.datasetVersionId, datasetVersionNumber: $drawerStore.meta.dataset.datasetVersionNumber, tagId: 0, variableId: $drawerStore.meta.dataset.varId, url:'' };

			for(const [rowNr, row] of Object.entries(column)) 
			{
				let value: string[][] = (Object.entries(row as object) as string[][])
				data.push(value[0][1]);
			};
			domainConstraint.domain = joinRows(data);
		} 
	}

	function fileParser(event: any) {
		if (event.target != null) {
			const fs = event.target.files;
			for (let f of fs) {
				papa.parse(f, {
					skipEmptyLines: true,
					header: false,
					complete: function (r) {
						domainConstraint.domain = joinRows(r.data);
					}
				});
			}
		}
	}

	function joinRows(data: any): string {
		return data.join('\n').trim().replaceAll('\t', '');
	}

</script>

{#if domainConstraint}
	<div class="grid grid-cols-2 gap-x-5 h-96" in:slide out:slide>
		<div class="py-1 text-right" title="Upload File">
			<!-- svelte-ignore a11y-mouse-events-have-key-events -->
			<!-- svelte-ignore a11y-no-static-element-interactions -->
			<div class="inline-block" on:mouseover={() => {
				helpStore.show('uploadCsv');
			}}>
				<!-- svelte-ignore a11y-mouse-events-have-key-events -->
				<FileButton
					id="uploadCsv"
					title="Upload CSV"
					button="btn variant-filled-secondary h-9 w-16 shadow-md"
					name="uploadCsv"
					on:change={fileParser}
					><Fa
						icon={faArrowUpFromBracket}
					/>
				</FileButton>				
			</div>
			<div class="inline-block">
				<!-- svelte-ignore a11y-mouse-events-have-key-events -->
				{#if ds.length > 0}
					<button
						id="importDs"
						title="Import from Dataset"
						type="button"
						class="btn variant-filled-secondary h-9 w-16 shadow-md"
						name="importDs"
						on:click={() => drawerStore.open(drawerSettings)}
						on:mouseover={() => {
							helpStore.show('importDs');
						}}
						><Fa
							icon={faFileImport}
						/>
					</button>
				{:else}
					<button
						id="importDs"
						title="Import from Dataset"
						type="button"
						class="btn variant-filled-secondary h-9 w-16 shadow-md"
						name="importDs"
						disabled
						on:mouseover={() => {
							helpStore.show('importDs');
						}}
						><Fa
							icon={faFileImport}
						/>
					</button>
				{/if}
			</div>
		</div>
		<div class="pb-3 row-span-2">
			<div id="itemstable" class="table-container h-96">
				<!-- svelte-ignore a11y-mouse-events-have-key-events -->
				<table class="table w-full table-compact bg-tertiary-500/30 max-h-80" on:mouseover={() => {
					helpStore.show('domainList');
				}}>
					<thead>
						<tr class="bg-primary-300">
							<th class="!p-2">Domain List</th>
						</tr>
					</thead>
					<tbody>
						{#each $domainItemsTableStore as domainItem}
							<tr>
								<td>{domainItem}</td>
							</tr>
						{/each}
					</tbody>
				</table>
			</div>
		</div>
		<!-- svelte-ignore a11y-mouse-events-have-key-events -->
		<!-- svelte-ignore a11y-no-static-element-interactions -->
		<div class="pb-3"
		on:mouseover={() => {
			helpStore.show('domain');
		}}>
			<CodeEditor
				id="domain"
				initialValue={domainConstraint.domain}
				toggle={false}
				bind:value={domainConstraint.domain}
				actions={false}
				styles={{
					'&': {
						borderRadius: '0.5rem',
						width: '100%',
						height: '23.25rem'
					},
					'.cm-scroller': {
						borderRadius: '0.5rem'
					}
				}}
			/>
		</div>		
	</div>
{/if}
{#if ds.length > 0}
	<Drawer>
		<DatasetImport {drawerStore}/>
	</Drawer>
{/if}
