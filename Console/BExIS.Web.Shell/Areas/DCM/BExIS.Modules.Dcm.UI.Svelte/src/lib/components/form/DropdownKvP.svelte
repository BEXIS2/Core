<script>

import InputContainer from "./InputContainer.svelte";


 export let id;
 export let source;
 export let target;
 export let title;
 export let valid;
 export let invalid;
 export let feedback;
 export let required;

$:selected = null;

$:updatedSelectedValue(target);
$:updatedTarget(selected);

function updatedSelectedValue(selection)
{
  if(selection!=null)
  {
    selected = selection.id
  }
}

function updatedTarget(id)
{
  target = source.find(opt => opt.id === id)
}

</script>


<InputContainer label={title} {feedback} {required}>
 <select 
  {id} 
  class="select"
  class:input-success="{valid}" 
  class:input-error="{invalid}" 
  bind:value={selected}
  on:change
  on:select
 >
  <option value={null}>-- Please select --</option>
  {#each source as e}
      <option value={e.id} >{e.text}</option>
   {/each}
 </select>
</InputContainer>

