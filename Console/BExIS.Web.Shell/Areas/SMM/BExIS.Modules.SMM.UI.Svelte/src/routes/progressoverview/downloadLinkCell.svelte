<script lang="ts">
    export let value;
    export let row;
    export let dispatchFn;
    export let column;
</script>

<div class="cell">
    <a href={row.original.downloadLink} class="text-blue-600 underline hover:text-blue-800">Download link</a>
</div>