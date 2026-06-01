<script lang="ts">
    import { getDifference } from "$lib/helper/custom_diff";

    // first diff cell test

    export let value;
    export let row;
    export let dispatchFn;
    export let column;

    $: diffs = getDifference(row.original.originalName, value);
</script>


<div class="cell">
    {#each diffs as part}
    {#if part.removed}
        <del class="removed">{part.value}</del>
    {:else if part.added}
        <ins class="added">{part.value}</ins>
    {:else}
        <span>{part.value}</span>
    {/if}
    {/each}
</div>

<style>
    .cell {
        flex: 1;
        padding: 8px;
        border: 1px solid #ddd;
        white-space: pre-wrap;
    }
    del.removed {
        background-color: #ffeef0;
        color: #b31412;
        text-decoration: line-through;
    }
    ins.added {
        background-color: #e6ffec;
        color: #22863a;
        text-decoration: none;
        font-weight: bold;
    }
</style>
