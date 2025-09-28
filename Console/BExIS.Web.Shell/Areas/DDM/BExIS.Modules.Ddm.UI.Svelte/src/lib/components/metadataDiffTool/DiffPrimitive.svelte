<script lang="ts">
	export let value1: string | number | boolean | undefined | null;
	export let value2: string | number | boolean | undefined | null;

	$: value1String = String(value1);
	$: value2String = String(value2);

	$: useSimpleFormat =
		typeof value1 !== typeof value2 || value1String.length < 40 || value2String.length < 40;
	$: isDiff = value1 !== value2;

	function renderValue(val: string) {
		return val.length > 0 ? val : '\u00A0';
	}

	interface DiffPart {
		value: string;
		removed: boolean;
		added: boolean;
	}

	function customDiff(a: string, b: string): DiffPart[] {
		const unitsA = a.split(/\s+/);
		const unitsB = b.split(/\s+/);

		// LCS algorithm to find the longest common subsequence
		const m = unitsA.length;
		const n = unitsB.length;
		const dp: number[][] = Array.from({ length: m + 1 }, () => Array(n + 1).fill(0));

		// Build LCS table
		for (let i = 1; i <= m; i++) {
			for (let j = 1; j <= n; j++) {
				if (unitsA[i - 1] === unitsB[j - 1]) {
					dp[i][j] = dp[i - 1][j - 1] + 1;
				} else {
					dp[i][j] = Math.max(dp[i - 1][j], dp[i][j - 1]);
				}
			}
		}

		// Backtrack to get diff parts
		let i = m,
			j = n;
		const parts: DiffPart[] = [];
		while (i > 0 || j > 0) {
			if (i > 0 && j > 0 && unitsA[i - 1] === unitsB[j - 1]) {
				parts.unshift({ value: unitsA[i - 1], removed: false, added: false });
				i--;
				j--;
			} else if (j > 0 && (i === 0 || dp[i][j - 1] >= dp[i - 1][j])) {
				parts.unshift({ value: unitsB[j - 1], removed: false, added: true });
				j--;
			} else if (i > 0 && (j === 0 || dp[i][j - 1] < dp[i - 1][j])) {
				parts.unshift({ value: unitsA[i - 1], removed: true, added: false });
				i--;
			}
		}

		// Combine adjacent parts of the same type
		const combined: DiffPart[] = [];
		for (const part of parts) {
			const last = combined[combined.length - 1];
			if (last && last.removed === part.removed && last.added === part.added) {
				last.value += ' ' + part.value;
			} else {
				combined.push(part);
			}
		}

		return combined;
	}

	let showCustomDiff = true;
</script>

{#if !isDiff}
	{#if value1String.length > 0 && value1String !== 'null' && value1String !== 'undefined'}
		<span class="rounded bg-surface-100 px-1">
			{value1String}
		</span>
	{:else}
		<span class="rounded bg-warning-100 px-1 font-mono text-warning-800">empty</span>
	{/if}
{:else}
	<!-- Simple Format -->
	<div class:flex={!showCustomDiff} class="my-2 hidden items-center gap-2">
		<span class="h-full rounded bg-error-50 px-2 py-0.5 text-error-800">
			{renderValue(value1String)}
		</span>
		<span class="text-surface-500">→</span>
		<span class="h-full rounded bg-success-100 px-2 py-0.5 text-success-800">
			{renderValue(value2String)}
		</span>
	</div>
	<!-- Detailed Diff -->
	{#if !useSimpleFormat}
		<div
			class="t-2 hidden w-full flex-wrap gap-1 rounded bg-surface-100 p-2"
			class:flex={showCustomDiff}
		>
			{#each customDiff(value1String, value2String) as part}
				{#if part.removed}
					<span class="rounded bg-error-50 px-1 text-error-800 line-through hover:no-underline">
						{part.value}
					</span>
				{:else if part.added}
					<span class="rounded bg-success-100 px-1 text-success-800">{part.value}</span>
				{:else}
					<span class="px-1">{part.value}</span>
				{/if}
			{/each}
		</div>
	{/if}
	{#if !useSimpleFormat}
		<label class="my-2 flex cursor-pointer items-center justify-center gap-1">
			<input type="checkbox" bind:checked={showCustomDiff} class="checkbox mr-1" />
			<span class="text-sm text-surface-600">Show detailed diff</span>
		</label>
	{/if}
{/if}
