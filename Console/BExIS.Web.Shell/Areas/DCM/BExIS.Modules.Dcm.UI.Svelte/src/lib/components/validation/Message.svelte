<script>
	import { Accordion, AccordionItem } from '@skeletonlabs/skeleton';

	export let title;
	export let count;
	export let messages;
</script>

<Accordion>
	<AccordionItem>
		<svelte:fragment slot="summary">({count})</svelte:fragment>
		<svelte:fragment slot="lead"><div class="break-words w-11/12">{title}</div></svelte:fragment>
		<svelte:fragment slot="content">
			<ol class="list-disc px-5 break-words w-11/12">
				{#each messages as m}
					<li class="break-words w-11/12">{m}</li>
				{/each}
			</ol>
		</svelte:fragment>
	</AccordionItem>
</Accordion>
