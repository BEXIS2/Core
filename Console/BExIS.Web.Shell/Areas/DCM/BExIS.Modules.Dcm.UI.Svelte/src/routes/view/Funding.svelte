<script lang="ts">

export let f: string;

const funder = f?f:'No funder information available';


</script>

<div class="card p-5">
 <h4 class="h4">Funding</h4> 
 <b>{funder}</b>
</div>