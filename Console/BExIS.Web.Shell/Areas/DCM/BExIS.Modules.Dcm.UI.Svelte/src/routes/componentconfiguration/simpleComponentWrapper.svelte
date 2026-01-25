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
    <SimpleComponent {simpleComponent} {path} {required} {label} />
{/if}