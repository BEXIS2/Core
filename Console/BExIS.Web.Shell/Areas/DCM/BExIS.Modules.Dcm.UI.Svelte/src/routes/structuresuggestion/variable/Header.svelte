<script>

 import {FormGroup, Input, Row,Col, Alert } from 'sveltestrap';

 export let index;
 export let name;
 export let isKey;
 export let isOptional;

 </script>

<Row>

 <Col xs=4 md=4><h1>{name} ({index})</h1></Col>

 <Col >
  {#if isKey && isOptional}
    <Alert color="warning">If variables are defined as part of the primary key and are optional, there may be complications when uploading.</Alert>
  {/if}
  <div style="float:right">

    <Input id="isKey" type="switch" label="Mark as part of primary keys" bind:checked={isKey} />

    <Input id="isOptional" type="switch" label="Value can be optional" bind:checked={isOptional} />

  </div>
</Col>
</Row>