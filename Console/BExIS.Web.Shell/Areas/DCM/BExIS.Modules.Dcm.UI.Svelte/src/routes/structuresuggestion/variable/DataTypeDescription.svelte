<script>
 export let type;
 export let missingvalue = "not set";
</script>

{#if type==="string"}

<p>
 <b>String detected</b>
 If not correct, please check if placeholder for missing values provided correctly.<br>
 <b>missing value = {missingvalue}</b>
</p>

{/if}