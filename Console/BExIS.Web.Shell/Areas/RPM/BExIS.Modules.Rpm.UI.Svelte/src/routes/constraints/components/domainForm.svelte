<script lang="ts">
	import { CodeEditor, Table, helpStore, MultiSelect } from '@bexis2/bexis2-core-ui';
	import { slide } from 'svelte/transition';
	import type { DatasetInfo, DomainConstraintListItem } from '../models';
	import { writable } from 'svelte/store';
	import * as apiCalls from '../services/apiCalls';
	import { createEventDispatcher, onMount } from 'svelte';
	import suite from './domainForm';
	import { Drawer, FileButton, getDrawerStore, type DrawerSettings } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import { faArrowUpFromBracket, faFileImport } from '@fortawesome/free-solid-svg-icons';
	import papa from 'papaparse';


	export let domainConstraint: DomainConstraintListItem;

	const dispatch = createEventDispatcher();
	let datasets: DatasetInfo[] = [];
	let dataset: DatasetInfo;
	let ds: DatasetInfo[] = [];
	$: datasets = ds;
	
	const drawerStore = getDrawerStore();
	const drawerSettings: DrawerSettings = {position: 'right'}; 
	
	// load form result object
	let res = suite.get();

	// use to actived save if form is valid
	$: disabled = !res.isValid();
	$: dispatch(String(disabled));

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

	const domainItemsTableStore = writable<string[]>([]);
	$: domainItemsTableStore.set(setDomainItems(domainConstraint.domain));

	function setDomainItems(domain: string): string[] {
		let dis: string[] = [];
		let lines = domain.split('\n');
		lines.forEach(function (value) {
			if(value != undefined && value != '' && dis.filter((di) => di === value).length == 0)	
			{
				dis.push(value);
			}
		});
		return dis;
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
		<div class="mt-0 py-1 text-right mt-7" title="Import">
			<!-- svelte-ignore a11y-mouse-events-have-key-events -->
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
				{#if datasets.length > 0}
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
						{disabled}
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
				<table class="table w-full table-compact bg-tertiary-200 h-80" on:mouseover={() => {
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
{#if datasets.length > 0}
<Drawer>
	<MultiSelect
	title="Datasets"
	id="datasets"
	bind:target={dataset}
	source={datasets}
	itemId="id"
	itemLabel="name"
	complexSource={true}
	complexTarget={false}
	required={false}
	help={true}
/>
</Drawer>
{/if}
