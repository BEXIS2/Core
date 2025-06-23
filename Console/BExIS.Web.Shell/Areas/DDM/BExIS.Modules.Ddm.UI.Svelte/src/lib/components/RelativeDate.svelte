<script lang="ts">
	import { faClockRotateLeft } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';

	export let date: Date;
	export let showIcon: boolean = false;
	export let label: string | undefined = undefined;
	export let tag: string | null = 'span';

	let className: string | undefined = undefined;
	export { className as class };

	const wordOrPlural = (count: number, word: string): string => {
		if (count === 1) return `${count} ${word}`;
		return `${count} ${word}s`;
	};

	const dateToRelative = (date: Date): string => {
		const now = new Date();
		const diff = now.getTime() - date.getTime();
		if (diff < 0) return 'In the future';
		const seconds = Math.floor(diff / 1000);
		const minutes = Math.floor(seconds / 60);
		const hours = Math.floor(minutes / 60);
		const days = Math.floor(hours / 24);
		const months = Math.floor(days / 30);
		const years = Math.floor(months / 12);
		if (years > 0) return wordOrPlural(years, 'year') + ' ago';
		if (months > 0) return wordOrPlural(months, 'month') + ' ago';
		if (days > 0) return wordOrPlural(days, 'day') + ' ago';
		if (hours > 0) return wordOrPlural(hours, 'hour') + ' ago';
		if (minutes > 0) return wordOrPlural(minutes, 'minute') + ' ago';
		return 'Just now';
	};

	const dateToReadable = (date: Date): string => {
		const options: Intl.DateTimeFormatOptions = {
			year: 'numeric',
			month: '2-digit',
			day: '2-digit',
			hour: '2-digit',
			minute: '2-digit',
			second: '2-digit'
		};
		return date.toLocaleString('en-US', options).replace(',', '');
	};
</script>

{#if tag}
	<svelte:element
		this={tag}
		class={className}
		title={(label ? `${label}: ` : '') + dateToReadable(date)}
	>
		{#if showIcon}
			<Fa icon={faClockRotateLeft} class="inline-block" />
		{/if}
		{dateToRelative(date)}
	</svelte:element>
{:else}
	{#if showIcon}
		<Fa icon={faClockRotateLeft} class="inline-block" />
	{/if}
	{dateToRelative(date)}
{/if}
