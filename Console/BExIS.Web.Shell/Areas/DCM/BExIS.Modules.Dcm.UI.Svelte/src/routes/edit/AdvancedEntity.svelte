<script lang="ts">
	import { type ModalSettings, Tab, TabGroup } from '@skeletonlabs/skeleton';
  import { Modal, getModalStore } from '@skeletonlabs/skeleton';

  import { notificationType, notificationStore } from '@bexis2/bexis2-core-ui';


	import Entity from './Entity.svelte';
	import type { ExtensionType } from './types';
	import ExtensionCreation from './ExtensionCreation.svelte';
  import Fa from 'svelte-fa';
  import { faTrash } from '@fortawesome/free-solid-svg-icons';
  import { deleteExtension } from './services';

	const modalStore = getModalStore();

 let tabSet: number = 0;
 export let id: number = 0;
	export let version: number;
 export let title = "";
 export let extensions:ExtensionType[] = []

 
 async function removeExtensionFn( extId, title){ 
		const modal: ModalSettings = {
			type: 'confirm',
			title: 'Delete ' + title + '(' + extId + ')',
			body: 'Are you sure you wish to delete the current extension?',
			// TRUE if confirm pressed, FALSE if cancel pressed
			response: (r: boolean) => {
				if (r === true) {
					console.log("remove ext", extId);
          var res = deleteExtension(extId);
          res.then((e)=>{

            console.log(e)
            if(e.success){
              notificationStore.showNotification({
                message: 'Extension deleted successfully',
                notificationType: notificationType.success
              })
	
            } else {
              notificationStore.showNotification({
                message: 'Error deleting extension',
                notificationType: notificationType.error
              })
            }

            //reload page
            location.reload();
          })
				}
			}
		};

		modalStore.trigger(modal);
	}

</script>

<TabGroup>
 <Tab bind:group={tabSet} name="entity" value={0}>
  entity
 </Tab>
 <!--existing extentions -->

 {#each extensions as ext (ext.id)}
  <Tab bind:group={tabSet} name={ext.title} value={ext.id}>
   {ext.title} 
   

  </Tab>
 {/each}
 <!--add extentions -->
 <Tab bind:group={tabSet}  name="add" value={100}>
  +
 </Tab>

  <svelte:fragment slot="panel">

  {#if tabSet === 0}
   <Entity {id} {version} {title} />
  {:else if tabSet === 100}
   <ExtensionCreation {id}/>
  {/if}

 {#each extensions as ext (ext.id)}

   {#if tabSet === ext.id}
   <div class="flex justify-end">
    <button class="chip variant-filled-error " type="button" on:click={() => removeExtensionFn(ext.id, ext.title)} ><Fa icon={faTrash} /></button>   
  </div>
  
  <Entity id={ext.id} version={0} title={ext.title} />

   {/if}
  {/each}
 </svelte:fragment>
</TabGroup>
