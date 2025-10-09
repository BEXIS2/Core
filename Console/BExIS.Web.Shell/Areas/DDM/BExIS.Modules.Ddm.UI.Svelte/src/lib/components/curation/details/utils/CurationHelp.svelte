<script lang="ts">
	import { popup, type PopupSettings } from '@skeletonlabs/skeleton';
	import Fa, { FaLayers } from 'svelte-fa';
	import {
		faA,
		faBroom,
		faCirclePlus,
		faCircleQuestion,
		faCopy,
		faDoorOpen,
		faEdit,
		faFile,
		faListCheck,
		faMessage,
		faPen,
		faSquare
	} from '@fortawesome/free-solid-svg-icons';
	import { curationHelpType, helpTypeNames } from '$lib/models/CurationHelp';
	import { curationStore } from '$lib/stores/CurationStore';
	import { derived } from 'svelte/store';
	import { CurationEntryStatus, CurationEntryStatusDetails } from '$lib/models/CurationEntry';

	export let popupId: string;
	export let label: string | undefined = 'Help';
	export let type: curationHelpType = curationHelpType.empty;
	type Direction = 'top' | 'bottom' | 'left' | 'right';
	type Placement = Direction | `${Direction}-start` | `${Direction}-end`;
	export let popupPlacement: Placement = 'bottom-end';

	let popupOpen = false;

	const popupFeatured: PopupSettings = {
		event: 'click',
		target: popupId,
		placement: popupPlacement,
		closeQuery: 'a[href]:not(.internal-scroll)',
		state: (event) => (popupOpen = event.state)
	};

	const statusColors = derived(
		curationStore.statusColorPalette,
		($statusColorPalette) => $statusColorPalette.colors
	);

	const statusIcons = CurationEntryStatusDetails.map((status) => status.icon);

	let popupScrollElement: HTMLElement | null = null;
	// Scroll inside
	function scrollToId(id: string) {
		const element = document.getElementById(id);
		if (element && popupScrollElement) {
			popupScrollElement.scrollTo({
				top: element.offsetTop - popupScrollElement.offsetTop,
				behavior: 'smooth'
			});
		}
	}

	const mainResearcherStructure = [
		{ id: 'help-curation-entries', name: 'Curation Entries' },
		{ id: 'help-filters', name: 'Filters and Search' },
		{ id: 'help-color-palette', name: 'Color Palette' }
	];

	const mainCuratorStructure = [
		{ id: 'help-greeting', name: 'Greeting' },
		{ id: 'help-tasks', name: 'Curation Tasks' },
		{ id: 'help-curation-entries', name: 'Curation Entries' },
		{ id: 'help-template', name: 'Templates' },
		{ id: 'help-filters', name: 'Filters and Search' },
		{ id: 'help-color-palette', name: 'Color Palette' }
	];
</script>

<button
	class:variant-filled-primary={!popupOpen}
	class:variant-outline-primary={popupOpen}
	class:text-primary-500={popupOpen}
	class="btn w-fit grow-0 px-4 py-0.5"
	type="button"
	use:popup={popupFeatured}
	id="{popupId}-button"
	title="Show help for {helpTypeNames[type]}"
>
	{#if label && label.trim().length > 0}
		<span>{label}</span>
	{/if}
	<Fa icon={faCircleQuestion} />
</button>

<div
	data-popup={popupId}
	id={popupId}
	class="card z-50 -mt-1 border border-primary-500 text-sm font-normal shadow-lg"
>
	<div class="grid max-h-96 max-w-prose gap-2 overflow-y-auto p-3" bind:this={popupScrollElement}>
		{#if type === curationHelpType.mainResearcher}
			<!-- Researcher Introduction -->
			<h1 class="text-base font-semibold">Curation Help</h1>
			<p>
				Welcome to the curation details section for your dataset. Here, you can review important
				information and collaborate directly with your curator. Explore the available sections to
				understand the interface and its features.
			</p>
			<p>
				Your curator may have provided a summary or guidance below the dataset title to help you
				proceed effectively.
			</p>

			<h2 class="mt-2 text-base font-semibold" id="help-structure">Structure of this help</h2>
			<p class="flex flex-wrap gap-x-3 gap-y-1">
				{#each mainResearcherStructure as { id, name }}
					<a
						href={'#' + id}
						class="internal-scroll text-primary-500 underline"
						on:click|preventDefault={() => scrollToId(id)}
					>
						{name}
					</a>
				{/each}
			</p>

			<hr />
		{:else if type === curationHelpType.mainCurator}
			<!-- Curator Introduction -->
			<h1 class="text-base font-semibold">Curation Help</h1>
			<p>
				Welcome to the curation details section for this dataset. Here, you can manage and track the
				curation process, communicate with the researcher, and ensure the quality of the dataset.
				Explore the available sections to understand the interface and its features. <br />
				You can provide a
				<span class="font-semibold">
					<Fa icon={faDoorOpen} class="inline-block" />
					Greeting
				</span> for the researcher at the top of the page to welcome them and offer first guidance for
				a good start. (see below)
			</p>

			<h2 class="mt-2 text-base font-semibold" id="help-structure">Structure of this help</h2>
			<p class="flex flex-wrap gap-x-3 gap-y-1">
				{#each mainCuratorStructure as { id, name }}
					<a
						href={'#' + id}
						class="internal-scroll text-primary-500 underline"
						on:click|preventDefault={() => scrollToId(id)}
					>
						{name}
					</a>
				{/each}
			</p>

			<hr />

			<!-- Greeting -- Curator -->
			<h2 class="mt-2 text-base font-semibold" id="help-greeting">Greeting</h2>
			<p>
				You can provide a <span class="font-semibold">
					<Fa icon={faDoorOpen} class="inline-block" />
					Greeting
				</span>
				or summary for the researcher at the top of the curation details page. This can help to make
				the curation more personal and should offer guidance or context for the curation process. You
				can also use
				<span class="rounded bg-surface-300 px-1 font-mono">*Open*</span>,
				<span class="rounded bg-surface-300 px-1 font-mono">*Changed*</span>,
				<span class="rounded bg-surface-300 px-1 font-mono">*Approved*</span>,
				<span class="rounded bg-surface-300 px-1 font-mono">*Paused*</span>, and
				<span class="rounded bg-surface-300 px-1 font-mono">*Chat*</span>
				inside your greeting to better explain the task for the researcher. These keywords will be replaced
				with the respective colored status and function indicators. Additionally, you can use
				<span class="rounded bg-surface-300 px-1 font-mono">*Some Text*</span>
				to make text italic or
				<span class="rounded bg-surface-300 px-1 font-mono">**Some Text**</span>
				to make text bold.
			</p>
		{/if}

		{#if type === curationHelpType.tasks || type === curationHelpType.mainCurator}
			<!-- Tasks -->
			<h2 class="mt-2 text-base font-semibold" id="help-tasks">Curation Tasks</h2>
			<p>
				The <span class="font-semibold">
					<Fa icon={faListCheck} class="inline-block" />
					Curation Tasks
				</span>
				section should be filled with prewritten templates that guide you through the curation process.
				Many markdown features are supported to help you format the tasks clearly. Especially useful
				are unordered lists with checkboxes to mark completed tasks:<br />
				<span class="rounded bg-surface-300 px-1 font-mono">- [ ] Task 1</span><br />
				Many more markdown features are supported but will not be explained here. You can find many guides
				online that explain markdown formatting.
			</p>
			<p>
				Inside the curation tasks, you can also create templates for curation entries by using
				<span class="rounded bg-surface-300 px-1 font-mono">[TemplateLink](...)</span>. However,
				these template links should not be created manually but rather with the provided template
				tool that is explained below. This ensures that the links are correctly formatted and
				functional. By clicking on the
				<span class="text-success-400">
					<Fa icon={faCirclePlus} class="inline-block" />
					Create
				</span>
				button that replaces the template link when the markdown is rendered, you can create a new curation
				entry based on the template.
			</p>
			<p>
				Clicking the
				<Fa icon={faEdit} class="inline-block text-secondary-400" />
				edit icon next to the create button, you can easily edit the template, which updates the markdown
				text accordingly.
			</p>
			<p>
				If AutoCreate is enabled in a template link, a
				<FaLayers>
					<Fa icon={faSquare} class="text-primary-500" />
					<Fa icon={faA} class="text-primary-50" scale="0.7" />
				</FaLayers>
				icon will be shown next to the create button. This means that a curation entry will be created
				once the
				<span class="text-primary-500">
					<FaLayers>
						<Fa icon={faSquare} class="text-primary-500" />
						<Fa icon={faA} class="text-primary-50" scale="0.7" />
					</FaLayers>
					Auto Create Templates (n)
				</span>
				button below the tasks is clicked. This allows you to create multiple entries quickly without
				having to click each create button individually. The
				<span class="text-primary-500">Create as Draft</span>
				checkbox next to the auto create button allows you to decide whether these entries should be
				created as drafts, which requires additional clicks by you to push them to the server, or directly
				created as open entries.
			</p>
		{/if}

		<!-- Curation Entries -->
		{#if type === curationHelpType.mainResearcher}
			<!-- Curation Entries -- Researcher -->
			<h2 class="mt-2 text-base font-semibold" id="help-curation-entries">
				Curation Entries / Issues
			</h2>
			<p>
				The most important part of the curation tool are the curation entries that mark issues,
				tasks or just general notes about the dataset. These entries are listed below and can be
				filtered and sorted. Each entry has a status that indicates its current state:
			</p>
			<ul class="ml-2 grid gap-2">
				<li>
					<strong style="color: {$statusColors[CurationEntryStatus.Open]}">
						<Fa icon={statusIcons[CurationEntryStatus.Open]} class="inline-block" />
						Open
					</strong>: The entry has been created and is awaiting action. You should review the issue
					and take the necessary steps to address it. If you have questions, you can use the chat
					feature to communicate with your curator.
				</li>
				<li>
					<strong style="color: {$statusColors[CurationEntryStatus.Fixed]}">
						<Fa icon={statusIcons[CurationEntryStatus.Fixed]} class="inline-block" />
						Changed
					</strong>: You should mark the entry as changed when you have made the necessary updates
					or modifications to the dataset in response to the issue raised. This status indicates
					that you have taken action on the entry, but it still requires review by the curator.
				</li>
				<li>
					<strong style="color: {$statusColors[CurationEntryStatus.Closed]}">
						<Fa icon={statusIcons[CurationEntryStatus.Closed]} class="inline-block" />
						Approved
					</strong>: The entry has been resolved and approved by the curator. No further action is
					required on this entry. In some cases the curator may create approved entries to show you
					that they have reviewed certain aspects of the dataset and found them to be satisfactory.
					If you do any changes to an entry that is approved, please change the status back to
					changed.
				</li>
				<li>
					<strong style="color: {$statusColors[CurationEntryStatus.Ok]}">
						<Fa icon={statusIcons[CurationEntryStatus.Ok]} class="inline-block" />
						Paused
					</strong>: The entry is temporarily on hold. This status may be used when waiting for
					additional information or resources before proceeding. If you do any changes to an entry
					that is paused, please change the status back to changed.
				</li>
			</ul>

			<p>
				You can click on the status of an entry (below the title and description) to change it. Next
				to the status, you can also open the
				<span>
					<Fa icon={faMessage} class="inline-block" />
					Chat
				</span>
				for this entry to discuss it with your curator.
			</p>
		{:else if type === curationHelpType.mainCurator}
			<!-- Curation Entries -- Curator -->
			<h2 class="mt-2 text-base font-semibold" id="help-curation-entries">
				Curation Entries / Issues
			</h2>
			<p>
				Curation entries are used to document issues, tasks, or notes about the dataset. As a
				curator, you can create new entries, update their status, and communicate with the
				researcher. Each entry has a status that reflects its current state:
			</p>
			<ul class="ml-2 grid gap-2">
				<li>
					<strong style="color: {$statusColors[CurationEntryStatus.Open]}">
						<Fa icon={statusIcons[CurationEntryStatus.Open]} class="inline-block" />
						Open
					</strong>: The entry has been created and is awaiting action from the researcher. Use this
					status to highlight issues or tasks that need attention.
				</li>
				<li>
					<strong style="color: {$statusColors[CurationEntryStatus.Fixed]}">
						<Fa icon={statusIcons[CurationEntryStatus.Fixed]} class="inline-block" />
						Changed
					</strong>: The researcher has made updates or modifications in response to your entry.
					Review these changes and decide if further action is needed.
				</li>
				<li>
					<strong style="color: {$statusColors[CurationEntryStatus.Closed]}">
						<Fa icon={statusIcons[CurationEntryStatus.Closed]} class="inline-block" />
						Approved
					</strong>: The entry has been resolved and approved by you. Use this status to indicate
					that the issue has been satisfactorily addressed.
				</li>
				<li>
					<strong style="color: {$statusColors[CurationEntryStatus.Ok]}">
						<Fa icon={statusIcons[CurationEntryStatus.Ok]} class="inline-block" />
						Paused
					</strong>: The entry is temporarily on hold, for example, while waiting for additional
					information or resources. You can resume the entry by changing its status when ready.
				</li>
			</ul>

			<p>
				You can change the status of an entry by clicking on it. Use the chat feature next to each
				entry to communicate directly with the researcher and clarify any questions or provide
				additional guidance. To edit or create entries you have to click on
				<span class="text-secondary-500">
					<Fa icon={faPen} class="inline-block" />
					Switch to Edit Mode
				</span> button at the top right of the entries list.
			</p>
			<p>
				In edit mode, you can modify existing entries or create new ones. Entries that are marked as
				<span class="text-primary-500">
					<Fa icon={faFile} class="inline-block" />
					Draft
				</span>
				are not pushed to the server until you edit them again and save them. This also means reloading
				the page will discard any unsaved changes. Therefore, you should ensure to not keep too many
				draft entries at once and push them to the server as soon as possible. Also note that only drafts
				can be deleted. After a draft is pushed to the server, it becomes a regular entry that can only
				be edited but not deleted. If you still need to hide an entry after it has been pushed to the
				server, you can change its category / type to "None (Hidden)". This will move it to the bottom
				of the list and hide it from the researcher. You wil only see hidden entries if you enable the
				Edit Mode.
			</p>
			<p>When editing or creating an entry, you can set the following parameters:</p>
			<ul class="ml-2 grid gap-2">
				<li>
					<strong>Category</strong>: The category or type of the entry. This helps to organize and
					filter entries. If you want to hide an entry from the researcher, set its category to
					"None (Hidden)". You will only see hidden entries if you enable the Edit Mode.
				</li>
				<li>
					<strong>Title</strong>: A brief summary of the issue or task or the source of the issue.
					This should be concise yet informative to give a quick overview of the entry. If multiple
					entries have the same title and are adjacent, they will be grouped together in the list to
					save space.
				</li>
				<li>
					<strong>Position</strong>: The position of the entry in the list. You can pick any number
					and also resort it later.
				</li>
				<li>
					<strong>Description</strong>: A detailed explanation of the entry. This should provide
					enough context and information for the researcher to understand what is required.
				</li>
				<li>
					<strong>Initial Comment (Draft only)</strong>: This is a preliminary comment or note that
					you can add to the entry while it is still in draft mode. This may be used to provide
					additional context or instructions for the researcher that is too long for the description
					field. Once the entry is pushed to the server, this comment becomes a regular comment.
				</li>
				<li>
					<strong>Initial Status</strong>: The initial status of the entry when it is created. You
					can choose from Open, Changed, Approved, or Paused. The default status is Open.
				</li>
			</ul>
			<p>
				If you want to create a template from an existing entry, you can do so by first editing the
				entry and then selecting the
				<span>
					<Fa icon={faCopy} class="inline-block" />
					Create Template
				</span>
				option. This creates opens the template tool with the exact state of your current input fields,
				which may be different from the original entry, if you have edited it. This allows you to quickly
				create templates based on existing entries that you have already customized. You can read more
				about <strong>Templates</strong> below.
			</p>
		{/if}

		{#if type === curationHelpType.mainCurator}
			<h2 class="mt-2 text-base font-semibold" id="help-template">Templates</h2>
			<p>
				Templates are a powerful feature that allows you to create predefined structures for
				curation entries. They can help you standardize the curation process and ensure that
				important aspects are consistently addressed. Templates are created using a special markdown
				syntax within the curation tasks section. A template link looks like this:
				<span class="break-all rounded bg-surface-300 px-1 font-mono">
					[TemplateEntry](?type=2&name=Example%20Title&description=This%20is%20an%20example%20description.&draft)
				</span>
			</p>
			<p>
				To create a new template link, you can click on the
				<span>
					<Fa icon={faCopy} class="inline-block" />
					Create Template
				</span>
				button when editing a curation entry. This opens the template tool with the current state of
				your entry. Afterwards you can adjust the parameters to fit your needs and copy the template
				link at the bottom, by clicking on it. This link can then be pasted into the curation tasks markdown.
				You can create as many template links as you want. Each link will be replaced with a
				<span class="text-success-400">
					<Fa icon={faCirclePlus} class="inline-block" />
					Create
				</span>
				button in the rendered markdown. Clicking this button creates a new curation entry based on the
				template.
			</p>
			<p>
				Alternatively, you can copy an existing markdown template link inside the curation tasks and
				paste it at another position to duplicate it. You should then save the tasks and edit the
				newly created template link by clicking on the
				<Fa icon={faEdit} class="inline-block text-secondary-400" />
				edit icon next to the create button that replaces the template link in the rendered markdown.
				This opens the template tool with the current parameters of the template link, which you can
				then adjust as needed.
			</p>
			<p>
				Inside the template tool, which opens as a popup, you can set the following parameters,
				which are all optional:
			</p>
			<ul class="ml-2 grid gap-2">
				<li>
					<strong>Category, Title, Description, Initial Comment, Initial Status</strong>: See
					Curation Entries above.
				</li>
				<li>
					<strong>Placement</strong>: The placement of the entry when it is created. You can choose
					from "Top" to insert the new entry at the top of the entries list or "Bottom" to insert it
					at the bottom of the entries list.
				</li>
				<li>
					<strong>Create as Draft</strong>: If enabled, the created entry will be marked as draft.
					This means that it will not be pushed to the server until you edit it again and save it.
				</li>
				<li>
					<strong>Auto Create</strong>: If enabled, a curation entry will be created automatically
					when the
					<span class="text-primary-500">
						<FaLayers>
							<Fa icon={faSquare} class="text-primary-500" />
							<Fa icon={faA} class="text-primary-50" scale="0.7" />
						</FaLayers>
						Auto Create Templates (n)
					</span>
					button below the tasks is clicked. This allows you to create multiple entries quickly without
					having to click each create button individually. Ideally you should only use this for a few
					templates that are very general.
				</li>
			</ul>
		{/if}

		{#if type === curationHelpType.mainResearcher || type === curationHelpType.mainCurator}
			<!-- Filters and Search -->
			<h2 class="mt-2 text-base font-semibold" id="help-filters">Filters and Search</h2>
			<p>
				To help you manage and navigate through the curation entries, you can use the filters and
				search functionality at the top of the entries list. You can filter entries by their status
				and their category. The status filter allows multiple selections, that are combined with an
				OR logic. The category filter allows only a single selection. Additionally, you can use the
				search bar to find specific entries based on keywords in their title, description, or
				comments.
			</p>
			<p>
				All filters can be cleared at once by clicking the
				<span class="italic text-tertiary-700">
					<Fa icon={faBroom} class="inline-block" />
					Clear Applied Filters
				</span> button below the filters.
			</p>

			<h2 class="mt-2 text-base font-semibold" id="help-color-palette">Color Palette</h2>
			<p>
				If you find it difficult to distinguish between the different status colors, try one of the
				other color palettes. You can change the color palette by using the dropdown above the
				filters and the progress bar.
			</p>
		{/if}
		{#if type !== curationHelpType.mainResearcher && type !== curationHelpType.mainCurator && type !== curationHelpType.tasks}
			<!-- Not Implemented -->
			This help topic is not yet implemented.
		{/if}
	</div>
</div>
