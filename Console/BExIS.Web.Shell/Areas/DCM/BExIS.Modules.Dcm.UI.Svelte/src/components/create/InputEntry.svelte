<script>
   import { Input, Label, FormGroup} from 'sveltestrap';	

   export let type;
   export let value;
   export let label;
</script>

<FormGroup>
 <Label for="{label}">{label}</Label>
{#if type=='Boolean'}
    <Input {label} type="checkbox" bind:checked={value} /> 
{:else if type=='String'}
    <Input id={label} type="text" bind:value={value} />
{:else if type=='Int64' || type=='Int32'}
    <Input type="number" bind:value={value} />
{:else if type=='DateTime'} 
    <Input type="date" bind:value={value} />
{/if}

</FormGroup>