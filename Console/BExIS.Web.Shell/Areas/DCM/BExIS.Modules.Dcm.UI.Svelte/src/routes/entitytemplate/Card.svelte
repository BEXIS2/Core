<script>

import {
    Button,
    Card,
    CardBody,
    CardFooter,
    CardHeader,
    CardSubtitle,
    CardText,
    CardTitle
  } from 'sveltestrap';

import { createEventDispatcher } from 'svelte'


export let id;
export let name = "";
export let description = "";
export let entityType;
export let metadataStructure;

// export let metadataFields;
// export let permissionGroups;
// export let notificationGroups;
// export let disabledHooks;
// export let allowedHooks;

const dispatch = createEventDispatcher();

</script>
<Card class="mb-3">
 <CardHeader>
   <CardTitle>{name}</CardTitle>
 </CardHeader>
 <CardBody>
   <CardSubtitle>{#if description }{description}{/if}</CardSubtitle>
   <CardText>
    <span>{entityType.text}</span> <br>
    <span>{metadataStructure.text}</span>
   </CardText>
 </CardBody>
 <CardFooter>
    <slot>
      no action setup
    </slot>
 </CardFooter>
</Card>