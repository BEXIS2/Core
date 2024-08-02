<script lang="ts">
	import { onMount, createEventDispatcher } from 'svelte';

	// ui components
	import { Spinner, DropdownKVP, helpStore } from '@bexis2/bexis2-core-ui';

	import MissingValues from './MissingValues.svelte';

	//services
	import { store, load } from './services';

	import Fa from 'svelte-fa';
	import { faSave, faChevronRight, faArrowRotateLeft,faArrowLeft } from '@fortawesome/free-solid-svg-icons';

	//types
	import type { DataStructureCreationModel, markerType } from './types';

	import { positionType } from '@bexis2/bexis2-core-ui';
	import Controls from './Controls.svelte';
	import Attributes from './structure/Attributes.svelte';
	import ConstraintsDescription from './structure/variable/ConstraintsDescription.svelte';
	import { goTo } from '../../services/BaseCaller';

	export let model: DataStructureCreationModel;
	$: model;
	export let init: boolean = true;

	let delimeter;
	let isDrag: boolean = false;
	let state: boolean[][] = [];
	let selection: markerType[] = [];
	$: selection;
	let cLength: number = 0;
	let rLength: number = 0;
	let selectionsupport: boolean = true;
	let generate: boolean = true;

	let selectedRowIndex: number = 0;
	let data = [];
	$:data;

	let errors: string[] = [];
	$: errors;

	const MARKER_TYPE = {
		VARIABLE: 'variable',
		DESCRIPTION: 'description',
		UNIT: 'unit',
		MISSING_VALUES: 'missing-values',
		DATA: 'data'
	};

	// currently only one requirement exit
	// variable need to be selected
	let isValid: boolean = false;

	const dispatch = createEventDispatcher();

	onMount(async () => {
		console.log('start selection suggestion');
		console.log('load selection', model.entityId, model.file);
		setTableInfos(model.preview, String.fromCharCode(model.delimeter));
		setMarkers(model.markers, init);

		delimeter = model.delimeter;
	 prepareData(model.preview)
		checkStatus();

	});


	function prepareData(rows:string[])
	{
		data = [];
		if(rows)
		{
			rows.forEach(r=>{
					const cv = textMarkerHandling(r);
					data = [...data,cv]
			})

			console.log("🚀 ~ onMount ~ model.preview:", model.preview)
			console.log("🚀 ~ onMount ~ data:", data)
		}
	}

	function setTableInfos(rows, delimeter) {
		console.log('set table infos');
		//number of columns
		cLength = textMarkerHandling(rows[0]).length; // 1,2,"3,4",5

		console.log("🚀 ~ setTableInfos ~ cLength:", cLength)
		//number of rows
		rLength = rows.length;

		state = new Array(rLength).fill(false);

		for (var i = 0; i < state.length; i++) {
			state[i] = new Array<boolean>(cLength).fill(false);
		}

		console.log('state', state);
	}

	function setMarkers(markers, init = false) {
		for (var i = 0; i < markers.length; i++) {
			let marker = markers[i];
			console.log('marker', marker);
			if (init) {
				// if data come from server index need to set -1
				updateSelection(marker.type, marker.row - 1, marker.cells);
			} else {
				updateSelection(marker.type, marker.row, marker.cells);
			}
		}
	}

	function beginDrag(e) {
		isDrag = true;
	}

	function endDrag(e) {
		isDrag = false;
	}

	const mouseDownHandler = (r, c) => (e) => {
		console.log(e.type);

		if (e.which === 3 || e.button === 2) {
			console.log('Right mouse button at ' + e.clientX + 'x' + e.clientY);
		}

		if (r != selectedRowIndex) {
			clean();
		}

		if (isDrag || e.type === 'mousedown') {
			selectedRowIndex = r;

			//left mouse click
			if (e.which === 1 || e.button === 0) {
				selectCell(c);
			}

			// right mouse click
			if (e.which === 3 || e.button === 2) {
				deselectCell(c);
			}
		}
	};

	const mouseHandler = (r, c) => (e) => {
		if (isDrag || e.type === 'mousedown') {
			//left mouse click
			if (e.which === 1 || e.button === 0) {
				selectCell(c);
			}

			// right mouse click
			if (e.which === 3 || e.button === 2) {
				deselectCell(c);
			}
		}
	};

	const dbclickHandler = (c) => (e) => {
		console.log('dblclick', e.type, isDrag);
		if (isDrag || e.type === 'dblclick') {
			selectRow(selectedRowIndex);
		}
	};

	const selectCell = (c) => {
		state[selectedRowIndex][c] = true;
	};

	const deselectCell = (c) => {
		state[selectedRowIndex][c] = false;

		console.log('deselect');
		// if a selection is active , also delect it
		if (selection.length > 0) {
			console.log('selection l:', selection.length);

			if (selectionsupport) {
				// update alle cells in all stored selections
				for (let index = 0; index < selection.length; index++) {
					const selectObj = selection[index];
					if (selectObj) {
						selectObj.cells[c] = false;
						updateSelection(selectObj.type, selectObj.row, selectObj.cells);
					}
				}
			} else {
				var selectObj = selection.find((e) => e.row == selectedRowIndex);
				if (selectObj) {
					selectObj.cells[c] = false;
					updateSelection(selectObj.type, selectObj.row, selectObj.cells);
				}
			}
		}

		// clean all deactive selections
		cleanSelection();
		//check if selection is valid for save
		checkStatus();
	};

	const selectRow = (r) => {
		console.log('set true');
		for (var i = 0; i < cLength; i++) {
			console.log('select row', cLength);
			state[r][i] = true;
		}
	};

	function clean() {
		state = new Array(rLength).fill(false);

		for (var i = 0; i < state.length; i++) {
			state[i] = new Array(cLength).fill(false);
		}
	}

	// set selection empty or a new set up
	function resetSelection() {
		selection = [];
		isValid = false;
	}

	// remove every selection that has only false values in the array
	function cleanSelection() {
		selection = selection.filter((s) => s.cells.find((c) => c === true));
	}

	function getMarkerLayout(r) {
		var element = selection.find((e) => e.row === r);

		if (element) {
			console.log(element.type);
			return element.type;
		}

		return 'variable';
	}

	function onclickHandler(type) {
		// get selected cells
		let selectedCells = state[selectedRowIndex];

		// if selections upport is true and one entry exist, means that the cells selection is the same
		// like the stored one
		if (selectionsupport && selection.length > 0) {
			selectedCells = selection[0].cells;
		}

		updateSelection(type, selectedRowIndex, selectedCells);

		//check if selection is valid for save
		checkStatus();
	}

	function updateSelection(type, index, cells) {
		let obj = {
			type: type,
			row: index,
			cells: cells
		};

		// if exist row, remove entry
		let exist = selection.find((e) => e.row == obj.row);
		if (exist) {
			selection = selection.filter((e) => e.row !== obj.row);
		}

		// if exist, remove entry
		exist = selection.find((e) => e.type == obj.type);
		if (exist) {
			selection = selection.filter((e) => e.type !== obj.type);
		}

		// add obj to list and return new list
		selection = [...selection, obj];
	}

	// different thinks need to be done before save button is active
	function checkStatus() {
		errors = [];
		// minimum marker for variable and data need to exist
		let variabelMarker = selection.find((s) => s.type == MARKER_TYPE.VARIABLE);
		let dataMarker = selection.find((s) => s.type == MARKER_TYPE.DATA);

		let selectionCount = selection.length;

		if (selectionCount > 0) {
			// only check if selection exist
			if (!variabelMarker) {
				errors.push('the variables still need to be marked');
			}

			if (!dataMarker) {
				errors.push('the data still need to be marked');
			}

			let lastCount = 0;
			for (let index = 0; index < selection.length; index++) {
				const element = selection[index];
				console.log(index, element);
				let c = element.cells.filter((c) => c == true)?.length; // get length of all cells marked as true
				if (index > 0) {
					// after first run, check count against the others
					if (c != lastCount) {
						let message =
							'selection mismatch, the rows must have the same number of marked cells  ';
						errors.push(message);
						break;
					}
				}

				lastCount = c; // last count set
			}
			isValid = errors.length == 0 ? true : false;
		} // no selection
		else {
			isValid = false; // no selection, no errors,  not valid
			errors = [];
		}
	}

	async function save() {
		generate = true;

		model.markers = selection;

		// if model entityid == 0, means subject id is not set and store to cache is not needed
		if (model.entityId > 0) {
			console.log('save selection', model);
			let res = await store(model);
			console.log(res);
			if (res != false) {
				console.log('selection', res);
				dispatch('saved', model);
				generate = false;
			}
		} else {
			dispatch('saved', model);
			generate = false;
		}
	}

	// if you change the delimeter you need to change/update also the table informations
	function changeDelimeter() {
		setTableInfos(model.preview, String.fromCharCode(model.delimeter));
		prepareData(model.preview)
	}

	// ROW Selection
	const rowSelectionHandler = (r) => (e) => {
		console.log(r, e.which, e.button);
		//left mouse click
		if (e.which === 1 || e.button === 0) {
			clean();
			console.log(r);
			selectRow(r);
			selectedRowIndex = r;
		}

		// right mouse click
		if (e.which === 3 || e.button === 2) {
			clean();
		}
	};

	async function onChangeEncodingHandler(e) {
		const encoding = e;
		console.log('🚀 ~ e.detail:', e);
		const m = await load(model.file, model.entityId, encoding, 0);
		model.preview = m.preview;
	}

 
	function textMarkerHandling(row:string):[]
	{
		 const d = String.fromCharCode(model.delimeter);
		 const t = String.fromCharCode(model.textMarker);
			const values = row.split(d);

			let temp=[]; 

			if(row.includes(t))
			{
				 let tempValue:string = "";
					let startText:boolean = false;

					values.forEach(v => {

							if(v.includes(t))
							{
										if(v.startsWith(t) && v.endsWith(t))
										{
												temp = [...temp,v]
										}
										else
										{
													if (v.startsWith(t))
													{
																	tempValue = v;
																	startText = true;
													}

													if (v.endsWith(t))
													{
																	tempValue += d + v;
																	temp = [...temp,tempValue];
																	startText = false;
													}
										}
							}
							else
							{
								if (startText){
									tempValue += d + v;
								}
								else{
									temp = [...temp, v];
								}
							}
						
							
					});
					
					return temp;
			}
			else
			{
				return values;
			}

	}

	function back() {
		goTo("/rpm/datastructure/create");
	}
</script>


{#if !model || state.length == 0 || generate == false}
<button title="back" class="btn variant-filled-warning" on:click={() => back()}
	><Fa icon={faArrowLeft} /></button>
	<!--if the model == false, access denied-->
	{#if !model || state.length == 0 || generate == false}
		<div class="h-full w-full text-surface-700">
			<Spinner
				position={positionType.center}
				label="Loading Structure Suggestion based on: {model.file}"
			/>
		</div>
	{:else}
		<div class="h-full w-full text-surface-700">
			<Spinner position={positionType.center} label="Generate Structure..." />
		</div>
	{/if}
{:else}
	<!-- load page -->
	<form on:submit|preventDefault={save}>
		<div
			id="structure-suggestion-container"
			class="flex-col gap-3"
			on:mousedown={beginDrag}
			on:mouseup={endDrag}
		>
			<div class="flex gap-5">
				<div id="edit" class="flex flex-col grow gap-2">
					<div id="reader selections" class="flex flex-none gap-2">
						<DropdownKVP
							id="delimeter"
							title="Delimeter"
							bind:target={model.delimeter}
							source={model.delimeters}
							complexTarget={false}
							on:change={changeDelimeter}
							help={true}
						/>

						<DropdownKVP
							id="decimal"
							title="Decimal"
							bind:target={model.decimal}
							source={model.decimals}
							complexTarget={false}
							help={true}
						/>

						<DropdownKVP
							id="textMarker"
							title="TextMarker"
							bind:target={model.textMarker}
							source={model.textMarkers}
							complexTarget={false}
							help={true}
						/>

						<DropdownKVP
							id="encoding"
							title="Encoding"
							bind:target={model.fileEncoding}
							source={model.encodings}
							complexTarget={false}
							help={true}
							on:change={onChangeEncodingHandler(model.fileEncoding)}
						/>
					</div>

					<div id="markers" class="py-5 flex gap-1">
						<button
							class="btn variant-filled-error"
							id="selectVar"
							type="button"
							on:click={() => onclickHandler(MARKER_TYPE.VARIABLE)}
							on:mouseover={() => helpStore.show('selectVar')}>Variable</button
						>
						<button
							class="btn variant-filled-success"
							type="button"
							id="selectUnit"
							on:mouseover={() => helpStore.show('selectUnit')}
							on:click={() => onclickHandler(MARKER_TYPE.UNIT)}>Unit</button
						>
						<button
							class="btn variant-filled-warning"
							type="button"
							id="selectDescription"
							on:mouseover={() => helpStore.show('selectDescription')}
							on:click={() => onclickHandler(MARKER_TYPE.DESCRIPTION)}>Description</button
						>
						<button
							class="btn variant-filled-secondary"
							type="button"
							color="info"
							id="selectMissingValues"
							on:mouseover={() => helpStore.show('selectMissingValues')}
							on:click={() => onclickHandler(MARKER_TYPE.MISSING_VALUES)}>Missing Values</button
						>
						<button
							class="btn variant-filled-primary"
							type="button"
							id="selectData"
							on:mouseover={() => helpStore.show('selectData')}
							on:click={() => onclickHandler(MARKER_TYPE.DATA)}>Data</button
						>

						<button
							title="reset selection"
							id="resetSelection"
							class="btn variant-filled-warning text-lg"
							type="button"
							on:mouseover={() => helpStore.show('resetSelection')}
							on:click={resetSelection}><Fa icon={faArrowRotateLeft} /></button
						>
					</div>

					<div id="missingvalues" class="grow">
						<!-- Missing Values-->
						<MissingValues bind:list={model.missingValues} />
					</div>

					<div class="flex">
						<div id="errors" class="m-2 text-sm grow text-right">
							{#each errors as error}
								<label class="text-error-500">{error}</label>
							{/each}
						</div>
						<div class="text-right">
							<button title="save" class="btn variant-filled-primary text-lg" disabled={!isValid}>
								<Fa icon={faSave} />
							</button>
						</div>
					</div>
				</div>

				<div class="controls"><Controls /></div>
			</div>

			<div id="preview data" class="flex-col py-5">
				<div id="data infos" class="flex flex-auto gap-5 pb-2">
					<label><b>Total:</b> {model.total}</label>
					<label><b>Found:</b> {model.total - model.skipped}</label>
					<label><b>Skipped:</b> {model.skipped}</label>
					<label class="grow text-right"><i>you see only the first 10 rows of the data</i> </label>
				</div>

				<div class="overflow-x-auto">
					<table class="table table-compact" on:contextmenu={(e) => e.preventDefault()}>
						<tbody>
							{#each data as row, r}
								<tr>
									<td
										class="w-8 hover:cursor-pointer select-none text-sm hover:border-surface-400 hover:border-solid hover:border-b-2"
										on:mousedown={rowSelectionHandler(r)}
									>
										<div class="pt-1">
											<Fa icon={faChevronRight} size="sm" />
										</div>
									</td>

									{#each row as cell, c}
										<td
											class="hover:cursor-pointer select-none hover:border-surface-400 hover:border-solid hover:border-b-2"
											on:dblclick={dbclickHandler(r)}
											on:mousedown={mouseDownHandler(r, c)}
											on:mouseenter={mouseHandler(r, c)}
											class:variant-soft-error={selection.find(
												(e) => e.row === r && e.cells[c] === true
											)?.type === MARKER_TYPE.VARIABLE}
											class:variant-soft-success={selection.find(
												(e) => e.row === r && e.cells[c] === true
											)?.type === MARKER_TYPE.UNIT}
											class:variant-soft-warning={selection.find(
												(e) => e.row === r && e.cells[c] === true
											)?.type === MARKER_TYPE.DESCRIPTION}
											class:variant-soft-secondary={selection.find(
												(e) => e.row === r && e.cells[c] === true
											)?.type === MARKER_TYPE.MISSING_VALUES}
											class:variant-soft-primary={selection.find(
												(e) => e.row === r && e.cells[c] === true
											)?.type === MARKER_TYPE.DATA}
											class:variant-ghost-surface={state[r][c]}
										>
											{(cell = cell.replaceAll(String.fromCharCode(model.textMarker), ''))}
										</td>
									{/each}
								</tr>
							{/each}
						</tbody>
					</table>
				</div>
			</div>
		</div>
	</form>
{/if}

<style>
	.variable {
		background-color: blue;
		color: white;
	}

	.unit {
		background-color: green;
		color: white;
	}

	.description {
		background-color: var(--bs-warning);
	}

	.missing-values {
		background-color: var(--bs-info);
	}

	.data {
		background-color: var(--bs-primary);
		color: white;
	}
</style>
