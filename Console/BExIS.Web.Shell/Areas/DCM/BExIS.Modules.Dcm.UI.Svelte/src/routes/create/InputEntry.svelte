<script>
   import { Input, Label, FormGroup} from 'sveltestrap';	

   export let type;
   export let value;
   export let label;
   export let valid;
   export let invalid;
   export let feedback;
</script>

<FormGroup>
 <Label for="{label}">{label}</Label>
{#if type=='Boolean'}
    <Input {label} type="checkbox" bind:checked={value} {valid} {invalid} {feedback}/> 
{:else if type=='String'}
    <Input id={label} type="text" bind:value={value} {valid} {invalid} {feedback}  on:input/>
{:else if type=='Text'}
    <Input id={label} type="textarea" bind:value={value} {valid} {invalid} {feedback}  on:input/>
{:else if type=='Int64' || type=='Int32'}
    <Input type="number" bind:value={value} {valid} {invalid} {feedback} on:input/>
{:else if type=='DateTime'} 
    <Input type="date" bind:value={value} {valid} {invalid} {feedback} on:input/>
{/if}

</FormGroup>