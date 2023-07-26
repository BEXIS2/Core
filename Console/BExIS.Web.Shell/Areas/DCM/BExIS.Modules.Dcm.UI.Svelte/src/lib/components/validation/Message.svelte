<script>
	import { Accordion, AccordionItem } from '@skeletonlabs/skeleton';

	export let title;
	export let count;
	export let messages;
</script>

<Accordion>
	<AccordionItem>
		<svelte:fragment slot="summary">({count})</svelte:fragment>
		<svelte:fragment slot="lead">{title}</svelte:fragment>
		<svelte:fragment slot="content">
			<ol class="list-disc px-5">
				{#each messages as m}
					<li>{m}</li>
				{/each}
			</ol>
		</svelte:fragment>
	</AccordionItem>
</Accordion>
