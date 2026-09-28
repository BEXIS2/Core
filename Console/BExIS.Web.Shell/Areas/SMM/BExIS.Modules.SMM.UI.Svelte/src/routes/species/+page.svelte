<script lang="ts">
	import { ErrorMessage, Page, pageContentLayoutType, positionType, Spinner } from "@bexis2/bexis2-core-ui";
	import type { SpeciesModel } from "./types";
	import { Table } from '@bexis2/bexis2-core-ui';
	import type { TableConfig } from '@bexis2/bexis2-core-ui';
	import { Modal, getModalStore } from '@skeletonlabs/skeleton';
	
	import { resultStore, acceptedStore } from './data';
	import type { ResultRow } from './data';
	import Dropzone from "svelte-file-dropzone"
	import { MultiSelect } from "@bexis2/bexis2-core-ui";
	import ResultTableOptions from "./ResultTableOptions.svelte";
	import AcceptedTableOptions from "./AcceptedTableOptions.svelte";
	import EditResult from "./EditResult.svelte";
	import { get } from "svelte/store";
	
	// let files: FileList;
	const ONE_MB: number = 1000000;
	const modalStore = getModalStore()

	let files: any[] = [];
	let fileData: string[][] = [];
	let headerData: string[] = [];


	function processRawCSV(data: string): string[][] {
		const output: string[][] = [];
		const rows = data.split("\n");
		if (!rows.length) return output;

		headerData = rows[0].split(",");

		for (let i = 0; i < rows.length; i++) {
			const cells = rows[i].split(",");
			output.push(cells);
		}

		return output;
	}

	function handleFilesSelect(e: CustomEvent<any>) {
		files = e.detail.acceptedFiles;
		for (let i = 0; i < files.length; i++) {
			const reader = new FileReader();
			reader.onload = () => {
				const binaryStr = reader.result;
				if (typeof binaryStr === "string") {
					fileData = processRawCSV(binaryStr);
				} else if (binaryStr == null) {
					console.error("reader result returned null, could not handle selected file");
				} else {
					console.error("reader result returned ArrayBuffer, could not handle selected file");
				}
			};

			// this seems to be the normal way..
			reader.readAsText(files[i]);
		}
	}


	function synthLotsOfData() {
		var synthRows: ResultRow[] = [];
		for (let i = 0; i < 10000; i++) {
			synthRows.push({
				inputID: i,
				status: "accepted",
			})
		}

		resultStore.update(items => [...items, ...synthRows]);
	}

	function synthTestData() {
		var synthRows: ResultRow[] = [];
		for (let i = 0; i < 10; i++) {
			synthRows.push({
				inputID: i,
				inputName: `inputName_${i}`,
				matchType: "exact",
				scientificName: `exactName_${i}`,
				id: Math.floor(Math.random() * (10 - 1) + 1)
			})
		}

		// for (let i = 100; i < 200; i++) {
		// 	synthRows.push({
		// 		inputID: i,
		// 		inputName: `inputName_${i}`,
		// 		matchType: "variant",
		// 		scientificName: `variantName_${i}`
		// 	})
		// }

		for (let i = 10; i < 20; i++) {
			synthRows.push({
				inputID: i,
				inputName: `inputName_${i}`,
				matchType: "none",
				scientificName: "ABC",
				id: Math.floor(Math.random() * (10 - 1) + 1)
			})
		}

		resultStore.update(items => [...items, ...synthRows]);
	}

	function logStore() {
		console.log(get(resultStore));
	}

	function acceptAllByMatchType(match_type: string) {
		let itemsAccepted: ResultRow[] = [];
		resultStore.update(currentItems => {
			const remaining: ResultRow[] = [];
			for (const item of currentItems) {
				if (item.matchType === match_type) {
					itemsAccepted.push(item);
				} else {
					remaining.push(item);
				}
			}

			return remaining;
		});

		if (itemsAccepted.length > 0) {
			acceptedStore.update(currentItems => [...currentItems, ...itemsAccepted]);
		}
	}

	const resultTableActions = (action: CustomEvent<{ row: ResultRow; type: string }>) => {
		const { type, row } = action.detail;
		switch (type) {
			case 'ACCEPT':
				resultStore.update(items => items.filter(i => i.inputID !== row.inputID));
				acceptedStore.update(items => [...items, row]);
				break;
			case 'READ':
				break;
			case 'UPDATE':
				modalStore.trigger({
					type: 'component',
					title: `Edit Result name ${row.inputName}`,
					component: {
						ref: EditResult,
						props: { row: row }
					}
				});
				break;

			default:
				break;
		}
	};

	const acceptedTableActions = (action: CustomEvent<{ row: ResultRow; type: string }>) => {
		const { type, row } = action.detail;
		switch (type) {
			case 'REMOVE':
				acceptedStore.update(items => items.filter(i => i.inputID !== row.inputID));
				resultStore.update(items => [...items, row]);
				break;

			default:
				break;
		}
	};
	
	const resultConfig: TableConfig<ResultRow> = {						
		id: 'resultRows',						
		data: resultStore,
		resizable: "columns",
		height: 700,
		fitToScreen: false,
		defaultPageSize: 50,
		pageSizes: [20, 50, 100],
		showColumnsMenu: true,					
		columns: {							
			inputID: {								
				header: 'inputID',						
			}						
		},
		optionsComponent: ResultTableOptions	
	};

	const acceptedConfig: TableConfig<ResultRow> = {						
		id: 'acceptedRows',						
		data: acceptedStore,
		resizable: "columns",
		height: 700,
		fitToScreen: false,
		defaultPageSize: 50,
		pageSizes: [20, 50, 100],
		showColumnsMenu: true,					
		columns: {							
			inputID: {								
				header: 'inputID',						
			}						
		},
		optionsComponent: AcceptedTableOptions
	};

</script>
<Page 
	title="Species" 
	note=""
	contentLayoutType={pageContentLayoutType.center}
>
	<Dropzone on:drop={handleFilesSelect} multiple={false} accept=".csv" maxSize={100*ONE_MB}/>
	{#each files as item}
		<h2>{item.name}</h2>
	{/each}
<!-- 
	<table border="1">
		<tr>
			{#each headerData as header, i}
				<td><b>{header}</b></td>
			{/each}
		</tr>
		{#each fileData as row}
			<tr>
				{#each row as item}
					<td>{item}</td>
				{/each}
			</tr>
		{/each}
	</table> -->

	<button class="btn variant-filled-success" on:click={synthLotsOfData}>Synthesize Huge</button>
	<button class="btn variant-filled-success" on:click={synthTestData}>Synthesize Testdata</button>
	<button class="btn variant-filled-success" on:click={() => acceptAllByMatchType('exact')}>Accept all Exact</button>
	<button class="btn variant-filled-success" on:click={() => acceptAllByMatchType('variant')}>Accept all Variant</button>
	<button class="btn variant-filled-success" on:click={() => acceptAllByMatchType('none')}>Accept all None</button>
	<button class="btn variant-filled-secondary" on:click={() => logStore()}>Log Store</button>
	<h2 class="h2">Result</h2>
	<div class="flex items-center justify-center">
		<Table config={resultConfig} on:action={resultTableActions}/>
		<Modal />
	</div>
	
	<div class="h-20"></div>
	
	<h2 class="h2">Accepted</h2>
	<div class="flex items-center justify-center">
		<Table config={acceptedConfig} on:action={acceptedTableActions}/>
	</div>
	<div class="h-80"></div>
</Page>