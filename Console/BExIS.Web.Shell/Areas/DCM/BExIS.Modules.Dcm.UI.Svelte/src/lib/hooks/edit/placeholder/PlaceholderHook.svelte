<script>
	import PlaceHolderHookContent from './PlaceHolderHookContent.svelte';
</script>

<div class="placeholder-card flex p-5 m-2 gap-5">
	<div class="flex-none w-48 space-y-2">
		<div class="placeholder-line h-6 w-48"></div>
		<div class="placeholder-line h-6 w-32"></div>
	</div>

	<PlaceHolderHookContent />
</div>

<style>
	.placeholder-card {
		animation: placeholderEnter 0.45s ease-out both;
	}

	.placeholder-card :global(.placeholder-line),
	.placeholder-card :global(.placeholder-block) {
		position: relative;
		overflow: hidden;
		border-radius: 999px;
		background: linear-gradient(90deg, rgba(209, 213, 219, 0.95) 0%, rgba(243, 244, 246, 0.95) 50%, rgba(209, 213, 219, 0.95) 100%);
		background-size: 200% 100%;
		animation: placeholderShimmer 1.5s ease-in-out infinite;
	}

	.placeholder-line {
		position: relative;
		overflow: hidden;
		border-radius: 999px;
		background: linear-gradient(90deg, rgba(209, 213, 219, 0.95) 0%, rgba(243, 244, 246, 0.95) 50%, rgba(209, 213, 219, 0.95) 100%);
		background-size: 200% 100%;
		animation: placeholderShimmer 1.5s ease-in-out infinite;
	}

	@keyframes placeholderShimmer {
		0% {
			background-position: 200% 0;
		}
		100% {
			background-position: -200% 0;
		}
	}

	@keyframes placeholderEnter {
		from {
			opacity: 0;
			transform: translateY(6px);
		}
		to {
			opacity: 1;
			transform: translateY(0);
		}
	}
</style>
