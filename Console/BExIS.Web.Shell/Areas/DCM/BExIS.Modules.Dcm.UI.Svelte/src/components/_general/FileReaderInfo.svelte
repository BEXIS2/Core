<script>
 import {Spinner,  FormGroup, Input, Label,Table, Button, Col, Row } from 'sveltestrap';

 export let delimeter;
 export let delimeters;

 export let decimal;
 export let decimals;

 export let textMarker;
 export let textMarkers;
 
</script>

<Row>
 <Col>
   <FormGroup>
     <Label>Delimeter</Label>
     <Input type="select" id="delimeter" bind:value={delimeter}>
      <option value={null}>-- Please select --</option>
       {#each delimeters as item}
          <option value={item.id}>{item.text}</option>
       {/each}
     </Input>
    </FormGroup>
 </Col>

 <Col>
   <FormGroup>
      <Label>Decimal</Label>
      <Input type="select" id="decimal" bind:value={decimal} >
        <option value={null}>-- Please select --</option>
        {#each decimals as item}
            <option value={item.id} >{item.text}</option>
        {/each}
      </Input>
   </FormGroup>
 </Col>
 <Col>
   <FormGroup>
     <Label>TextMarker</Label>
     <Input type="select" id="decimal" bind:value={textMarker} >
       <option value={null}>-- Please select --</option>
       {#each textMarkers as item}
           <option value={item.id} >{item.text}</option>
       {/each}
     </Input>
  </FormGroup>
 </Col>
</Row>