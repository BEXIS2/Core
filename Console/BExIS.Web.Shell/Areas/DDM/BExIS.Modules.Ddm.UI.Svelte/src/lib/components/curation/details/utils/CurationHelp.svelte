<script lang="ts">
	import { popup, type PopupSettings } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import { faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
	import { curationHelpType, helpTypeNames } from '$lib/models/CurationHelp';

	export let popupId: string;
	export let label: string | undefined = 'Help';
	export let type: curationHelpType = curationHelpType.empty;

	const popupFeatured: PopupSettings = {
		event: 'click',
		target: popupId,
		placement: 'bottom-end'
	};
</script>

<button
	class:variant-filled-primary={true}
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

<div data-popup={popupId} id={popupId} class="z-50 font-normal">
	<div class="bg-base-100 card grid max-w-64 gap-2 p-3 shadow-lg">
		{#if type === curationHelpType.empty}
			<!-- Content for empty help (should not be used in production) -->
			Nothing
		{:else if type === curationHelpType.mainResearcher}
			<!-- Content for main help for researcher -->
			Main researcher help content goes here.
		{:else if type === curationHelpType.mainCurator}
			<!-- Content for main help for Curator -->
			Main curator help content goes here.
		{:else if type === curationHelpType.tasks}
			<!-- Content for help for task list -->
			Tasks help content goes here.
		{/if}
	</div>
</div>
