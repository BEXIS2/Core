<script lang="ts">
	import { CodeEditor, TextInput, helpStore } from '@bexis2/bexis2-core-ui';
	import { slide } from 'svelte/transition';
	import type { PatternConstraintListItem } from '../models';
	import { createEventDispatcher, onMount } from 'svelte';
	const dispatch = createEventDispatcher();

	import suite from './patternForm';

	export let patternConstraint: PatternConstraintListItem;

	let example: string = '';

	$: result = createRegex(patternConstraint.pattern, example);

	// load form result object
	let res = suite.get();

	// use to actived save if form is valid
	$: disabled = !res.isValid();
	$: dispatch(String(disabled));

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: any) {
		//console.log("input changed", e)
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			// check changed field
			res = suite(patternConstraint, e);
		}, 10);
	}

	onMount(async () => {
		if (patternConstraint.id == 0) {
			suite.reset();
		} else {
			setTimeout(async () => {
				res = suite(patternConstraint, '');
			}, 10);
		}
	});

	function createRegex(p: string, e: string): string {
		disabled = false;
		try {
			let regex = new RegExp(p, 'g');
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
		<!-- svelte-ignore a11y-mouse-events-have-key-events -->
		<div
			class="pb-3"
			on:mouseover={() => {
				helpStore.show('pattern');
			}}
		>
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
		<!-- svelte-ignore a11y-mouse-events-have-key-events -->
		<div
			class="pb-3"
			on:mouseover={() => {
				helpStore.show('result');
			}}
		>
			<!-- svelte-ignore a11y-label-has-associated-control -->
			<label>Result</label>
			{result}
		</div>
	</div>
{/if}
