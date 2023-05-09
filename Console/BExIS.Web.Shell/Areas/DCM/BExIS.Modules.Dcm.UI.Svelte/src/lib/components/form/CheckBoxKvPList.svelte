<script>
  
  import InputContainer from "./InputContainer.svelte";

  export let source;
  export let target;
  export let title;
  export let description="";
  export let key;

  let required = false;
  let feedback = "";
</script>

<InputContainer label={title} {feedback} {required}>
  {#each source as item}
    <label class="flex items-center space-x-2" for="{key}">
      <input class="checkbox" type="checkbox" bind:group={target} checked={item.key} />
      <p>{item.value}</p>
    </label>
  {/each}
</InputContainer>

 