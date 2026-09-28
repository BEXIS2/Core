<script lang="ts">
	import { Page, ErrorMessage, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import { onMount } from 'svelte';
 import type { linkType } from '@bexis2/bexis2-core-ui';
	import Entity from './Entity.svelte';
 import { getEntityTemplateByObject } from '../../services/EntityTemplateCaller';
	import Header from './Header.svelte';

 import type { ExtensionType} from './types'
	import AdvancedEntity from './AdvancedEntity.svelte';
	import { getExtensions } from './services';
	import type { EntityTemplateModel } from '$models/EntityTemplate';


	let container;
	let id: number = 0;
	let version: number;
 let title = "";

 let entityTemplate:EntityTemplateModel;
 let extensions:ExtensionType[] = []

 const links: linkType[] = [
		{
			label: 'Manual',
			url: '/home/docs/Datasets#dataset-edit-page'
		}
	];

 onMount(async () => {
  console.log('LOAD EDIT', Date.now);
		// get data from parent
		container = document.getElementById('edit');
		id = Number(container?.getAttribute('dataset'));
		version = Number(container?.getAttribute('version'));

  entityTemplate = await getEntityTemplateByObject(id);
  console.log("🚀 ~ entityTemplate:", entityTemplate)

  if(entityTemplate && entityTemplate.hasExtension) // load extentions if existing
  {
   extensions = await getExtensions(id);
   console.log("🚀 ~ extensions:", extensions)
  }

 });
 

</script>
<Page title="Edit: ({id} | {title})" contentLayoutType={pageContentLayoutType.center} {links}>

 {#if entityTemplate}
  <Header {id} {version} {title} />

  {#if entityTemplate.hasExtension} <!--if using extensions add a tab here-->
   <AdvancedEntity {id} {version} {title} {extensions} entity={entityTemplate.entityType.text} />

  {:else}
   <Entity {id} {version} {title} />
  {/if}
  
 {/if}
 

</Page>