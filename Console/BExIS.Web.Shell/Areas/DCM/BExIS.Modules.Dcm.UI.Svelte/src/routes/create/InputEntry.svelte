<script>
   import {TextInput,TextArea,Checkbox, DateInput,NumberInput} from "@bexis2/bexis2-core-ui";

   export let type;
   export let value;
   export let label;
   export let valid;
   export let invalid;
   export let feedback;
   export let required;
</script>

{#if type=='Boolean'}
    <Checkbox id={label} {label} bind:checked={value} {required} {valid} {invalid} {feedback} on:input/> 
{:else if type=='String'}
    <TextInput id={label} {label} bind:value={value} {valid} {invalid} {feedback} {required} on:input/>
{:else if type=='Text'}
    <TextArea id={label} {label} type="textarea" bind:value={value} {valid} {invalid} {feedback} {required} on:input/>
{:else if type=='Int64' || type=='Int32'}
    <NumberInput id={label} {label} bind:value={value}  {valid} {invalid} {feedback} {required} on:input/>
{:else if type=='DateTime'} 
    <DateInput id={label} {label} bind:value={value} {valid} {invalid} {feedback} {required} on:input/>
{/if}

