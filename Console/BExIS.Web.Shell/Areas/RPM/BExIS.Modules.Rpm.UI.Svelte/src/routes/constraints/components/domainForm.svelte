<script lang="ts">
    import { CodeEditor } from '@bexis2/bexis2-core-ui';
    import { slide } from 'svelte/transition';

    export let domainConstraint: any;

</script>

<div class="h-80" in:slide out:slide>
    <CodeEditor
        id="domain"
        initialValue={domainConstraint.domain} 
        toggle={false} 
        bind:value={domainConstraint.domain}
        actions={false}
        styles={
            {
                '&': {
                borderRadius: '0.5rem',
                width: '100%',
                height: '20rem'
                },
                '.cm-scroller': {
                borderRadius: '0.5rem'
                }
            }
        } 
    />
</div>