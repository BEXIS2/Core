<script lang="ts">
	import { CodeEditor, TextInput } from '@bexis2/bexis2-core-ui';
	import { slide } from 'svelte/transition';
	import type { PatternConstraintListItem } from '../models';

	export let patternConstraint: PatternConstraintListItem;

	let example: string = '';
	$: result = createRegex(patternConstraint.pattern, example);

	function createRegex(p: string, e: string): string {
		try {
			let regex = new RegExp(p);
			let r = e.match(regex)?.toString();
			r = r == undefined || '' ? 'No Match' : r;
			return r;
		} catch {
			return 'Invalid Regex';
		}
	}
</script>

{#if patternConstraint}
	<div class="grid grid-cols-3 gap-5" in:slide out:slide>
		<div class="pb-3">
			<!-- svelte-ignore a11y-label-has-associated-control -->
			<label>Regex Expression</label>
			<CodeEditor
				id="pattern"
				language="regex"
				initialValue={patternConstraint.pattern}
				toggle={false}
				bind:value={patternConstraint.pattern}
				actions={false}
			/>
		</div>
		<div class="pb-3">
			<TextInput id="example" label="Example" help={true} bind:value={example} />
		</div>
		<div class="pb-3">
			<!-- svelte-ignore a11y-label-has-associated-control -->
			<label>Result</label>
			{result}
		</div>
	</div>
{/if}
