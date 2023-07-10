<script lang=ts>
    export let cols:number;
    export let rows:number = 10;
</script>

<div class="placeholder animate-pulse w-full h-10 sp"></div>
<table class="table table-compact w-full">
    {#each Array(rows) as _}
        <tr>
            {#each Array(cols) as _}
                <td><div class="placeholder animate-pulse h-9 w-30 m-3 sp"></div></td>
            {/each}
        </tr>
    {/each}
</table>