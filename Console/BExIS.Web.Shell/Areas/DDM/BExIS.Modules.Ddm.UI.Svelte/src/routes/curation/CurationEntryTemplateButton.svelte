<script lang="ts">
	import Fa from 'svelte-fa';
	import {
		faA,
		faArrowRightFromFile,
		faCirclePlus,
		faEdit,
		faFilePen,
		faMessage,
		faTurnDown,
		faTurnUp
	} from '@fortawesome/free-solid-svg-icons';
	import { curationStore } from './stores';
	import {
		DefaultCurationEntryTemplate,
		entryTemplatePopupState,
		getTemplateLinkText,
		parseTemplateLink,
		type CurationEntryTemplateModel
	} from './CurationEntryTemplate';
	import { get } from 'svelte/store';
	import { CurationEntryStatusDetails, DefaultCurationEntryCreationModel } from './types';
	import { createEventDispatcher } from 'svelte';

	export let markdown: string;

	const dispatch = createEventDispatcher();

	const { curation, statusColorPalette } = curationStore;

	$: template = {
		...DefaultCurationEntryTemplate,
		...parseTemplateLink(markdown)
	} as CurationEntryTemplateModel;

	const createEntryFromTemplate = () => {
		if (!markdown) return;
		const type = template.type ?? DefaultCurationEntryCreationModel.type;
		const highestPosition = get(curation)?.highestPositionPerType?.[type];
		const position =
			template.placement === 'top' ? 1 : highestPosition !== undefined ? highestPosition + 1 : 1;
		const entryModel = {
			...template,
			position
		};
		curationStore.addEmptyEntry(entryModel, false, template.createAsDraft ?? false, true);
	};

	const callback = (newEntryTemplate: CurationEntryTemplateModel) => {
		markdown = getTemplateLinkText(newEntryTemplate);
		dispatch('change', markdown);
	};

	const editTemplate = () => {
		entryTemplatePopupState.set({
			show: true,
			template: { ...template },
			callback
		});
	};

	// TODO implement positioning logic (show on bottom if close to top of viewport)
	const tooltipBottom = false;
</script>

<div class="relative inline-block w-24 leading-none">
	&#8203; <!-- Zero-width space to ensure the div has some height -->
	<div
		class="group absolute -bottom-1 -left-0.5 inline-flex w-full cursor-default gap-0 rounded hover:ring-1 hover:ring-primary-500 hover:ring-offset-1"
	>
		<!-- Tooltip -->
		<div
			class="template-tooltip pointer-events-none w-max rounded p-1 text-xs shadow-lg transition-opacity group-hover:pointer-events-auto group-hover:opacity-100"
			class:template-tooltip-bottom={tooltipBottom}
		>
			<div class="flex items-center gap-1 whitespace-nowrap">
				<span
					style="background-color: {$statusColorPalette.colors[template.status]};"
					class="overflow-hidden text-ellipsis whitespace-nowrap rounded px-1 py-0.5 text-white"
				>
					<Fa
						icon={CurationEntryStatusDetails[template.status].icon}
						class="inline-block text-xs"
					/>
					{CurationEntryStatusDetails[template.status].name}
				</span>
				<span class="flex">
					<span
						class="max-w-24 overflow-hidden text-ellipsis font-bold"
						class:italic={!template.name?.length}
					>
						{template.name?.length ? template.name : 'Untitled'}
					</span>
					:
				</span>
				<span
					class="max-w-48 overflow-hidden text-ellipsis"
					class:italic={!template.description?.length}
				>
					{template.description?.length ? template.description : 'Empty description'}
				</span>
				<span
					title={template.placement === 'top' ? 'Creates at top' : 'Creates at bottom'}
					class="rounded bg-success-400 px-[0.4rem] py-0.5 text-white"
				>
					<Fa
						icon={template.placement === 'top' ? faTurnUp : faTurnDown}
						class="-mr-0.5 inline-block"
					/>
				</span>
				{#if template.comment?.length}
					<span title="Has initial comment" class="rounded bg-surface-500 px-1 py-0.5 text-white">
						<Fa icon={faMessage} class="inline-block" />
					</span>
				{/if}
				<span
					title={template.createAsDraft ? 'Gets created as draft' : 'Gets created directly'}
					class="rounded px-1 py-0.5 text-white"
					class:bg-primary-500={template.createAsDraft}
					class:bg-warning-500={!template.createAsDraft}
				>
					<Fa
						icon={template.createAsDraft ? faFilePen : faArrowRightFromFile}
						class="-mr-0.5 inline-block"
					/>
				</span>
			</div>
		</div>
		<!-- Buttons -->
		<button
			class="btn m-0 shrink grow gap-1 overflow-hidden text-ellipsis whitespace-nowrap rounded-r-none px-1 py-0.5 text-sm text-success-600 text-opacity-70 hover:variant-soft-success hover:text-opacity-100"
			title="Create curation entry from task"
			on:click|preventDefault={createEntryFromTemplate}
		>
			<Fa icon={faCirclePlus} class="text-xs" />
			Create
		</button>
		{#if template.autoCreate}
			<div
				class="flex grow items-center justify-center bg-primary-500 bg-opacity-0 text-primary-500 transition-opacity group-hover:bg-opacity-50 group-hover:text-primary-50"
				title="This entry is automatically created, when you click on 'Create All'"
			>
				<Fa icon={faA} class="inline-block size-[0.7rem] px-0.5" />
			</div>
		{/if}
		<button
			class="btn m-0 shrink grow gap-1 overflow-hidden text-ellipsis whitespace-nowrap rounded-l-none px-1 py-0.5 text-sm text-secondary-600 text-opacity-70 hover:variant-soft-secondary hover:text-opacity-100"
			title="Edit task"
			on:click|preventDefault={editTemplate}
		>
			<Fa icon={faEdit} class="text-xs" />
		</button>
	</div>
</div>

<style>
	.template-tooltip::after {
		content: '';
		position: absolute;
		top: 100%;
		left: 50%;
		transform: translateX(-50%);
		width: 0;
		height: 0;
		border-left: 5px solid transparent;
		border-right: 5px solid transparent;
		border-top: 5px solid white;
		border-bottom: none;
	}

	.template-tooltip {
		z-index: 10;
		position: absolute;
		bottom: 100%;
		left: 50%;
		transform: translateX(-50%);
		background-color: white;
		opacity: 0;
	}

	.template-tooltip-bottom::after {
		top: auto;
		bottom: 100%;
		border-left: 5px solid transparent;
		border-right: 5px solid transparent;
		border-top: none;
		border-bottom: 5px solid white;
	}

	.template-tooltip-bottom {
		bottom: auto;
		top: 100%;
		transform: translateX(-50%);
	}
</style>
