<script>
	import PlaceholderHook from '$lib/hooks/edit/placeholder/PlaceholderHook.svelte';

	const placeholders = Array.from({ length: 5 });
</script>

<div class="space-y-3">
	{#each placeholders as _, index}
		<div class="placeholder-group" style={`animation-delay: ${index * 90}ms`}>
			<PlaceholderHook />
		</div>
		{#if index < placeholders.length - 1}
			<hr class="!border-dashed opacity-50" />
		{/if}
	{/each}
</div>

<style>
	.placeholder-group {
		animation: placeholderGroupEnter 0.4s ease-out both;
	}

	@keyframes placeholderGroupEnter {
		from {
			opacity: 0;
			transform: translateY(4px);
		}
		to {
			opacity: 1;
			transform: translateY(0);
		}
	}
</style>
