<script>
   import { Input} from 'sveltestrap';	

   export let type;
   export let value;
   export let label;
</script>


{#if type=='Boolean'}
    <Input {label} type="checkbox" bind:checked={value} /> 
{:else if type=='String'}
    <Input id={label} type="text" bind:value={value} />
{:else if type=='Int64' || type=='Int32'}
    <Input type="number" bind:value={value} />
{:else if type=='DateTime'} 
    <Input type="date" bind:value={value} />
{/if}
