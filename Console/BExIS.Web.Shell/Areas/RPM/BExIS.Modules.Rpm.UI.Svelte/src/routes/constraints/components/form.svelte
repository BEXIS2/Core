<script lang="ts">
	import { TextInput, TextArea, DropdownKVP, helpStore, CodeEditor } from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';
	import { faSave, faXmark } from '@fortawesome/free-solid-svg-icons';

	import type {
		ConstraintListItem,
		ConstraintValidationResult,
		DomainConstraintListItem,
		RangeConstraintListItem,
		PatternConstraintListItem
	} from '../models';
	import { onMount } from 'svelte';
	import * as apiCalls from '../services/apiCalls';
	import { fade, slide } from 'svelte/transition';
	import DomainForm from './domainForm.svelte';
	import RangeForm from './rangeForm.svelte';
	import PatternForm from './patternForm.svelte';
	import Warning from './warning.svelte';

	//notifications
	import { notificationStore, notificationType } from '@bexis2/bexis2-core-ui';
	// event
	import { createEventDispatcher } from 'svelte';
	const dispatch = createEventDispatcher();

	// validation
	import suite from './form';
	import { FileButton, getModalStore, type ModalSettings } from '@skeletonlabs/skeleton';
	import type { ValidationResult } from '../../../models';

	// load form result object
	let res = suite.get();

	// use to actived save if form is valid
	$: disabled = !res.isValid();

	// init unit
	export let constraint: ConstraintListItem;
	export let constraints: ConstraintListItem[];

	let domainConstraint: DomainConstraintListItem;
	let rangeConstraint: RangeConstraintListItem;
	let patternConstraint: PatternConstraintListItem;
	$: setConstraintByType(constraint);
	let ct: string[] = [];
	$: constraintTypes = ct.map((c) => ({ id: c, text: c }));
	const modalStore = getModalStore();
	let warning: string = 'Changing the Contstrait may cause inconsistencies in Datasets.';

	onMount(async () => {
		ct = await apiCalls.GetConstraintTypes();
		if (constraint.id == 0) {
			suite.reset();
		} else {
			setTimeout(async () => {
				res = suite({ constraint: constraint, constraints: constraints }, '');
			}, 10);
		}
	});

	async function setConstraintByType(constraint: ConstraintListItem) {
		if (constraint.type == 'Domain') {
			if (domainConstraint == undefined) {
				domainConstraint = {
					id: constraint.id,
					version: constraint.version,
					name: constraint.name,
					description: constraint.description,
					formalDescription: constraint.formalDescription,
					domain: '',
					negated: constraint.negated,
					inUse: constraint.inUse,
					variableIDs: constraint.variableIDs
				};
				if (domainConstraint.id != 0) {
					domainConstraint = await apiCalls.GetDomainConstraint(constraint.id);
				}
			} else {
				domainConstraint.name = constraint.name;
				domainConstraint.description = constraint.description;
			}
			if (domainConstraint.id != constraint.id) {
				domainConstraint = await apiCalls.GetDomainConstraint(constraint.id);
			}
		}

		if (constraint.type == 'Range') {
			if (rangeConstraint == undefined) {
				rangeConstraint = {
					id: constraint.id,
					version: constraint.version,
					name: constraint.name,
					description: constraint.description,
					formalDescription: constraint.formalDescription,
					lowerbound: 0,
					upperbound: 0,
					lowerboundIncluded: true,
					upperboundIncluded: true,
					negated: constraint.negated,
					inUse: constraint.inUse,
					variableIDs: constraint.variableIDs
				};
				if (rangeConstraint.id != 0) {
					rangeConstraint = await apiCalls.GetRangeConstraint(constraint.id);
				}
			} else {
				rangeConstraint.name = constraint.name;
				rangeConstraint.description = constraint.description;
			}
			if (rangeConstraint.id != constraint.id) {
				rangeConstraint = await apiCalls.GetRangeConstraint(constraint.id);
			}
		}

		if (constraint.type == 'Pattern') {
			if (patternConstraint == undefined) {
				patternConstraint = {
					id: constraint.id,
					version: constraint.version,
					name: constraint.name,
					description: constraint.description,
					formalDescription: constraint.formalDescription,
					pattern: '',
					negated: constraint.negated,
					inUse: constraint.inUse,
					variableIDs: constraint.variableIDs
				};
				if (patternConstraint.id != 0) {
					patternConstraint = await apiCalls.GetPatternConstraint(constraint.id);
				}
			} else {
				patternConstraint.name = constraint.name;
				patternConstraint.description = constraint.description;
			}
			if (patternConstraint.id != constraint.id) {
				patternConstraint = await apiCalls.GetPatternConstraint(constraint.id);
			}
		}
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

	function submit() {
		if (constraint.inUse) {
			const modal: ModalSettings = {
				type: 'confirm',
				title: 'Save Constraint',
				body: warning,
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: (r: boolean) => {
					if (r === true) {
						save();
					}
				}
			};
			modalStore.trigger(modal);
		} else {
			save();
		}
	}

	async function save() {
		let message: string;
		let result: ConstraintValidationResult;
		switch (constraint.type) {
			case 'Domain':
				result = await apiCalls.EditDomainConstraint(domainConstraint);
				break;

			case 'Range':
				result = await apiCalls.EditRangeConstraint(rangeConstraint);
				break;

			case 'Pattern':
				result = await apiCalls.EditPatternConstraint(patternConstraint);
				break;

			default:
				let vr: ValidationResult = {
					isValid: false,
					validationItems: [{ name: 'Constraint Type', message: 'no Constraint Type is chosen' }]
				};
				result = {
					validationResult: vr,
					constraintListItem: constraint
				};
				break;
		}

		if (result.validationResult.isValid != true) {
			message = "Can't save Constraint";
			if (result.constraintListItem.name != '') {
				message += ' "' + result.constraintListItem.name + '" .';
			}
			if (
				result.validationResult.validationItems != undefined &&
				result.validationResult.validationItems.length > 0
			) {
				result.validationResult.validationItems.forEach((validationItem) => {
					message += '<li>' + validationItem.message + '</li>';
				});
			}
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: message
			});
		} else {
			message = 'Constraint "' + result.constraintListItem.name + '" saved.';
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: message
			});
			suite.reset();
			dispatch('save');
		}
	}

	function cancel() {
		suite.reset();
		dispatch('cancel');
	}
</script>

{#if constraint && constraintTypes}
	{#if constraint.inUse}
		<Warning {constraint} />
	{/if}
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
				
					{#if constraint.formalDescription && constraint.formalDescription != ''}
					<p class="ml-2">
						{constraint.formalDescription}
					</p>
					{:else}
					<p class="ml-2 text-surface-600">
						Will be generate by the System
					</p>
					{/if}
			</div>

			<div class="pb-3" title="Type">
				{#if constraint.id == 0}
					<DropdownKVP
						id="constraintTypes"
						title="Constraint Type"
						bind:target={constraint.type}
						source={constraintTypes}
						required={true}
						complexTarget={false}
						help={true}
					/>
				{:else}
					<!-- svelte-ignore a11y-mouse-events-have-key-events -->
					<!-- svelte-ignore a11y-label-has-associated-control -->
					<label
						on:mouseover={() => {
							helpStore.show('constraintTypes');
						}}>Constraint Type</label
					>
					<p class="ml-2">{constraint.type}</p>
				{/if}
			</div>

			{#if constraint.type && constraint.type != ''}
				<div class="pb-3 col-span-2">
					{#if constraint.type == 'Domain'}
						<DomainForm
							{domainConstraint}
							on:true={() => {
								disabled = true;
							}}
							on:false={() => {
								disabled = false;
							}}
						/>
					{:else if constraint.type == 'Range'}
						<RangeForm
							{rangeConstraint}
							on:true={() => {
								disabled = true;
							}}
							on:false={() => {
								disabled = false;
							}}
						/>
					{:else if constraint.type == 'Pattern'}
						<PatternForm
							{patternConstraint}
							on:true={() => {
								disabled = true;
							}}
							on:false={() => {
								disabled = false;
							}}
						/>
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
