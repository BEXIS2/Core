<script>

import Fa from 'svelte-fa/src/fa.svelte'
import CheckBoxList  from '../form/CheckBoxList.svelte'
import CheckBoxKvPList  from '../form/CheckBoxKvPList.svelte'
import DropdownKvP  from '../form/DropdownKvP.svelte'

import suite from './edit'

import {createEventDispatcher} from 'svelte'

import { onMount } from 'svelte'; 
import {Form, FormGroup, Input, Label, Row, Col, Button, Spinner} from 'sveltestrap';

import { setApiConfig }  from '@bexis2/svelte-bexis2-core-ui'
import { getEntityTemplate, saveEntityTemplate }  from '../../services/Caller'

import { faSave } from '@fortawesome/free-regular-svg-icons'


 export let id = 0;
 
 export let hooks= [];
 export let metadataStructures= [];
 export let dataStructures=[];
 export let systemKeys=[];
 export let entities=[];
 export let groups=[];
 export let filetypes=[];

 const dispatch = createEventDispatcher();

 $:entityTemplate = null; 
 
 let form = null;
 let name = null;

  onMount(async () => {
    console.log("start entity template", id);
    setApiConfig("https://localhost:44345","davidschoene","123456");
    load();
    suite.reset();
  })
 
  async function load()
  {
    entityTemplate = await getEntityTemplate(id);
    console.log("load entity", entityTemplate);
  }

  async function handleSubmit() {
    console.log("before submit", entityTemplate);
    const res = await saveEntityTemplate(entityTemplate);
    if(res!=false)
    {
      console.log("save", res);
      dispatch("save", res);
    }
  }


  let res = suite.get();
  $:disabled = !res.isValid();
  // validation
  function onChangeHandler(e)
  {
    // add some delay so the entityTemplate is updated 
    // otherwise the values are old
    setTimeout(async () => {
      console.log("input",e.target.id);
      console.log("Target", e.target.value);
      console.log("entityTemplate", entityTemplate);
      console.log("entity", entityTemplate.entityType);

      res = suite(entityTemplate, e.target.id)

			},10)

     
    //  console.log("errors", res.getErrors());
    //  console.log("name errors", res.getErrors("name"));
    //  console.log("name has errors", res.hasErrors());
    //  console.log("------------------");
    //  console.log("name is valid", res.isValid("name"));
    //  console.log("description is valid", res.isValid("description"));
    //  console.log("metadataStructure is valid", res.isValid("metadataStructure"));
    //  console.log("entityType is valid", res.isValid("entityType"));
    //  console.log("------------------");

    //  console.log("form is valid", res.isValid());

  }


 </script>

{#if entityTemplate}
<!-- <Form on:submit:preventDefault={handleSubmit}> -->
<form on:submit|preventDefault={handleSubmit}>
  <Row>
    <Col xs="6">
      <FormGroup>
        <Label for="name">Name</Label>
        <Input
        id="name"
        bind:value={entityTemplate.name} 
        valid={res.isValid("name")} 
        invalid={res.hasErrors("name")}  
        feedback={res.getErrors("name")} 
        on:input={onChangeHandler}
        required={true}
        />
      </FormGroup>

      <FormGroup>
        <Label for="description" >Description</Label>
        <Input id="description"
        bind:value="{entityTemplate.description}" 
        type="textarea"
        valid={res.isValid("description")} 
        invalid={res.hasErrors("description")}  
        feedback={res.getErrors("description")} 
        on:input={onChangeHandler} 
        required={true} 
        />
      </FormGroup>

    </Col>
    <Col xs="6">
      <DropdownKvP 
      id="entityType" 
      title="Entity" 
      source={entities} 
      bind:target={entityTemplate.entityType} 
      valid={res.isValid("entityType")}
      invalid={res.hasErrors("entityType")}  
      feedback={res.getErrors("entityType")} 
      on:change={onChangeHandler} 
      />
    </Col>
  </Row>

  <Row>
    <Col>
      <h3>Metadata</h3>
      <DropdownKvP 
      id="metadataStructure"
      title="Metadata structure" 
      bind:target={entityTemplate.metadataStructure}
      source={metadataStructures} 
      valid={res.isValid("metadataStructure")}
      invalid={res.hasErrors("metadataStructure")}  
      feedback={res.getErrors("metadataStructure")} 
      on:change={onChangeHandler} 
      />

      <FormGroup>
        <Input id="metadataInvalidSaveMode" type="switch" label="MetadataInvalidSaveMode" bind:value={entityTemplate.metadataInvalidSaveMode} />
      </FormGroup>

      <CheckBoxKvPList key="systemkey" title="Metadata input Field" source={systemKeys} bind:target={entityTemplate.metadataFields} />
    </Col>
    <Col>
      <h3>Datastructure</h3>
      <FormGroup>
        <Input id="hasDatastructure" type="switch" label="hasDatastructure" bind:checked={entityTemplate.hasDatastructure } />
      </FormGroup>
      
      {#if entityTemplate.hasDatastructure}
         <CheckBoxKvPList key="datastructures" title="Datastructures" source={dataStructures} bind:target={entityTemplate.datastructureList} />
      {/if}
    </Col>
  </Row>

  <h3>Group</h3>

  <Row>
    <Col>
      <CheckBoxKvPList key="permission" title="Permission groups" source={groups} bind:target={entityTemplate.permissionGroups} />
    </Col>
    <Col>
      <CheckBoxKvPList key="notification" title="Notification groups" source={groups} bind:target={entityTemplate.notificationGroups} />
    </Col>
  </Row>

  <h3>Additional</h3>
  <Row>
    <Col>
      <CheckBoxList key="hooks" title="Disabled hooks" source={hooks} bind:target={entityTemplate.disabledHooks} />
    </Col>
    <Col>
      <CheckBoxList key="filetypes" title="Allowed File Types" source={filetypes} bind:target={entityTemplate.allowedFileTypes} />
    </Col>
  </Row>

  <Button type="submit" color="primary" {disabled}><Fa icon={faSave}/></Button>
<!-- </Form> -->
</form>
{:else}
<Spinner color="info" size="sm" type ="grow" text-center />
{/if}


