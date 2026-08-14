<script lang="ts">
	import Fa from 'svelte-fa';
	import {
		faCheck,
		faEye,
		faFileUpload,
		faSave,
		faTriangleExclamation,
    faEyeSlash
	} from '@fortawesome/free-solid-svg-icons';
 
	import { onMount } from 'svelte';
	import * as apiCalls from '../../../services/MetadataCaller';
	import {
		activateShow,
		getValidationStore,
		setMetadataStore,
		toggleShow
	} from '$lib/components/utils/metadata/metadataComponentUtils';
	import type { validationStoretype } from '$lib/components/utils/metadata/models';
	import { metadataStore, validationStore } from '$lib/components/utils/metadata/stores';

	import {
		Api,
		FileUploader,
		notificationStore,
		notificationType,
		TextInput,
		type fileUploaderType
	} from '@bexis2/bexis2-core-ui';
	import { convertDisplayName } from '../../../lib/components/utils/metadata/metadataShared';
	import { goTo } from '$services/BaseCaller';
	import { createEventDispatcher } from 'svelte';
	import suite from '$lib/components/utils/metadata/simpleComponentSuite';
	import { FileButton } from '@skeletonlabs/skeleton';

	const dispatch = createEventDispatcher();

	export let datasetId: number;

	export let metadata;
	export let saveWithError: boolean = false;
	let hasChanged: boolean = true; // need to implement change detection to enable/disable save button based on whether there are unsaved changes or not, for now it's always enabled
	let showErrorOverview: boolean = false;
	let comment: string = '';
	let fileUploadType: fileUploaderType = {
		accept: ['.json', '.xml'],
		existingFiles: [],
		descriptionType: 0,
		multiple: false,
		maxSize: 10 // 10MB
	};

	const unsubscribedMetadata = metadata;

	$: showErrorOverview = true;
	$: metadata; //console.log("functions - metadata:", metadata);

	let disbaleSaveBtn: boolean = false;
	$: disbaleSaveBtn;

	let vestResults: any = null;
	$vestResults;

	let validationStoreValues: validationStoretype;
	$: {
		validationStoreValues;
		disbaleSaveBtn = disableSaveFn();
		//console.log("🚀 ~ file: +page.svelte:92 ~ $: ~ disbaleSaveBtn:", disbaleSaveBtn)
		//console.log("🚀 ~ validationStoreValues ~ $: ~ validationStoreValues:", validationStoreValues)
	}

	onMount(() => {

		metadataStore.subscribe((s) => {
			metadata = s;
		});

		validationStore.subscribe((s) => {
			//console.log("🚀 ~ validationStore subscribe ~ s:", s)
			validationStoreValues = s;
		});
	});

	function hasErrors(key) {
		if (validationStoreValues) {
			const invalidParts = validationStoreValues.simpleTypeValidationItems.filter(
				(item) =>
					item.path.startsWith(key) &&
					item.isValid === false &&
					item.errorMessage &&
					item.errorMessage.trim() !== ''
			);
			return invalidParts && invalidParts.length > 0;
		}
	}

	function disableSaveFn(): boolean {
		//console.log("🚀 ~ disableSaveFn ~ hasChanged:", hasChanged, saveWithError)
		if (hasChanged == false) return true; // when there are changes, the save button is enabled, so return false for disabled
		if (saveWithError) return false; // when save with error is allowd, the save button is always enabled
		if (!validationStoreValues) return true; // if there is no validation result, we consider the form as not valid, so the save button is disabled

		return !validationStoreValues.allSimpleRequiredValid; //	disable save button when the metadata is not valid
	}

	function toggleAll(path: string) {
		const complexItem = path.split('.');
		//console.log("🚀 ~ toggleAll",path,complexItem)

		for (let i = complexItem.length; i > 0; i--) {
			const p = complexItem.slice(0, i).join('.');
			//console.log("🚀 ~ toggleAll ~ p:", p)
			activateShow(p);
		}

		setTimeout(() => {
			const ziel = document.getElementById(path + '.item');
			ziel?.scrollIntoView({ behavior: 'smooth' });
		}, 500);
	}

	// function successHandler(e) {
	// 	console.log('🚀 ~ successHandler ~ e:', e);

	// 	const status = e.detail.status;
	// 	if (status === 200) {
	// 		notificationStore.showNotification({
	// 			notificationType: notificationType.success,
	// 			message: 'Metadata successfully imported.'
	// 		});

	// 		//console.log("🚀 ~ successHandler ~ metadata:", metadata)
	// 		metadata = JSON.parse(String(e.detail.data));
	// 		//console.log("🚀 ~ successHandler ~ metadata:", metadata)
	// 		setMetadataStore(metadata);
	// 		dispatch('metadataUpdated');
	// 	}
	// }

	// let files: FileList;

	// async function fileUploadSelectionFn(e) {
	// 	console.log('🚀 ~ fileUploadSelectionFn ~ e:', e);
	// 	const file = e.target.files[0];
	// 	if (file) {
	// 		fileUploadType.existingFiles = [file.name];
	// 		console.log('🚀 ~ fileUploadSelectionFn ~ fileUploadType:', fileUploadType);

	// 		const formData = new FormData();
	// 		formData.append('id', datasetId.toString());
	// 		formData.append(file.name, file);

	// 		const res = await Api.post('/dcm/m/import', formData);

	// 		console.log('🚀 ~ fileUploadSelectionFn ~ res:', res);

	// 		if (res.status === 200) {
	// 			notificationStore.showNotification({
	// 				notificationType: notificationType.success,
	// 				message: 'Metadata successfully imported.'
	// 			});

	// 			//console.log("🚀 ~ successHandler ~ metadata:", metadata)
	// 			metadata = JSON.parse(String(res.data));
	// 			//console.log("🚀 ~ successHandler ~ metadata:", metadata)
	// 			setMetadataStore(metadata);
	// 			dispatch('metadataUpdated');
	// 		}
	// 	}
	// }
</script>


	<div id="metadata-options" class="flex-col w-full gap-4">
		<h2 class="h2 pb-4">Edit Metadata</h2>
		<div class="flex text-justify w-full pr-4 pb-2">
			Please fill out the form as completely as possible. The more information you provide, the better your dataset can be found and reused by others.
		</div>
	</div>

	<!-- Error messages-->
	<div class="flex flex-col gap-2 items-end w-full pr-5">
		{#if validationStoreValues}
			{#key validationStoreValues}
				{#if validationStoreValues.simpleTypeValidationItems.filter((item) => item.isValid === false && item.errorMessage && item.errorMessage.trim() !== '').length > 0}
					<button
						class="badge" title="There are validation errors in the metadata."
						on:click={() => (showErrorOverview = !showErrorOverview)}
					>
						{#if showErrorOverview}
              <Fa icon={faEyeSlash} />
            {:else}
              <Fa icon={faEye} />
            {/if}
            &nbsp;Warnings: {validationStoreValues.simpleTypeValidationItems.filter(
							(item) => item.isValid === false && item.errorMessage && item.errorMessage.trim() !== ''
						).length}
					</button>
				{/if}
			{/key}
		{/if}
	</div>

	<div>
		<hr />
		<nav class="list-nav">
			<ul class="list-disc space-y-2">
				{#each Object.entries(metadata) as [key, value]}
					{#if typeof value === 'object' && value !== null}
						<a href="#{key}" class="w-full" on:click={() => activateShow(key)}>
							<li class="flex items-center gap-1">
								<span class="h-1.5 w-1.5 rounded-full bg-gray-500 mr-2"></span>
								

								{#if validationStoreValues && hasErrors(key)}
									<span class="text-warning-600"><Fa icon={faTriangleExclamation} /></span>
								{:else if validationStoreValues && !hasErrors(key)}
									<!--       <span class="text-success-500"><Fa icon={faCheck} /></span>-->
								{/if}
                <span class="">{convertDisplayName(key)}</span>
							</li>
						</a>
						{#if validationStoreValues && showErrorOverview}
							{#key validationStoreValues}
								{#each validationStoreValues.simpleTypeValidationItems.filter((item) => item.isValid === false && item.errorMessage && item.errorMessage.trim() !== '') as item}
									{#if item.path.startsWith(key)}
										<div class="ml-4 flex flex-col">
											<button
												type="button"
												class="text-sm text-gray-500 text-left p-0 m-0 border border-gray-300 rounded-md hover:bg-gray-100"
												on:click={() => toggleAll(item.path)}
												aria-label={`Open ${item.path}`}
											>
												<div>
													{item.path
														.split('.')
														.slice(1)
														.map((segment) => {
															// Check if the segment is a non-empty string that represents an integer
															const isInteger = segment.trim() !== '' && !isNaN(Number(segment));
															const processedSegment = isInteger ? String(Number(segment) + 1) : segment;

															return convertDisplayName(processedSegment);
														})
														.join('/')}
													<br /><span class="text-xs italic bold pl-2">{item.errorMessage}</span>
												</div>
											</button>
										</div>
									{/if}
								{/each}
							{/key}
						{/if}
					{/if}
				{/each}
			</ul>
		</nav>
	</div>

