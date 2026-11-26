		
  <script lang="ts">

   export let author: string = '';

  </script>
  
  <div class="flex">
      {#if author && author != ' '}
						   <div class="font-bold mr-2">{author}</div>
      {:else} 
        <div class="font-bold mr-2">n/a</div>
      {/if}
  </div>
