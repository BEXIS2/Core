<script lang="ts">
	import { createEventDispatcher, onDestroy, onMount } from 'svelte';
	import Fa from 'svelte-fa';
	import { faTimes } from '@fortawesome/free-solid-svg-icons';

	export let title = 'Popup Title';

	let showPopup = false;

	const dispatch = createEventDispatcher();

	export function openPopup() {
		showPopup = true;
		dispatch('open');
	}

	export function closePopup() {
		showPopup = false;
		dispatch('close');
	}

	function handleKeydown(event: KeyboardEvent) {
		if (event.key === 'Escape') {
			closePopup();
		}
	}

	$: {
		if (showPopup) {
			document.body.style.overflow = 'hidden';
			window.addEventListener('keydown', handleKeydown);
		} else {
			document.body.style.overflow = '';
			window.removeEventListener('keydown', handleKeydown);
		}
	}

	onMount(() => {
		window.addEventListener('keydown', handleKeydown);
	});

	onDestroy(() => {
		document.body.style.overflow = '';
		window.removeEventListener('keydown', handleKeydown);
	});
</script>

{#if showPopup}
	<div
		class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-40"
		role="dialog"
		aria-modal="true"
	>
		<button
			type="button"
			tabindex="-1"
			aria-label="Close popup"
			class="fixed left-0 top-0 z-10 h-screen w-screen cursor-default opacity-0"
			on:click={() => closePopup()}
		></button>
		<div
			class="animate-popup-in z-10 flex max-h-[90vh] w-[700px] max-w-[90vw] flex-col overflow-hidden rounded-xl bg-white shadow-2xl"
			role="document"
		>
			<header
				class="flex items-center justify-between border-b border-gray-200 bg-gray-50 px-6 pb-4 pt-6"
			>
				<h2 class="m-0 text-lg font-semibold">{title}</h2>
				<button class="btn-ghost btn" aria-label="Close" on:click={() => closePopup()}>
					<Fa icon={faTimes} />
				</button>
			</header>
			<div class="flex flex-1 flex-col overflow-auto p-6">
				<slot></slot>
			</div>
		</div>
	</div>
{/if}

<style>
	@keyframes popup-in {
		from {
			transform: translateY(40px) scale(0.98);
			opacity: 0;
		}
		to {
			transform: translateY(0) scale(1);
			opacity: 1;
		}
	}

	.animate-popup-in {
		animation: popup-in 0.18s cubic-bezier(0.4, 0, 0.2, 1);
	}
</style>
