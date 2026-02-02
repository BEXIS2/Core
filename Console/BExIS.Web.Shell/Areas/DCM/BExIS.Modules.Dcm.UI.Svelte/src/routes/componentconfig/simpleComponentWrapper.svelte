<script lang="ts">
    import { onMount } from 'svelte';
    import {
        TextInput,
        NumberInput,
        TextArea,
        DropdownKVP,
        helpStore,
        CodeEditor
    } from '@bexis2/bexis2-core-ui';
    import { SlideToggle } from '@skeletonlabs/skeleton';
    import SimpleComponent from './simpleComponent.svelte';

    export let simpleComponent: any;
    export let path: string;
    export let required: boolean = false;

    let label: string = !path
        ? ''
        : path.split('.').length > 1
            ? path.split('.')[path.split('.').length - 1]
            : path;

</script>

{#if path && simpleComponent.properties}
<div class="px-5 py-2">
				<div class="flex flex-col md:flex-row md:items-center gap-2">
  <div class="flex-1 min-w-[100px]">
    <SimpleComponent {simpleComponent} {path} {required} {label} />
            </div>
    <div class="flex shrink-0 gap-1 justify-end min-w-[120px]">
    <!-- empty for no buttons -->aaaaaaa
  </div>
  		</div>
</div>
{/if}