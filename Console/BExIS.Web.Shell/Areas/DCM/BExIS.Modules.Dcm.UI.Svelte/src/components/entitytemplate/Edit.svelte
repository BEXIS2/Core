<script>

import Fa from 'svelte-fa/src/fa.svelte'
import CheckBoxList  from '../form/CheckBoxList.svelte'

 import { onMount } from 'svelte'; 
 import {Form, FormGroup, Input, Label, Row, Col, Button, Spinner} from 'sveltestrap';
 
 import { setApiConfig }  from '@bexis2/svelte-bexis2-core-ui'
 import { getEntityTemplate, saveEntityTemplate }  from '../../services/Caller'

 
 import { faSave } from '@fortawesome/free-regular-svg-icons'

 let id = 0;
 
 export let hooks= [];
 export let metadataStructures= [];
 export let dataStructures=[];
 export let systemKeys=[];
 export let entities=[];
 export let groups=[];
 export let filetypes=[];


 $:entityTemplate = null;
 $: console.log("entityTemplate", entityTemplate);
 
 onMount(async () => {
   console.log("start entity template");
   setApiConfig("https://localhost:44345","davidschoene","123456");
   load();
 })
 
  async function load()
  {
    entityTemplate = await getEntityTemplate(id);
  }

  async function handleSubmit() {
    console.log("before submit", entityTemplate);
    debugger;
    const res = saveEntityTemplate(entityTemplate);

    console.log("save", res);

  }

 </script>
 
 <h1>Edit Entity Template</h1>
{#if entityTemplate}
<Form on:submit={handleSubmit}>
    <FormGroup>
    <Label for="name">Name</Label>
    <Input bind:value="{entityTemplate.name}" />
  </FormGroup>
  
  <FormGroup>
    <Label for="description">Description</Label>
    <Input type="textarea" bind:value="{entityTemplate.description}" />
  </FormGroup>
  
  <FormGroup>
    <Label for="entityType">Entity type</Label>
    <Input type="select" name="select" id="entityType" bind:value={entityTemplate.entityType}>
      {#each entities as e}
         <!-- content here -->
         <option value={e}>{e.value}</option>
      {/each}
    </Input>
  </FormGroup>


  <h3>Metadata settings</h3>
  <FormGroup>
    <Label for="metadatastructure">Metadata structure</Label>
    <Input type="select" name="select" id="entityType" bind:value={entityTemplate.metadataStructure}>
      {#each metadataStructures as e}
         <!-- content here -->
         <option value={e}>{e.value}</option>
      {/each}
    </Input>
  </FormGroup>

  <FormGroup>
    <Input id="metadataInvalidSaveMode" type="switch" label="MetadataInvalidSaveMode" bind:value={entityTemplate.metadataInvalidSaveMode} />
  </FormGroup>
<!-- 
  <CheckBoxList key="systemkey" title="Metadata input Field" source={systemKeys} bind:target={entityTemplate.metadataFields} /> -->

  <FormGroup style="max-height: 210px; overflow:auto; overflow-x:hidden;">
    {#each systemKeys as key}
      <div class="form-check">
        <input class="form-check-input" type=checkbox bind:group={entityTemplate.metadataFields} value={key.key}>
        <label class="form-check-label" for="permission">{key.value}</label>
      </div>
    {/each}
  </FormGroup>

  <h3>Group settings</h3>

  <Row>
    <Col>
      <Label for="permission">Permission groups</Label>
      <FormGroup style="max-height: 210px; overflow:auto; overflow-x:hidden;">
        {#each groups as group}
          <div class="form-check">
            <input class="form-check-input" type=checkbox bind:group={entityTemplate.permissionGroups} value={group.key}>
            <label class="form-check-label" for="permission">{group.value}</label>
          </div>
        {/each}
      </FormGroup>
    </Col>
    <Col>
      <Label for="notification">Notification groups</Label>
      <FormGroup style="max-height: 210px; overflow:auto; overflow-x:hidden;">
        {#each groups as group}
          <div class="form-check">
            <input class="form-check-input" type=checkbox bind:group={entityTemplate.notificationGroups} value={group.key}>
            <label class="form-check-label" for="notification">{group.value}</label>
          </div>
        {/each}
      </FormGroup>
    </Col>
  </Row>

  <h3>Additional settings</h3>
  <Row>
    <Col>
      <CheckBoxList key="hooks" title="Disabled hooks" source={hooks} bind:target={entityTemplate.disabledHooks} />
    </Col>
    <Col>
      <CheckBoxList key="filetypes" title="Allowed File Types" source={filetypes} bind:target={entityTemplate.allowedFileTypes} />
    </Col>
  </Row>

  <Button type="submit" color="primary"><Fa icon={faSave}/></Button>
</Form>
{:else}
<Spinner color="info" size="sm" type ="grow" text-center />
{/if}

