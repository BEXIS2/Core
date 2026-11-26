<script>
	import Fa from "svelte-fa";
	import { faArrowLeft } from "@fortawesome/free-solid-svg-icons";
</script>
 
 
 
<button class="badge variant-ghost-primary" on:click={() => history.back()}><Fa	icon="{faArrowLeft}"/> <span >Back</span> </button>
