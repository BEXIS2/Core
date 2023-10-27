<script lang="ts">
    import { CodeEditor, TextInput } from '@bexis2/bexis2-core-ui';
	import { slide } from 'svelte/transition';

    export let rangeConstraint: any;
    $: console.log('rangeConstraint',rangeConstraint);

	
</script>
<div class="grid grid-cols-3 gap-5" in:slide out:slide>
    <div class="pb-3">
        <div>
        <!-- svelte-ignore a11y-label-has-associated-control -->
        <label>include</label>
        <input id="includeLowerbound" type="checkbox" bind:checked={rangeConstraint.lowerboundIncluded} />
        </div>
        <TextInput
            id="lowerbound"
            label="Lower Bound"
            help={true}
            bind:value={rangeConstraint.lowerbound}							
        />
    </div>
    <div class="pb-3">
        <div>
        <!-- svelte-ignore a11y-label-has-associated-control -->
        <label>include</label>
        <input id="includeUpperbound" type="checkbox" bind:checked={rangeConstraint.upperboundIncluded} />
        </div>
        <TextInput
            id="upperbound"
            label="Upper Bound"
            help={true}
            bind:value={rangeConstraint.upperbound}							
        />
    </div>
</div>