<script lang="ts">
	import { onDestroy, onMount } from 'svelte';

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
	import { curationStore } from './stores';
	import Fa from 'svelte-fa';
	import { faCopy, faTimes } from '@fortawesome/free-solid-svg-icons';
	import CurationEntryInputs from './CurationEntryInputs.svelte';
	import { DefaultCurationEntryCreationModel, type CurationEntryCreationModel } from './types';
	import { getTemplateLinkText } from './CurationEntryTemplate';

	const popupState = curationStore.entryTemplatePopupState;

	const close = () => {
		popupState.set({ show: false });
	};

	function handleKeydown(event: KeyboardEvent) {
		if (event.key === 'Escape') {
			close();
		}
	}

	onMount(() => {
		window.addEventListener('keydown', handleKeydown);
	});

	$: {
		if ($popupState.show) {
			document.body.style.overflow = 'hidden';
			window.addEventListener('keydown', handleKeydown);
		} else {
			document.body.style.overflow = '';
			window.removeEventListener('keydown', handleKeydown);
		}
	}

	onDestroy(() => {
		document.body.style.overflow = '';
		window.removeEventListener('keydown', handleKeydown);
	});

	let inputData: CurationEntryCreationModel = { ...DefaultCurationEntryCreationModel };

	popupState.subscribe(($popupState) => {
		if ($popupState.inputData) {
			inputData = { ...DefaultCurationEntryCreationModel, ...$popupState.inputData };
		} else {
			inputData = { ...DefaultCurationEntryCreationModel };
		}
	});

	let position: 'top' | 'bottom' = 'bottom';
	let createAsDraft: boolean = true;
	let autoCreate: boolean = false;

	$: templateLink = getTemplateLinkText(inputData, position, createAsDraft, autoCreate);
</script>

{#if $popupState.show}
	<div
		class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-40"
		role="dialog"
		aria-modal="true"
	>
		<button
			type="button"
			tabindex="-1"
			aria-label="Close popup"
			class="fixed left-0 top-0 z-10 h-screen w-screen cursor-default opacity-0"
			on:click={close}
		></button>
		<div
			class="animate-popup-in z-10 flex max-h-[90vh] w-[700px] max-w-[90vw] flex-col overflow-hidden rounded-xl bg-white shadow-2xl"
			role="document"
		>
			<header
				class="flex items-center justify-between border-b border-gray-200 bg-gray-50 px-6 pb-4 pt-6"
			>
				<h2 class="m-0 text-lg font-semibold">Edit Curation Entry Template</h2>
				<button class="btn-ghost btn" aria-label="Close" on:click={close}>
					<Fa icon={faTimes} />
				</button>
			</header>
			<form class="flex flex-wrap gap-x-2 gap-y-1 overflow-y-auto p-6">
				<CurationEntryInputs bind:inputData isDraft positionHidden />
				<div class="flex w-full justify-between gap-2 border-t border-surface-300 pt-2">
					<label>
						<span>Insert New Entry:</span>
						<select bind:value={position} class="input">
							<option value="top">Top</option>
							<option value="bottom">Bottom</option>
						</select>
					</label>
					<div class="flex h-full flex-col justify-center gap-2">
						<label class="has-[:disabled]:opacity-50">
							<input type="checkbox" bind:checked={createAsDraft} class="input size-5" />
							<span>Create as Draft</span>
						</label>
						<label>
							<input type="checkbox" bind:checked={autoCreate} class="input size-5" />
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
						{getTemplateLinkText(inputData, position, createAsDraft, autoCreate)}
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
			</form>
		</div>
	</div>
{/if}

<style>
	@keyframes popup-in {
		from {
			transform: translateY(40px) scale(0.98);
			opacity: 0;
		}
		to {
			transform: translateY(0) scale(1);
			opacity: 1;
		}
	}

	.animate-popup-in {
		animation: popup-in 0.18s cubic-bezier(0.4, 0, 0.2, 1);
	}

	.copy-button {
		word-break: break-all;
		overflow-wrap: anywhere;
		white-space: normal;
	}
</style>
