<script lang="ts">
	import { FileInfo, type fileInfoType } from '@bexis2/bexis2-core-ui';

	// for files

	export let id = 0;
	export let files: fileInfoType[] = [];
	$: files;

	export let descriptionType: number = 0;
	$: descriptionType;

</script>

{#if files.length > 0}
	<div>
			<div class="flex flex-col">
			{#each files as file, index }
				<div class="flex gap-3	items-center border-b border-surface-200 py-2">
				
					<div class="self-center flex-none w-6"><FileInfo type={file.type} size="x-large" /></div>
					{index	+ 1}.
					{file.name}
					{file.type}
					{file.lenght}
					{file.description}
				</div>
			{/each}
		</div>
	</div>
{/if}
