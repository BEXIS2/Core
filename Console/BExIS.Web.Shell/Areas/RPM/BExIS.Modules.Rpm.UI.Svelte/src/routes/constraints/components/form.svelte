<script lang="ts">
	import { TextInput, TextArea, DropdownKVP, helpStore, CodeEditor } from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';
	import { faSave, faXmark, faArrowUpFromBracket } from '@fortawesome/free-solid-svg-icons';

	import type { ConstraintListItem, ConstraintValidationResult} from '../models';
	import { onMount } from 'svelte';
	import * as apiCalls from '../services/apiCalls';
	import { fade, slide } from 'svelte/transition';
	import papa from 'papaparse';
	import DomainForm from './domainForm.svelte';
	import RangeForm from './rangeForm.svelte';
	import PatternForm from './patternForm.svelte';

	//notifications
	import { notificationStore, notificationType } from '@bexis2/bexis2-core-ui';
	// event
	import { createEventDispatcher } from 'svelte';
	const dispatch = createEventDispatcher();

	// validation
	import suite from './form';
	import { FileButton } from '@skeletonlabs/skeleton';
	

	// load form result object
	let res = suite.get();

	// use to actived save if form is valid
	$: disabled = !res.isValid();

	// init unit
	export let constraint: ConstraintListItem;
	export let constraints: ConstraintListItem[];

	let domainConstraint: any;
	let rangeConstraint: any;
	let patternConstraint: any;
	$: getConstraintByType(constraint.type);
	let ct: string[] = [];
	$: constraintTypes = ct.map((c) => ({ id: c, text: c }));

	
	
	onMount(async () => {
		ct = await apiCalls.GetConstraintTypes();
		if (constraint.id == 0) {
			suite.reset();
		}
	});

	function getConstraintByType(type: string){
		domainConstraint = type != "Domain" ? domainConstraint : {
			id: constraint.id,
			version: constraint.version,
			name: constraint.name,
			description: constraint.description,
			formalDescription: constraint.formalDescription,
			domain: domainConstraint != undefined ? domainConstraint.domain : "",
			inUse: false
		};
		rangeConstraint = type != "Range" ? rangeConstraint : {
			id: constraint.id,
			version: constraint.version,
			name: constraint.name,
			description: constraint.description,
			formalDescription: constraint.formalDescription,
			lowerbound: rangeConstraint != undefined ? rangeConstraint.lowerbound : "",
			upperbound: rangeConstraint != undefined ? rangeConstraint.upperbound : "",
			lowerboundIncluded: rangeConstraint != undefined ? rangeConstraint.lowerboundIncluded : "",
			upperboundIncluded: rangeConstraint != undefined ? rangeConstraint.upperboundIncluded : "",
			inUse: false
		};

		patternConstraint = type != "Pattern" ? patternConstraint : {
			id: constraint.id,
			version: constraint.version,
			name: constraint.name,
			description: constraint.description,
			formalDescription: constraint.formalDescription,
			pattern: patternConstraint != undefined ? patternConstraint.pattern : "",
			inUse: false
		};

	}

	function fileParser(event: any) {
		if(event.target != null)
		{
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

	function joinRows(data: any) :string
	{
		return data.join('\n').trim().replaceAll('\t','')
	}

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: any) {
		//console.log("input changed", e)
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			// check changed field
			res = suite({ constraint: constraint, constraints: constraints }, e.target.id);
		}, 10);
	}

	async function submit() {

	}

	function cancel() {
		suite.reset();
		dispatch('cancel');
	}
</script>

{#if constraint && constraintTypes}
	<form on:submit|preventDefault={submit}>
		<div class="grid grid-cols-2 gap-5">
			<div class="pb-3">
				<TextInput
					id="name"
					label="Name"
					help={true}
					required={true}
					bind:value={constraint.name}
					on:input={onChangeHandler}
					valid={res.isValid('name')}
					invalid={res.hasErrors('name')}
					feedback={res.getErrors('name')}
				/>
			</div>

			<div class="pb-3 col-span-2">
				<TextArea
					id="description"
					label="Description"
					help={true}
					required={true}
					bind:value={constraint.description}
					on:input={onChangeHandler}
					valid={res.isValid('description')}
					invalid={res.hasErrors('description')}
					feedback={res.getErrors('description')}
				/>
			</div>

			<div class="pb-3 col-span-2">
				<!-- svelte-ignore a11y-label-has-associated-control -->
				<label>Formal Description</label>
				<p>
					{#if constraint.formalDescription && constraint.formalDescription !=""}
						{constraint.formalDescription}
					{:else}
						Will be generate by the System
					{/if}
				</p>
			</div>

			<div class="pb-3" title="Type">
				<DropdownKVP
					id="constraintTypes"
					title="constraint Type"
					bind:target={constraint.type}
					source={constraintTypes}
					required={true}
					complexTarget={false}
					help={true}
				/>
			</div>
			
			<div class="pb-3 text-right mt-7" title="Type">
				{#if constraint.type == 'Domain'}
				<div in:fade out:fade>
				<!-- svelte-ignore missing-declaration -->
					<FileButton id="uploadCsv" title="Upload CSV" button="btn variant-filled-secondary h-9 w-16 shadow-md" name="uploadCsv" on:change={fileParser}><Fa icon={faArrowUpFromBracket} /></FileButton>
				</div>
				{/if}
			</div>

			{#if constraint.type && constraint.type != ""}
			<div class="pb-3 col-span-2">
				{#if constraint.type == 'Domain'} 
					<DomainForm {domainConstraint}/>
				{:else if constraint.type == 'Range'}
					<RangeForm {rangeConstraint}/>
				{:else if constraint.type == 'Pattern'}
					<PatternForm {patternConstraint}/>
				{/if}
			</div>
			{/if}

			<div class="py-5 text-right col-span-2">
				<!-- svelte-ignore a11y-mouse-events-have-key-events -->
				<button
					type="button"
					class="btn variant-filled-warning h-9 w-16 shadow-md"
					title="Cancel"
					id="cancel"
					on:mouseover={() => {
						helpStore.show('cancel');
					}}
					on:click={() => cancel()}><Fa icon={faXmark} /></button
				>
				<!-- svelte-ignore a11y-mouse-events-have-key-events -->
				<button
					type="submit"
					class="btn variant-filled-primary h-9 w-16 shadow-md"
					title="Save Constraint, {constraint.name}"
					id="save"
					{disabled}
					on:mouseover={() => {
						helpStore.show('save');
					}}><Fa icon={faSave} /></button
				>
			</div>
		</div>
	</form>
{/if}
