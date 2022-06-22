<script>



import {Button, Table, Column} from 'sveltestrap';

import Header from './ShowHeader.svelte'


export let id; //enityid
export let title;
export let description;
export let structureId;

export let variables =[];

</script>

<Header {id} {structureId} {title} {description} ></Header>

{#if variables}

<Table rows={variables} let:row>
  <Column header="Id" width="1rem">
    {row.id}
  </Column>
  <Column header="Name">
    {row.name}
  </Column>
  <Column header="Unit" >
    {row.unit}
  </Column>
  <Column header="Datatype">
    {row.dataType}
  </Column>
</Table>
{/if}