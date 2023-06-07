<!-- <script>
    import {TextInput,TextArea,Checkbox, DateInput,NumberInput} from "@bexis2/bexis2-core-ui";
    import {FormGroup,FormText } from 'sveltestrap';

    export let item;
 </script>
 
<FormGroup>
    {#if item.type=='Boolean'}
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
    <FormText>
        {#if item.description!=undefined}
            <Description bind:description={item.description} lengthMinDes={100}></Description>
        {/if}
      </FormText>
</FormGroup> -->
