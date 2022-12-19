<script>
 import {FormGroup, Label, Input} from 'sveltestrap';

 export let id;
 export let source;
 export let target;
 export let title;
 export let valid;
 export let invalid;
 export let feedback;

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

<FormGroup>
 <Label>{title}</Label>
 <Input 
 type="select" 
 {id} 
 bind:value={selected} 
 {valid}
 {invalid}
 {feedback}
 on:change
 on:select
 >
  <option value={null}>-- Please select --</option>
  {#each source as e}
      <option value={e.id} >{e.text}</option>
   {/each}
 </Input>
</FormGroup>