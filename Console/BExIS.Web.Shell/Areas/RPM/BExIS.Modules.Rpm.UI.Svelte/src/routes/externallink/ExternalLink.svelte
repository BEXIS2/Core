<script lang="ts">
	import { MultiSelect, TextInput, helpStore, type listItemType } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faXmark, faSave } from '@fortawesome/free-solid-svg-icons';

	import { createEventDispatcher, onMount } from 'svelte';

	import suite from './externalLink';
	import { externalLinkTypeEnum, externalLinkType, type prefixCategoryType } from '$lib/components/meaning/types';
	import { create, update } from './services';
	import { externalLinksStore, externalLinkTypesStore ,prefixCategoryStore, prefixesStore } from '$lib/components/meaning/stores';

	export let link: externalLinkType;
	$: link;

	let help: boolean = true;
	let loaded = false;

	// validation
	let res = suite.get();
	$: isValid = res.isValid();

	// data
	import { helpInfoList } from './help';
	import UrlPreview from './UrlPreview.svelte';

	let linksList = $externalLinksStore
	let prefixCategoryAsList:prefixCategoryType[] = $prefixCategoryStore;
	let prefixes = $prefixesStore;
	let types:listItemType[] = $externalLinkTypesStore;
	$: types;
	
	const dispatch = createEventDispatcher();
	
	onMount(() => {
		console.log("🚀 ~ file: ExternalLink.svelte:28 ~ linksList:", linksList)
		helpStore.setHelpItemList(helpInfoList);

		// reset & reload validation
		suite.reset();

		// set types =

		console.log('🚀 ~ file: ExternalLink.svelte:45 ~ onMount ~ types:', types);

		setTimeout(async () => {
			if (link.id > 0) {
				res = suite(link, '');
			} // run validation only if start with an existing
		}, 10);

		//updateAutoCompleteList();

		loaded = true;
	});

	function onChangeFn(e) {
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			res = suite(link, e.target.id);
		}, 100);
	}

	function cancel() {
		suite.reset();
		dispatch('cancel');
	}

	async function submit() {
		var s = (await (link.id == 0)) ? create(link) : update(link);

		if ((await s).status === 200) {
			dispatch('success');
		} else {
			dispatch('fail');
		}

		suite.reset();
	}

	//change event: if select change check also validation only on the field
	// *** is the id of the input component
	function onSelectHandler(e, id) {

		// // set prefix = null if type = prefix
		// // type id for prefix == 1
	  console.log(id,e);
		if(id=="type" && e.detail.text=="prefix"){ // if type and prefix
		 	console.log("reset prefix");
		 	link.prefix = undefined;
		}
		

		res = suite(link, id); 
 }

</script>

{#if loaded}
	<form on:submit|preventDefault={submit}>
		<div id="link-{link.id}-form" class=" space-y-5 card shadow-md p-5">
			<div class="flex gap-5 items-center">
				<div class="w-1/2">
					<TextInput
						id="name"
						bind:value={link.name}
						on:change
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
					bind:target={link.type}
					placeholder="-- Please select --"
					invalid={res.hasErrors('type')}
					feedback={res.getErrors('type')}
					on:change={(e) => onSelectHandler(e, 'type')}
					{help}
				/>
			</div>

				<div class="w-1/4">
					{#if link.type?.id === externalLinkTypeEnum.prefix}
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
						bind:target={link.prefixCategory}
						placeholder="-- Please select --"
						invalid={res.hasErrors('type')}
						feedback={res.getErrors('type')}
						on:change={(e) => onSelectHandler(e, 'type')}
						{help}
					/>
				{/if}
				</div>


			</div>
			<div class="flex gap-5">
				{#if link.type?.id !== externalLinkTypeEnum.prefix}
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
						bind:target={link.prefix}
						placeholder="-- Please select --"
						invalid={res.hasErrors('type')}
						feedback={res.getErrors('type')}
						on:change={(e) => onSelectHandler(e, 'type')}
						{help}
					/>
				</div>
				{/if}
				<div class="grow">
				<TextInput
					id="uri"
					label="Uri"
					bind:value={link.uri}
					on:change
					on:input={onChangeFn}
					placeholder="Uri"
					valid={res.isValid('uri')}
					invalid={res.hasErrors('uri')}
					feedback={res.getErrors('uri')}
					{help}
				/>
			</div>
			</div>

			<UrlPreview bind:link={link} />

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
					disabled={!isValid}
				>
					<Fa icon={faSave} /></button>
			</div>
		</div>
	</form>
{/if}
