<script lang="ts">
	import {
		empty,
		getNodeByPath,
		getPartyIdByPath,
		hasValue,
		isActive,
		setActive,
		setInactive,
		toggleShow,
		activateShow,
		showDescriptionHandler,
		hideDescriptionHandler
	} from '$lib/components/utils/metadata/metadataComponentUtils';
	import { convertDisplayName } from '../../../lib/components/utils/metadata/metadataShared';
	import {
		faPlus,
		faChevronUp,
		faChevronDown,
		faQuestion,
		faTrash,
		faCircleQuestion
	} from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import {
		activeStore,
		hideStore,
		metadataStore,
		validationStore,
		showAllDescriptionsStore
	} from '$lib/components/utils/metadata/stores';
	import { onMount } from 'svelte';

	export let required: boolean = false;
	//  $:required;
	export let path: string;
	export let p: string = '';
	export let description: string = '';

	let label: string =
		path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;

	// set showDescription  if showAllDescriptionsStore is true or false; use local if showAllDescriptionsStore is null or undefined
	$: showDescription =
		$showAllDescriptionsStore !== null && $showAllDescriptionsStore !== undefined
			? $showAllDescriptionsStore
			: false;

	const togglePath = p !== '' ? p : path;

	export let active: boolean = false;
	$: active;

	onMount(() => {
		//console.log('complexComponentWrapper onMount', path, $activeStore);
		if (!$activeStore.includes(path)) {
			initActivity();
		} else {
			active = true;
		}
	});

	function initActivity() {
		active = isActive(path, required);

		if (active) {
			setActive(path);
		} else {
			setInactive(path);
		}
	}

	function changeFn(a: boolean) {
		active = !a;

		if (active) {
			setActive(path);
			activateShow(path);
		} else {
			setInactive(path);
			// remove from validation store
			removeFromValidationStore(path);
			// empty data in metadata store for this path and all child paths
			const data = getNodeByPath(path);
			empty(data);
		}

		// console.log('active',active,path, $activeStore);
	}

	function removeFromValidationStore(path: string) {
		validationStore.update((store) => {
			return {
				...store,
				simpleTypeValidationItems: store.simpleTypeValidationItems.filter(
					(item) => !item.path.startsWith(path)
				),
				complexTypeValidationItems: store.complexTypeValidationItems.filter(
					(item) => !item.path.startsWith(path)
				)
			};
		});
	}

	function handleShowDescription(e: MouseEvent | FocusEvent) {
		if (description) {
			const desc = { detail: { description: description ? description : '', id: path } };
			showDescriptionHandler(desc, 'complex');
		}
	}

	function handleHideDescription(e: MouseEvent | FocusEvent) {
		hideDescriptionHandler(e, 'complex');
	}

	function handleToggleShow() {
		if (!active || !$activeStore.includes(path)) {
			return;
		}
		toggleShow(togglePath);
	}
</script>

<div
	class="card flex min-h-8 bg-primary-300 dark:bg-primary-800 items-center gap-2"
	role="button"
	tabindex="0"
	on:mouseover={handleShowDescription}
	on:mouseleave={handleHideDescription}
	on:focus={handleShowDescription}
	on:blur={handleHideDescription}
>
	<div>
		{#if !active}
			<button
				class="badge mt-1 ml-1 mr-1"
				on:click={() => changeFn(active)}
				title="Add {convertDisplayName(label, true)} node"><Fa icon={faPlus} /></button
			>
		{:else if $activeStore.includes(path)}
			{#if !$hideStore.includes(path)}
				<button
					class="btn-sm text-right"
					title="Open or close {convertDisplayName(label, true)}"
					on:click={handleToggleShow}><Fa icon={faChevronUp} /></button
				>
			{:else}
				<button
					class="btn-sm text-right"
					title="Open or close {convertDisplayName(label, true)}"
					on:click={handleToggleShow}><Fa icon={faChevronDown} /></button
				>
			{/if}
		{/if}

		<!-- <Fa icon={faPlus} class="text-green-500" />

      <input class="checkbox" type="checkbox" bind:checked={active} on:change={()=>changeFn(active)}/> -->
	</div>
	<button class="text-left grow" on:click={handleToggleShow} type="button">
		<h4 id={path} class="text-md font-bold">
			{convertDisplayName(label, true)}
			{#if required}
				<span class="text-red-500">*</span>
			{/if}
			<!--{#if description}
				<button class="badge h-full mt-1" on:click|stopPropagation={()=>showDescription = !showDescription} title="Show Description"><Fa icon={faCircleQuestionRegular} size="lg"/></button>
		{/if}-->
		</h4>
	</button>

	<div class="text-left flex justify-end w-2">
		{#if active && !required}
			<button
				class="badge mt-1"
				on:click={() => changeFn(active)}
				title="Remove {convertDisplayName(label, true)} node. Content will be lost."
				><Fa icon={faTrash} /></button
			>
		{/if}
	</div>
	<div class="text-left flex justify-end w-2 px-2"></div>
</div>
{#if description && showDescription}
	<div class="text-sm text-gray-500 py-1 pl-2">{@html description}</div>
{/if}
