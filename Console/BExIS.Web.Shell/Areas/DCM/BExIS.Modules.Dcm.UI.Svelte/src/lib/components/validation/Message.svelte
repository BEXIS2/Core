<script>
	import { Accordion, AccordionItem } from '@skeletonlabs/skeleton';

	export let title;
	export let count;
	export let messages;
	export let type;
</script>

<Accordion>
	<AccordionItem>
		<svelte:fragment slot="summary"><div class="font-bold justify-self-end">{count}&nbsp;{type === 'error' ? 'Errors' : 'Warnings'}</div></svelte:fragment>
		<svelte:fragment slot="lead"><div class="break-words">{title}</div></svelte:fragment>
		<svelte:fragment slot="content">
			<ol class="list-disc px-5 break-words w-11/12">
				{#each messages as m}
					<li class="break-words">{m.replace(title + ":", '')}</li>
				{/each}
			</ol>
		</svelte:fragment>
	</AccordionItem>
</Accordion>
