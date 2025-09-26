<script lang="ts">
	import Fa from 'svelte-fa';
	import { faCopy, faFloppyDisk, faTimes } from '@fortawesome/free-solid-svg-icons';
	import CurationEntryInputs from './CurationEntryInputs.svelte';
	import {
		DefaultCurationEntryTemplate,
		entryTemplatePopupState,
		getTemplateLinkText,
		type CurationEntryTemplateModel
	} from '$lib/models/CurationEntryTemplate';

	const close = () => {
		entryTemplatePopupState.set({ show: false });
	};

	let template = { ...DefaultCurationEntryTemplate };

	entryTemplatePopupState.subscribe(($entryTemplatePopupState) => {
		if ($entryTemplatePopupState.template) {
			template = { ...DefaultCurationEntryTemplate, ...$entryTemplatePopupState.template };
		} else {
			template = { ...DefaultCurationEntryTemplate };
		}
	});

	let callback: ((template: CurationEntryTemplateModel) => void) | undefined;

	entryTemplatePopupState.subscribe(($popupState) => {
		callback = $popupState.callback;
	});

	$: templateLink = getTemplateLinkText(template);

	// Tooltip state for copy
	let copied = false;
	let copiedTimeout: ReturnType<typeof setTimeout>;

	function handleCopy() {
		navigator.clipboard.writeText(templateLink);
		copied = true;
		clearTimeout(copiedTimeout);
		copiedTimeout = setTimeout(() => {
			copied = false;
		}, 1500);
	}
</script>

<form class="flex flex-wrap gap-x-2 gap-y-1 overflow-y-auto p-6">
	<CurationEntryInputs bind:inputData={template} isDraft positionHidden />
	<div class="flex w-full justify-between gap-2 border-t border-surface-300 pt-2">
		<label>
			<span>Insert New Entry:</span>
			<select bind:value={template.placement} class="input">
				<option value="top">Top</option>
				<option value="bottom">Bottom</option>
			</select>
		</label>
		<div class="flex h-full flex-col justify-center gap-2">
			<label class="has-[:disabled]:opacity-50">
				<input type="checkbox" bind:checked={template.createAsDraft} class="input size-5" />
				<span>Create as Draft</span>
			</label>
			<label>
				<input type="checkbox" bind:checked={template.autoCreate} class="input size-5" />
				<span>Auto Create</span>
			</label>
		</div>
	</div>
	<!-- copy to clipboard button -->
	<label class="mt-2 w-full">
		<span>Template Link:</span>
		<button
			class="copy-button variant-ghost-surface btn relative w-full overflow-hidden pr-8 text-left text-sm ring-0"
			type="button"
			on:click={handleCopy}
			title="Copy to clipboard"
		>
			{templateLink}
			<Fa icon={faCopy} class="absolute right-2 top-1/2 -translate-y-1/2 text-lg" />
			<span
				class="pointer-events-none absolute flex size-full items-center justify-center bg-surface-400 bg-opacity-90 font-semibold transition-opacity duration-300"
				class:opacity-100={copied}
				class:opacity-0={!copied}
			>
				Copied to clipboard
			</span>
		</button>
	</label>
	{#if callback}
		<!-- If a callback is provided, use it -->
		<div class="mt-2 flex w-full justify-between gap-2">
			<button
				type="button"
				on:click|preventDefault={close}
				title="Cancel"
				class="variant-ghost-surface btn grow text-nowrap px-2 py-1 text-surface-800"
			>
				<Fa icon={faTimes} class="mr-1 inline-block" />
				Cancel
			</button>
			<button
				type="button"
				on:click|preventDefault={() => {
					if (callback) {
						callback(template);
					}
					close();
				}}
				title="Save and close"
				class="variant-filled-success btn grow text-nowrap px-2 py-1"
			>
				<Fa icon={faFloppyDisk} class="mr-1 inline-block" />
				Save
			</button>
		</div>
	{/if}
</form>

<style>
	.copy-button {
		word-break: break-all;
		overflow-wrap: anywhere;
		white-space: normal;
	}
</style>
