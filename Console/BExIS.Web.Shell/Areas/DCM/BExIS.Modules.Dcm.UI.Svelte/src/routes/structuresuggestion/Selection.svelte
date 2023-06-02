<script lang="ts">
	import { onMount, createEventDispatcher } from 'svelte';

	// ui components
	import { Spinner, DropdownKVP } from '@bexis2/bexis2-core-ui';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import MissingValues from '../structuresuggestion/MissingValues.svelte';

	//services
	import { store, load } from './services.js';

	//types
	import type { StructureSuggestionModel, Marker } from '../../models/StructureSuggestion';

  export let model: StructureSuggestionModel;

	let delimeter;

	$: model;

	let isDrag: boolean = false;
	let state: boolean[][] = [];
	let sel: Marker[] = [];
	$: selection = sel;
	let cLength: number = 0;
	let rLength: number = 0;
	let selectionsupport: boolean = false;

	let selectedRowIndex: number = 0;

	// currently only one requirement exit
	// variable need to be selected
	let isValid: boolean = false;

	const MARKER_TYPE = {
		VARIABLE: 'variable',
		DESCRIPTION: 'description',
		UNIT: 'unit',
		MISSING_VALUES: 'missing-values',
		DATA: 'data'
	};

	const dispatch = createEventDispatcher();

	onMount(async () => {
		console.log('start selection suggestion');
		init();
	});

	async function init() {
		console.log('load selection', model.id, model.file);
		setTableInfos(model.preview, String.fromCharCode(model.delimeter));
		setMarkers(model.markers);

		delimeter = model.delimeter;

		setTimeout(function () {
			console.log('model after 100ms', model);
			console.log('delimeter after 100ms', model.delimeter);
			console.log('decimal after 100ms', model.decimal);

			console.log('delimeter only', delimeter);
		}, 100);

		console.log('init', model);
		console.log('delimeter', model.delimeter);
		console.log('decimal', model.decimal);
		console.log('delimeter only', delimeter);
	}

	function setTableInfos(rows, delimeter) {
		console.log('set table infos');
		//number of columns
		cLength = rows[0].split(delimeter).length;
		//number of rows
		rLength = rows.length;

		state = new Array(rLength).fill(false);

		for (var i = 0; i < state.length; i++) {
			state[i] = new Array<boolean>(cLength).fill(false);
		}

		console.log('state', state);
	}

	function setMarkers(markers) {
		for (var i = 0; i < markers.length; i++) {
			let marker = markers[i];
			console.log('marker', marker);
			updateSelection(marker.type, marker.row - 1, marker.cells);

			// check if varaible is set, then activet store
			if (marker.type == MARKER_TYPE.VARIABLE) {
				isValid = true;
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
	};

	const selectRow = (r) => {
		console.log('set true');
		for (var i = 0; i < cLength; i++) {
			console.log(i);
			state[r][i] = true;
		}
	};

	function clean() {
		state = new Array(rLength).fill(false);

		for (var i = 0; i < state.length; i++) {
			state[i] = new Array(cLength).fill(false);
		}
	}

	function cleanSelection() {
		selection = [];
		isValid = false;
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

		if (type == MARKER_TYPE.VARIABLE) {
			isValid = true;
		}

		// if selectionsupport is true and one entry exist, means that the cells selection is the same
		// like the stored one
		if (selectionsupport && selection.length > 0) {
			selectedCells = selection[0].cells;
		}

		updateSelection(type, selectedRowIndex, selectedCells);
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

    console.log("selection", selection)
	}

	async function save() {
		model.markers = selection;

		console.log('save selection', model);
		let res = await store(model);
		console.log(res);

		if (res != false) {
			console.log('selection', res);
			dispatch('saved', model);
		}
	}
</script>

{#if !model || state.length == 0}
	<!--if the model == false, access denied-->
	<Spinner />
{:else}
	<!-- load page -->
	<form on:submit|preventDefault={save}>
		<div
			id="structure-suggestion-container"
			class="grid grid-cols-3 gap-5"
			on:mousedown={beginDrag}
			on:mouseup={endDrag}
		>
			<div>
				<DropdownKVP
					id="Delimeter"
					title="Delimeter"
					bind:target={model.delimeter}
					source={model.delimeters}
					targetIsComplex={false}
				/>
			</div>
			<div>
				<DropdownKVP
					id="Decimal"
					title="Decimal"
					bind:target={model.decimal}
					source={model.decimals}
					targetIsComplex={false}
				/>
			</div>
			<div>
				<DropdownKVP
					id="TextMarker"
					title="TextMarker"
					bind:target={model.textMarker}
					source={model.textMarkers}
					targetIsComplex={false}
				/>
			</div>

			<div class="col-span-2 space-y-5">
				<div>
					<button
						class="btn variant-filled-error"
						type="button"
						on:click={() => onclickHandler(MARKER_TYPE.VARIABLE)}>Variable</button
					>
					<button
						class="btn variant-filled-success"
						type="button"
						on:click={() => onclickHandler(MARKER_TYPE.UNIT)}>Unit</button
					>
					<button
						class="btn variant-filled-warning"
						type="button"
						on:click={() => onclickHandler(MARKER_TYPE.DESCRIPTION)}>Description</button
					>
					<button
						class="btn variant-filled-secondary"
						type="button"
						color="info"
						on:click={() => onclickHandler(MARKER_TYPE.MISSING_VALUES)}>Missing Values</button
					>
					<button
						class="btn variant-filled-primary"
						type="button"
						on:click={() => onclickHandler(MARKER_TYPE.DATA)}>Data</button
					>
					<button class="btn variant-ghost-surface" type="button" on:click={cleanSelection}>delete</button>
					<button class="btn variant-ghost-surface" disabled={!isValid}>edit</button>
					<!--<Fa icon={faTrashAlt}/> <Fa icon={faPenToSquare}/> -->
				</div>
				<div>
					<SlideToggle name="selection support" bind:checked={selectionsupport}
						>selection support</SlideToggle
					>
				</div>
				<div>
					<!-- Missing Values-->
					<MissingValues bind:list={model.missingValues} />
				</div>
				<div class="flex flex-auto gap-5">
					<label><b>Total:</b> {model.total}</label>
					<label><b>Found:</b> {model.total - model.skipped}</label>
					<label><b>Skipped:</b> {model.skipped}</label>
				</div>
			</div>

			<!-- controls-->
			<div class="col-span-1 card p-5 w-auto">
				<h4 class="h4">Controls</h4>
				<hr class="divide-x-8" />
				<dl class="list-dl gap-0">
					<div>
						<span class="badge bg-primary-500" />
						<span class="flex-auto">
							<dt class="font-bold">Selection</dt>
							<dd>left mouse button</dd>
						</span>
					</div>
					<div>
						<span class="badge bg-primary-500" />
						<span class="flex-auto">
							<dt class="font-bold">Drag</dt>
							<dd>left mouse button down and drag</dd>
						</span>
					</div>

					<div>
						<span class="badge bg-primary-500" />
						<span class="flex-auto">
							<dt class="font-bold">Select Row</dt>
							<dd>double click left mouse button</dd>
						</span>
					</div>
					<div>
						<span class="badge bg-primary-500" />
						<span class="flex-auto">
							<dt class="font-bold">Deselect</dt>
							<dd>right mouse button click</dd>
						</span>
					</div>
					<!-- ... -->
				</dl>
			</div>

			<div class="col-span-3" />
			<div>
				<table class="table table-compact">
					<tbody>
						{#each model.preview as row, r}
							<tr>
								{#each row.split(String.fromCharCode(model.delimeter)) as cell, c}
									<td 
                    class="hover:cursor-pointer select-none"

										on:dblclick={dbclickHandler(r)}
										on:mousedown={mouseDownHandler(r, c)}
										on:mouseenter={mouseHandler(r, c)}
                    
										class:variant-soft-error={selection.find((e) => e.row === r && e.cells[c] === true)
											?.type === MARKER_TYPE.VARIABLE}
										class:variant-soft-success={selection.find((e) => e.row === r && e.cells[c] === true)?.type ===
											MARKER_TYPE.UNIT}
										class:variant-soft-warning={selection.find((e) => e.row === r && e.cells[c] === true)
											?.type === MARKER_TYPE.DESCRIPTION}
										class:variant-soft-secondary={selection.find((e) => e.row === r && e.cells[c] === true)
											?.type === MARKER_TYPE.MISSING_VALUES}
										class:variant-soft-primary={selection.find((e) => e.row === r && e.cells[c] === true)?.type ===
											MARKER_TYPE.DATA}
										class:variant-ghost-surface={state[r][c]}
									>
										{cell}
									</td>
								{/each}
							</tr>
						{/each}
					</tbody>
				</table>
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

<!-- 
<style>
	.table-container {
		width: 100%;
		overflow-x: scroll;
	}

	.content {
		width: 100%;
	}

	.flipped,
	.flipped .content {
		transform: rotateX(180deg);
		-ms-transform: rotateX(180deg); /* IE 9 */
		-webkit-transform: rotateX(180deg); /* Safari and Chrome */
	}

	table,
	tr,
	td {
		-webkit-touch-callout: none; /* iOS Safari */
		-webkit-user-select: none; /* Safari */
		-khtml-user-select: none; /* Konqueror HTML */
		-moz-user-select: none; /* Old versions of Firefox */
		-ms-user-select: none; /* Internet Explorer/Edge */
		user-select: none;
	}

	tr:hover {
		background-color: #efefef;
		cursor: pointer;
	}

	tr:scope {
		background-color: seagreen;
	}

	.selected {
		background-color: lightgrey;
	}

	.variable {
		background-color: var(--bs-danger);
		color: white;
	}

	.unit {
		background-color: var(--bs-success);
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
</style> -->
