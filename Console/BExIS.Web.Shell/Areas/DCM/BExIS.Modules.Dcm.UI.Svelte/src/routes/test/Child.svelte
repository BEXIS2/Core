<script lang="ts">
	import { createEventDispatcher} from 'svelte'
	// load attributes from div

	function clickFn()
	{
		window.parent.postMessage('saved','*');
	}

</script>




	<button
	class="variant-filled-secondary rounded-md p-2"
	on:click={clickFn}>Send event</button>
	


