<script lang="ts">
	import type { PopupSettings } from '@skeletonlabs/skeleton';
	import { popup } from '@skeletonlabs/skeleton';

	import Fa from 'svelte-fa/src/fa.svelte';
	import { faQuestion } from '@fortawesome/free-solid-svg-icons/index';

	export let title: string;
	export let description;

	const popupFeatured: PopupSettings = {
		// Represents the type of event that opens/closed the popup
		event: 'hover',
		// Matches the data-popup value on your popup element
		target: title,
		// Defines which side of your trigger the popup will appear
		placement: 'top'
	};
</script>

<button type="button" class="btn-icon variant-ringed-warning" use:popup={popupFeatured}
	><Fa icon={faQuestion} />
</button>

<div class="card p-4 w-72 shadow-xl bg-surface-300 z-50 text-left space-y-2" data-popup={title}>
	<h3 class="h3">{title}</h3>
	<p class="p">{@html description}</p>
</div>
