<script lang="ts">
	import { popup, type PopupSettings } from '@skeletonlabs/skeleton';
	import { helpType } from './types';
	import Fa from 'svelte-fa';
	import { faCircleQuestion } from '@fortawesome/free-solid-svg-icons';

	export let popupId: string;
	export let label: string | undefined = 'Help';
	export let type: helpType = helpType.empty;

	const popupFeatured: PopupSettings = {
		event: 'click',
		target: popupId,
		placement: 'bottom-end'
	};

	const helpTypeNames = [
		'nothing', // empty = 0,
		'users', // mainResearcher = 1,
		'curators', // mainCurator = 2,
		'tasks' // tasks = 3,
	];
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
		{#if type === helpType.empty}
			<!-- Content for empty help (should not be used in production) -->
			Nothing
		{:else if type === helpType.mainResearcher}
			<!-- Content for main help for researcher -->
			Main researcher help content goes here.
		{:else if type === helpType.mainCurator}
			<!-- Content for main help for Curator -->
			Main curator help content goes here.
		{:else if type === helpType.tasks}
			<!-- Content for help for task list -->
			Tasks help content goes here.
		{/if}
	</div>
</div>
