<script>
    import {FormGroup,FormText } from 'sveltestrap';	
    import {TextInput} from '@bexis2/bexis2-core-ui';
    import Description from './Description.svelte'
  
    export let item
  </script>
    
    <FormGroup>
        <TextInput id="ddf" label="dfsdf" value="dssdf">

        </TextInput>
      <Inputentry on:invalid on:message type={item.type} bind:value={item} label={item.key}/> 
      <FormText>
        {#if item.description!=undefined}
        <Description bind:description={item.description} lengthMinDes={100}></Description>
        {/if}
      </FormText>
    </FormGroup>