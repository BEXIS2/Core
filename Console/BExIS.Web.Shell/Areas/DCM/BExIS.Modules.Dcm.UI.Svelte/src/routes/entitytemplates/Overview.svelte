<script lang="ts">
  import Fa from 'svelte-fa/src/fa.svelte'

  import {createEventDispatcher} from 'svelte'

  import { faTrash, faPen } from '@fortawesome/free-solid-svg-icons'
  import { onMount } from 'svelte'; 

  // ui components
  import {Spinner} from '@bexis2/bexis2-core-ui'
  import Card from './Card.svelte'

  //services
  import { setApiConfig }  from '@bexis2/bexis2-core-ui';
  import { deleteEntityTemplate }  from '../../services/EntityTemplateCaller'

  //types
  import type { EntityTemplateModel } from '../../models/EntityTemplate'

  const dispatch = createEventDispatcher();

  export let entitytemplates:EntityTemplateModel[];

  function edit(id){
    console.log("edit",id)
    dispatch("edit",id);
  }

  async function remove(index, id){

    console.log(index,id);
  //remove in backend
    const res = await deleteEntityTemplate(id)
    if(res === true)
    {
      //remove list
      entitytemplates = entitytemplates.filter((id, idx) => {
        return idx !== index;
      });
    }
  }


</script>

<div class="py-5 w-full grid grid-cols-2 md:grid-cols-3 gap-4">
  {#if entitytemplates}
  {#each entitytemplates as item, i (item.id)}
    <Card {...item} >
      <button class="btn variant-ringed-primary" on:click={edit(item.id)}><Fa icon="{faPen}" /></button>
      <button class="btn variant-ringed-secondary" on:click={remove(i, item.id)}><Fa icon="{faTrash}"/></button>
    </Card>
  {/each}
  {:else}
   <Spinner />
  {/if}
</div>