<script>
 
import Fa from 'svelte-fa/src/fa.svelte'
import { faPlus } from '@fortawesome/free-solid-svg-icons'

import { onMount } from 'svelte'; 
import {Spinner,Button, Collapse} from 'sveltestrap';

import { setApiConfig }  from '@bexis2/svelte-bexis2-core-ui'

import { 
  getEntities,
  getDataStructures,
  getMetadataStructures,
  getSystemKeys,
  getGroups, 
  getHooks,
  getFileTypes
}  from '../services/Caller'

import Edit from '../components/entitytemplate/Edit.svelte'
import Overview from '../components/entitytemplate/Overview.svelte'

let hooks= [];
let metadataStructures= [];
let dataStructures=[];
let systemKeys=[];
let entities=[];
let groups=[];
let filetypes=[];

let isOpen = false;

onMount(async () => {
  console.log("start entity template");
  setApiConfig("https://localhost:44345","davidschoene","123456");

  hooks = await getHooks();
  metadataStructures = await getMetadataStructures();
  dataStructures = await getDataStructures();
  systemKeys = await getSystemKeys();
  entities = await getEntities();
  groups = await getGroups();
  filetypes = await getFileTypes();

  console.log("hooks", hooks);
  console.log("metadataStructures", metadataStructures);
  console.log("dataStructures",dataStructures);
  console.log("systemKeys",systemKeys);
  console.log("entities",entities);
  console.log("groups",groups);
  console.log("filetypes",filetypes);

})

</script>
<Button color="primary" on:click={() => (isOpen = !isOpen)} class="mb-3">
 <Fa icon={faPlus}/>
</Button>
<Collapse {isOpen}>
<Edit  {hooks} {metadataStructures} {dataStructures} {systemKeys} {entities} {groups} {filetypes}/>
</Collapse>
<Overview/>
