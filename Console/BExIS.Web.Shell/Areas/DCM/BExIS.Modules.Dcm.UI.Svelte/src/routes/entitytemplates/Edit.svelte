<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	import { onMount } from 'svelte';
	import { fade } from 'svelte/transition';

	// ui Components
	import Fa from 'svelte-fa';
	import { DropdownKVP, MultiSelect, TextArea, TextInput, Spinner } from '@bexis2/bexis2-core-ui';
	import { faSave, faTrash } from '@fortawesome/free-solid-svg-icons/index';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import ContentContainer from '../../lib/components/ContentContainer.svelte';
	import { Modal, getModalStore } from '@skeletonlabs/skeleton';
	const modalStore = getModalStore();

	import EntryContainer from './EntryContainer.svelte';

	// validation
	import suite from './edit';

	// services
	import {
		getEntityTemplate,
		saveEntityTemplate,
		getSystemKeys
	} from '../../services/EntityTemplateCaller';

	// types
	import type { EntityTemplateModel } from '../../models/EntityTemplate';
	import type { listItemType } from '@bexis2/bexis2-core-ui';
	import type { ModalSettings } from '@skeletonlabs/skeleton';

	// help
	import { editHelp } from './help';
	import { helpStore } from '@bexis2/bexis2-core-ui';

	//Set list of help items and clear selection
	helpStore.setHelpItemList(editHelp);

	export let id: number = 0;

	export let hooks = [];
	export let metadataStructures: listItemType[] = [];
	export let dataStructures = [];
	let systemKeys;
	export let entities = [];
	export let groups = [];
	export let filetypes: string[];

	const dispatch = createEventDispatcher();

	let entityTemplate: EntityTemplateModel;

	$: entityTemplate;
	$: systemKeys;

	$: loaded = false;

	let type = [];
	$: type;

	suite.reset();

	onMount(async () => {
		console.log('start entity template', id);
		load();
		console.log('onmount');
	});

	async function load() {
		entityTemplate = await getEntityTemplate(id);
		console.log('load entity', entityTemplate);
		console.log('load filetypes', filetypes);
		updateSystemKeys('metadataStructure');

		// if id > 0 then run validation
		if (id > 0) {
			res = suite(entityTemplate);
			console.log('validate', res);
		}
	}

	async function handleSubmit() {
		console.log('before submit', entityTemplate);
		const res = await saveEntityTemplate(entityTemplate);
		if (res != false) {
			console.log('save', res);
			dispatch('save', res);
		}
	}

	// validation
	let res = suite.get();
	// flag to enable submit button
	$: disabled = !res.isValid();

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	async function onChangeHandler(e) {
		//console.log("input changed", e)
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			res = suite(entityTemplate, e.target.id);
		}, 10);

		// reload systemkeys if metdatatstructure has chagened
		console.log(e.target.id);

		// if metadata structure selection changed,
		updateSystemKeys(e.target.id);
	}

	async function updateSystemKeys(targetid) {
		if (targetid === 'metadataStructure') {
			let id = 0;
			if (entityTemplate.metadataStructure != undefined) {
				id = entityTemplate.metadataStructure.id;
			}

			systemKeys = await getSystemKeys(id);
			console.log(systemKeys);
		}
	}

	function onCancel() {
		let x = 'create';
		if (id > 0) {
			x = 'edit';
		}

		const modal: ModalSettings = {
			type: 'confirm',
			title: 'Cancel ' + x + ' entity template',
			body: 'Are you sure you wish to cancel the current form?',
			// TRUE if confirm pressed, FALSE if cancel pressed
			response: (r: boolean) => {
				if (r === true) {
					dispatch('cancel');
				}
			}
		};

		modalStore.trigger(modal);
	}
</script>

{#if entityTemplate}
	<ContentContainer>
		<form on:submit|preventDefault={handleSubmit}>
			<div class="w-full grid grid-cols-1 md:grid-cols-2 gap-4 py-4">
				<TextInput
					id="name"
					label="Template Name"
					bind:value={entityTemplate.name}
					valid={res.isValid('name')}
					invalid={res.hasErrors('name')}
					feedback={res.getErrors('name')}
					on:input={onChangeHandler}
					required={true}
					placeholder="Define a unique content-related name for your template."
					help={true}
				/>

				<DropdownKVP
					id="entityType"
					title="Entity Type"
					required={true}
					source={entities}
					bind:target={entityTemplate.entityType}
					valid={res.isValid('entityType')}
					invalid={res.hasErrors('entityType')}
					feedback={res.getErrors('entityType')}
					on:change={onChangeHandler}
					complexTarget={true}
					help={true}
				/>

				<TextArea
					id="description"
					label="Description"
					bind:value={entityTemplate.description}
					valid={res.isValid('description')}
					invalid={res.hasErrors('description')}
					feedback={res.getErrors('description')}
					on:input={onChangeHandler}
					required={true}
					placeholder="Briefly describe in which cases this template should be used. Based on an entity or use case."
					help={true}
				/>
			</div>


					<h3 class="h3">Metadata</h3>

					<div class="py-5 w-full grid grid-cols-1 md:grid-cols-2 gap-4">
					<DropdownKVP
						id="metadataStructure"
						title="Metadata Schema"
						bind:target={entityTemplate.metadataStructure}
						source={metadataStructures}
						valid={res.isValid('metadataStructure')}
						invalid={res.hasErrors('metadataStructure')}
						feedback={res.getErrors('metadataStructure')}
						on:change={onChangeHandler}
						complexTarget={true}
						required={true}
						help={true}
					/>
					<div class="flex flex-col gap-4">
					{#if systemKeys}
						<EntryContainer>
							<MultiSelect
								id="metadataFields"
								title="Required Fields"
								source={systemKeys}
								bind:target={entityTemplate.metadataFields}
								itemId="id"
								itemLabel="text"
								itemGroup="group"
								complexSource={true}
								help={true}
							/>
						</EntryContainer>
					{/if}


					<EntryContainer>
						<div id="invalidSaveMode" />
						<SlideToggle
					  active="bg-primary-500"
							name="Invalid-save-mode"
							bind:checked={entityTemplate.metadataInvalidSaveMode}
						>
							Allow saving with empty required fields
						</SlideToggle>
					</EntryContainer>
				</div>
				</div>


			<div class="flex flex-col space-y-4">
					

			<h3 class="h3">Data Structure</h3>
			<div class="py-5 w-full grid grid-cols-1 md:grid-cols-2 gap-4">
				<EntryContainer>
					<div role="group" class="flex flex-col gap-4"  on:mouseover={() => helpStore.show('hasDatastructure')} on:focus={() => helpStore.show('hasDatastructure')}>
						<SlideToggle 
						active="bg-primary-500"
						name="use_data_structure" 
						bind:checked={entityTemplate.hasDatastructure}>
							Allow to use data structures
						</SlideToggle>

						{#if entityTemplate.hasDatastructure}
							<MultiSelect
								id="datastructures"
								title="Limit the selection of allowed data structures"
								source={dataStructures}
								bind:target={entityTemplate.datastructureList}
								itemId="key"
								itemLabel="value"
								complexSource={true}
								help={true}
							/>
						{/if}
					</div>
				</EntryContainer>
			</div>
		</div>

			<h3 class="h3">Administration</h3>
			<p class="p">Set permissions per default to the following groups</p>
			<div class="py-5 w-full grid grid-cols-1 md:grid-cols-2 gap-4">
				<EntryContainer>
					<MultiSelect
						id="permissionsFull"
						title="Full"
						source={groups}
						bind:target={entityTemplate.permissionGroups.full}
						itemId="key"
						itemLabel="value"
						complexSource={true}
						help={true}
					/>
				</EntryContainer>

				<EntryContainer>
					<MultiSelect
						id="permissionsViewEditGrant"
						title="View, Edit and Grant"
						source={groups}
						bind:target={entityTemplate.permissionGroups.viewEditGrant}
						itemId="key"
						itemLabel="value"
						complexSource={true}
						help={true}
					/>
				</EntryContainer>

				<EntryContainer>
					<MultiSelect
						id="permissionsViewEdit"
						title="View and Edit"
						source={groups}
						bind:target={entityTemplate.permissionGroups.viewEdit}
						itemId="key"
						itemLabel="value"
						complexSource={true}
						help={true}
					/>
				</EntryContainer>

				<EntryContainer>
					<MultiSelect
						id="permissionsView"
						title="View"
						source={groups}
						bind:target={entityTemplate.permissionGroups.view}
						itemId="key"
						itemLabel="value"
						complexSource={true}
						help={true}
					/>
				</EntryContainer>
			</div>
			
			<h3 class="h3">Notifications</h3>
			<div class="py-5 w-full grid grid-cols-1 md:grid-cols-2 gap-4">
				<EntryContainer>
					<MultiSelect
						id="notification"
						title="Send notifications per default to the following groups"
						source={groups}
						bind:target={entityTemplate.notificationGroups}
						itemId="key"
						itemLabel="value"
						complexSource={true}
						help={true}
					/>
				</EntryContainer>
			</div>

			<h3 class="h3">Dataset Settings</h3>
			<div class="py-5 w-full grid xs:grid-cols-1 md:grid-cols-2 gap-4">
				<EntryContainer>
					<MultiSelect
						id="disabledHooks"
						title="Disable dataset components"
						source={hooks}
						bind:target={entityTemplate.disabledHooks}
						help={true}
					/>
				</EntryContainer>

				<EntryContainer>
					<MultiSelect
						id="allowedFileTypes"
						title="Restrict allowed file types for file upload"
						source={filetypes}
						bind:target={entityTemplate.allowedFileTypes}
						help={true}
					/>
				</EntryContainer>
			</div>

			<div class="py-5 grow text-right gap-2">
				<button title="cancel" type="button" class="btn variant-filled-warning" on:click={onCancel}
					><Fa icon={faTrash} /></button
				>
				<button title="save" type="submit" class="btn variant-filled-primary" {disabled}
					><Fa icon={faSave} /></button
				>
			</div>
		</form>
	</ContentContainer>
{:else}
	<Spinner textCss="text-secondary-500" />
{/if}

<Modal />
