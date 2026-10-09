<script lang="ts">
    export let value;
    export let row: any;
    export let dispatchFn;
    export let column;

    let copied = false;

    async function handleCopy() {
        try {
            await navigator.clipboard.writeText(row.original.jobKey);
            copied = true;

            // Reset the "Copied!" message after 2 seconds
            setTimeout(() => {
                copied = false;
            }, 2000);
        } catch (err) {
            console.error("Failed to copy text: ", err);
        }
    }
</script>

<div class="cell">
    <button class="btn variant-filled-primary" title={row.original.jobKey} on:click={handleCopy}>
        {#if copied}
            Copied! ✅
        {:else}
            Copy Value
        {/if}
    </button>
</div>