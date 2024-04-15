<script>
	import Fa from 'svelte-fa';
	import { faEye, faEyeSlash } from '@fortawesome/free-solid-svg-icons/index';

	import { createEventDispatcher } from 'svelte';
	import ContentContainer from '../../lib/components/ContentContainer.svelte';

	export let id;
	export let name = '';
	export let description = '';
	export let entityType;
	export let metadataStructure;
	export let linkedSubjects = [];
	export let allowedFileTypes = [];

	let hidden = true;

	const dispatch = createEventDispatcher();
</script>

<ContentContainer>
	<header class="card-header">
		<div class="flex">
			<div class="grow text-left">
				<h2 class="h2">{name}</h2>
			</div>
			<div class="grow-0 w-42 text-right">
				{#if linkedSubjects.length > 0}
					<span class="badge variant-filled-error">in use</span>
				{/if}
				<span class="badge variant-filled-surface">{metadataStructure.text}</span>
				<span class="badge variant-filled-secondary">{entityType.text}</span>
			</div>
		</div>
	</header>

	<div class="p-4 grow">
		{#if description}<blockquote class="blockquote mb-5">{description}</blockquote>{/if}

		<!--supported Filefomats-->

		{#if allowedFileTypes.length > 0}
			<div><i>Restricted to these file types: <b>{allowedFileTypes.join(', ')}</b></i></div>
		{/if}

		<!--linked subjects-->
		{#if linkedSubjects.length > 0}
			<i
				>Show linked {entityType.text}:
				<button class="btn-icon" on:click={() => (hidden = !hidden)}>
					{#if hidden}
						<Fa icon={faEye} />
					{:else}
						<Fa icon={faEyeSlash} />
					{/if}
				</button>
			</i>
			<div class:hidden>
				<ul class="ul list-disc pl-5">
					{#each linkedSubjects as item}
						<li>{item.text} ({item.id})</li>
					{/each}
				</ul>
			</div>
		{/if}
	</div>

	<footer class="card-footer">
		<slot>no action setup</slot>
	</footer>
</ContentContainer>
