<script lang="ts">
	import Fa from 'svelte-fa';
	import { faCheck, faAngleUp, faAngleDown } from '@fortawesome/free-solid-svg-icons';

	export let index: number;
	export let name: string;
	export let isKey: boolean;
	export let isOptional: boolean;
	export let isValid: boolean;
	export let expand: boolean;
	export let unit: string;
	export let datatype: string;
	export let template: string;
	export let description: string;
	export let datapreview: [];
</script>

<div class="card cursor-default">
	<header id="header_{index}" class="card-header pb-5">
		<div id={name} class="flex gap-5">
			<div class="flex gap-2 w-52">
				<div class="cursor-pointer" on:click={() => (expand = !expand)}>
					<!--		<div class="cursor-pointer"  on:click={() => expand = !expand}> -->
					{#if expand}<Fa icon={faAngleUp} />
					{:else}
						<Fa icon={faAngleDown} />
					{/if}
				</div>

				<h2 class="h2">{name}</h2>

				<div class="text-success-500">
					{#if isValid}<Fa icon={faCheck} />{/if}
				</div>
			</div>

			<div id="variable-{index}-description " class="flex-col text-left grow">
				<div class="text-xs">Description</div>
				{#if description}<div>{description}</div>{/if}
			</div>

			<div id="variable-{index}-dataprev " class="flex-col text-left w-52">
				<div class="text-xs">Data Preview</div>
				{#if datapreview}<div>{datapreview}</div>{/if}
			</div>

			<div id="variable-{index}-content" class="flex-col space-y-2 w-52">
				<div class="text-right">
					<!--datatype-->
					{#if datatype}<span title="data type" class="badge variant-filled-primary"
							>{datatype}</span
						>{/if}

					<!--unit-->
					{#if unit}<span title="unit" class="badge variant-filled-secondary">{unit}</span>{/if}

					<!--variable template-->
					{#if template}<span title="template" class="badge variant-filled-surface">{template}</span
						>{/if}

					<!--description-->

					<!-- meaning-->
				</div>
				<div class="text-right">
					<!-- primary key-->
					{#if isKey}<span class="badge variant-filled-error">primary key</span>{/if}
					<!-- optional-->
					{#if isOptional}<span class="badge variant-filled-warning">optional</span>{/if}
				</div>
			</div>
		</div>
	</header>
</div>
