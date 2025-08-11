<!--
  Confetti.svelte
  This component was written with much help by GitHub Copilot, using GPT-4.1.
  It creates a confetti effect that is used for the change of the curation entry
  state, specifically for changing to the "changed" and "approved" state.
  Copilot was used as this component required a large amount of complex maths.
  It has been altered to fit the specific requirements of the application.
-->

<script lang="ts">
	import { createEventDispatcher, tick } from 'svelte';

	export let x = 0;
	export let y = 0;
	export let show = false;
	export let count = 36;
	export let radius = 36;
	export let duration = 900; // ms

	export function trigger(event: MouseEvent) {
		const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
		const parentRect = (
			event.currentTarget as HTMLElement
		).offsetParent?.getBoundingClientRect() ?? { left: 0, top: 0 };
		x = rect.left - parentRect.left + rect.width / 2;
		y = rect.top - parentRect.top + rect.height / 2;
		show = false;
		tick().then(() => (show = true));
	}

	const dispatch = createEventDispatcher();

	function randomColor() {
		return [
			'#fbbf24', // yellow
			'#34d399', // green
			'#60a5fa', // blue
			'#f472b6', // pink
			'#f87171', // red
			'#a78bfa' // purple
		][Math.floor(Math.random() * 6)];
	}

	type ConfettiPiece = {
		x: number;
		y: number;
		color: string;
		rotate: number;
		angle: number;
		delay: number;
	};

	let confettiPieces: ConfettiPiece[] = [];

	$: if (show) {
		confettiPieces = Array.from({ length: count }, (_, i) => {
			const angle = (2 * Math.PI * i) / count;
			const xPos = Math.cos(angle) * radius * (0.4 + Math.random());
			const yPos = Math.sin(angle) * radius * (0.4 + Math.random());
			const spin = (Math.random() - 0.5) * 180;
			return {
				x: xPos,
				y: yPos,
				color: randomColor(),
				rotate: spin,
				angle: angle * (180 / Math.PI),
				delay: Math.random() * 0.1
			};
		});
		// Auto-hide after duration
		setTimeout(() => {
			dispatch('hide');
		}, duration);
	}
</script>

{#if show}
	{#each confettiPieces as c}
		<span
			class="confetti-piece pointer-events-none size-1"
			style="
                left: {x}px;
                top: {y}px;
                background: {c.color};
                --x: {c.x}px;
                --y: {c.y}px;
                --spin: {c.rotate}deg;
                --angle: {c.angle}deg;
                transform: translate(-50%, -50%) rotate({c.angle}deg);
                animation-delay: {c.delay}s;
            "
		></span>
	{/each}
{/if}

<style>
	.confetti-piece {
		position: absolute;
		opacity: 0.85;
		z-index: 100;
		animation: confetti-circle-pop 0.9s cubic-bezier(0.3, 1.5, 0.3, 1) forwards;
	}

	@keyframes confetti-circle-pop {
		0% {
			opacity: 1;
			transform: translate(-50%, -50%) rotate(var(--angle, 0deg)) scale(0.7);
		}
		60% {
			opacity: 1;
			transform: translate(calc(-50% + var(--x, 0px)), calc(-50% + var(--y, 0px)))
				rotate(calc(var(--angle, 0deg) + var(--spin, 0deg))) scale(1.1);
		}
		100% {
			opacity: 0;
			transform: translate(calc(-50% + var(--x, 0px)), calc(-50% + var(--y, 0px)))
				rotate(calc(var(--angle, 0deg) + var(--spin, 0deg) * 2)) scale(0.8);
		}
	}
</style>
