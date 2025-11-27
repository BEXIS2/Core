<script lang="ts">
	import { MultiSelect, TextInput, helpStore, type listItemType } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faXmark, faSave } from '@fortawesome/free-solid-svg-icons';

	import { createEventDispatcher, onMount, tick } from 'svelte';

	import suite from './externalLink';
	import {
		externalLinkTypeEnum,
		externalLinkType,
		type prefixCategoryType
	} from '$lib/components/meaning/types';
	import { create, update } from './services';
	import {
		externalLinksStore,
		externalLinkTypesStore,
		prefixCategoryStore,
		prefixesStore
	} from '$lib/components/meaning/stores';

	export let link: externalLinkType;
	$: link;

	// local editable copy to guarantee reactivity on nested property changes
	let localLink: externalLinkType = link ? { ...link } : ({} as externalLinkType);
	// keep local copy in sync when parent provides a new object
	$: if (link && link.id !== localLink.id) localLink = { ...link };
	

	let help: boolean = true;
	let loaded = false;

	// validation
	let res = suite.get();

	$: isValid = res.isValid()
	$: isChanged = false


	// data
	import { helpInfoList } from './help';
	import UrlPreview from './UrlPreview.svelte';

	let linksList = $externalLinksStore;
	let prefixCategoryAsList: prefixCategoryType[] = $prefixCategoryStore;
	let prefixes = $prefixesStore;
	let types: listItemType[] = $externalLinkTypesStore;
	$: types;

	const dispatch = createEventDispatcher();

	onMount(() => {
        console.log('🚀 ~ file: ExternalLink.svelte:28 ~ linksList:', linksList);
        helpStore.setHelpItemList(helpInfoList);

        // reset & reload validation
        suite.reset();

        console.log('🚀 ~ file: ExternalLink.svelte:45 ~ onMount ~ types:', types);

        if (link.id == 0) {
            suite.reset();
        } else {
			(async () => {
				await tick();
				// full validation: DO NOT pass an empty string as fieldName
				res = suite({ name: localLink.name, uri: localLink.uri, type: localLink.type, id: localLink.id }, undefined);
				console.log('Validation result init:', res.isValid(), res.getErrors(), res.getState && res.getState());
			})();
         }
 
         //updateAutoCompleteList();
 
         loaded = true;
     });

	function onChangeFn(e: Event) {
		// wait for DOM / bindings to update, then validate
		const target = e.target as EventTarget & { id?: string } | null;
		const id = target?.id ?? '';
		// ensure Svelte notices nested changes
		localLink = { ...localLink };
		console.log('onChangeFn ', id);
		console.log('localLink ', localLink);
		const uri = localLink.uri;
		
		(async () => {
			await tick();
			//res = suite({ name: localLink.name, uri: localLink.uri, type: localLink.type, id: localLink.id });
			res = suite({ name: localLink.name, uri: localLink.uri, type: localLink.type, id: localLink.id }, id || undefined);
		
		})();
		console.log('Changed ', e);
		console.log('Validation result:', res.isValid(), res.getErrors());

	}

	function cancel() {
		suite.reset();
		dispatch('cancel');
	}

	async function submit() {
		// use localLink for create/update
		var s = (await (localLink.id == 0)) ? create(localLink) : update(localLink);

		if ((await s).status === 200) {
			// sync back to parent prop via event
			dispatch('success', { link: localLink });
		} else {
			dispatch('fail');
		}

		suite.reset();
	}

	//change event: if select change check also validation only on the field
	// *** is the id of the input component
	function onSelectHandler(e: CustomEvent<any>, id: string) {
		// // set prefix = null if type = prefix
		// // type id for prefix == 1
		console.log(id, e);
		if (id == 'type' && e.detail.text == 'prefix') {
			localLink.prefix = undefined;
		}
		(async () => {
			await tick();
			res = suite({ name: localLink.name, uri: localLink.uri, type: localLink.type, id: localLink.id }, id || undefined);
			localLink = { ...localLink };
		})();
	}
</script>

{#if loaded}
	<form on:submit|preventDefault={submit}>
		<div id="link-{link.id}-form" class=" space-y-5 card shadow-md p-5">
			<div class="flex gap-5 items-center">
				<div class="w-1/2">
					<TextInput
						id="name"
						label="Name"
						required={true}
						bind:value={localLink.name}
						on:change={() => isChanged = true}
						on:input={onChangeFn}
						placeholder="Name"
						valid={res.isValid('name')}
						invalid={res.hasErrors('name')}
						feedback={res.getErrors('name')}
						{help}
					/>
				</div>
			</div>

			<div class="flex gap-5 items-center">
				<div class="w-1/4">
					<MultiSelect
						id="type"
						title="Type"
						bind:source={types}
						itemId="id"
						itemLabel="text"
						itemGroup="group"
						complexSource={true}
						complexTarget={true}
						isMulti={false}
						clearable={false}
						required={true}
						bind:target={localLink.type}
						placeholder="-- Please select --"
						invalid={res.hasErrors('type')}
						feedback={res.getErrors('type')}
						on:change={(e) => onSelectHandler(e, 'type')}
						on:change={() => isChanged = true}
						{help}
					/>
				</div>

				<div class="w-1/4">
					{#if localLink.type?.id === externalLinkTypeEnum.prefix}
						<MultiSelect
							id="prefixCategory"
							title="Prefix Category"
							bind:source={prefixCategoryAsList}
							itemId="id"
							itemLabel="name"
							itemGroup="group"
							complexSource={true}
							complexTarget={true}
							isMulti={false}
							clearable={true}
							bind:target={localLink.prefixCategory}
							placeholder="-- Please select --"
							invalid={res.hasErrors('prefixCategory')}
							feedback={res.getErrors('prefixCategory')}
							on:change={(e) => onSelectHandler(e, 'prefixCategory')}
							on:change={() => isChanged = true}
							{help}
						/>
					{/if}
				</div>
			</div>
			<div class="flex gap-5">
				{#if localLink.type?.id !== externalLinkTypeEnum.prefix}
					<div class="w-1/4">
						<MultiSelect
							id="prefix"
							title="Prefix"
							bind:source={prefixes}
							itemId="id"
							itemLabel="text"
							itemGroup="group"
							complexSource={true}
							complexTarget={true}
							isMulti={false}
							clearable={true}
							bind:target={localLink.prefix}
							placeholder="-- Please select --"
							invalid={res.hasErrors('prefix')}
							feedback={res.getErrors('prefix')}
							on:change={(e) => onSelectHandler(e, 'prefix')}
							on:change={() => isChanged = true}
							{help}
						/>
					</div>
				{/if}
				<div class="grow">
					<TextInput
						id="uri"
						label="Uri"
						bind:value={localLink.uri}
						on:change={() => isChanged = true}
						on:input={onChangeFn}
						placeholder="Uri"
						valid={res.isValid('uri')}
						invalid={res.hasErrors('uri')}
						feedback={res.getErrors('uri')}
						required={true}
						{help}
					/>
				</div>
			</div>

			<UrlPreview bind:link={localLink} />

			<div class="py-5 text-right col-span-2">
				<!-- svelte-ignore a11y-mouse-events-have-key-events -->
				<button
					type="button"
					class="btn variant-filled-warning h-9 w-16 shadow-md"
					title="Cancel"
					id="cancel"
					on:click={() => cancel()}><Fa icon={faXmark} /></button
				>
				<!-- svelte-ignore a11y-mouse-events-have-key-events -->
				<button
					type="submit"
					class="btn variant-filled-primary h-9 w-16 shadow-md"
					title="Save external link, {link.name}"
					id="save"
					disabled={!(isChanged && isValid)}
				>
					<Fa icon={faSave} /></button
				>
			</div>
		</div>
	</form>
{/if}
