<script>

import Fa from 'svelte-fa/src/fa.svelte'
import { faFileAlt } from '@fortawesome/free-solid-svg-icons'
import {Row,Col } from 'sveltestrap';

 export let id;
 export let index;
 export let name;
 export let description;


 const theme = {
  primaryColor: 'var(--bexis2-text-color-level-3)',
  secondaryColor: '#000000',
  primaryOpacity: 0.8,
  secondaryOpacity: 0.6,
}

</script>

<!-- svelte-ignore a11y-click-events-have-key-events -->
<div class="list-item" on:click >

 <Row>
  <Col xs=4 md=2>
   <Fa 
    size="3x" {...theme} icon={faFileAlt} class="lits-item-icon"/>
  </Col>
  <Col >
   <b>{name}</b> <br/>
   <span class="list-item-description">{description}</span>
  </Col>
 </Row>

</div>

<style>
.list-item{
  /* color: var(--nav-text-color); */
  /* border-bottom: 1px var(--border) solid; */
  padding: 0.5rem;
  box-shadow: 0px 1px 2px -2px var(--bexis2-text-color-level-2); 
  -webkit-box-shadow: 0px 1px 2px -2px var(--bexis2-text-color-level-2);
  -moz-box-shadow: 0px 1px 2px -2px var(--bexis2-text-color-level-2);
}
.list-item:hover
{
 background-color: var(--bexis2-hover);
 cursor: pointer;
}

.list-item:active
{
 background-color: var(--bexis2-hover);
 cursor: pointer;
}

.list-item-description
{
 color: var(--bexis2-text-color-level-3);
}

</style>