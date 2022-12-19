<script>

import Fa from 'svelte-fa/src/fa.svelte'
import CheckBoxKvPList  from '../form/CheckBoxKvPList.svelte'
import DropdownKvP  from '../form/DropdownKvP.svelte'
import MultiSelect  from '../form/MultiSelect.svelte'

import suite from './edit'

import {createEventDispatcher} from 'svelte'

import { onMount } from 'svelte'; 
import {FormGroup, Input, Label, Row, Col, Button, Spinner} from 'sveltestrap';

import { setApiConfig }  from '@bexis2/bexis2-core-ui/src/lib/index'
import { getEntityTemplate, saveEntityTemplate }  from '../../services/Caller'

import { faSave, faTrashAlt } from '@fortawesome/free-regular-svg-icons'


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

    // if id > 0 then run validation
    if(id>0)
    {
      res = suite(entityTemplate);
    }

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

  // validation
  let res = suite.get();
  // flag to enable submit button
  $:disabled = !res.isValid();

  //change event: if input change check also validation only on the field
  // e.target.id is the id of the input component
  function onChangeHandler(e)
  {
    // add some delay so the entityTemplate is updated 
    // otherwise the values are old
    setTimeout(async () => {
      res = suite(entityTemplate, e.target.id)
			},10)
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
    <Col xs="12">
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
  </Row>
  <Row>
    <Col>
      <h3>Metadata</h3>
      <DropdownKvP 
      id="metadataStructure"
      title="Structure" 
      bind:target={entityTemplate.metadataStructure}
      source={metadataStructures} 
      valid={res.isValid("metadataStructure")}
      invalid={res.hasErrors("metadataStructure")}  
      feedback={res.getErrors("metadataStructure")} 
      on:change={onChangeHandler} 
      />

      <MultiSelect 
      title="Required input fields" 
      source={systemKeys}
      bind:target={entityTemplate.metadataFields}
      optionIdentifier="key"
      labelIdentifier="value"
      isComplex={true}
      />
      
      <FormGroup>
        <Input id="metadataInvalidSaveMode" type="switch" label="Invalid save mode" bind:value={entityTemplate.metadataInvalidSaveMode} />
      </FormGroup>

    </Col>
    <Col>
      <h3>Datastructure</h3>
      <FormGroup>
        <Input id="hasDatastructure" type="switch" label="Use datastructures?" bind:checked={entityTemplate.hasDatastructure } />
      </FormGroup>
      
      {#if entityTemplate.hasDatastructure}
        <CheckBoxKvPList key="datastructures" title="Datastructures" source={dataStructures} bind:target={entityTemplate.datastructureList} />
      {/if}
    </Col>
  </Row>

  <h3>Group</h3>

  <Row>
    <Col>
      <MultiSelect  
      title="Permission" 
      source={groups} 
      bind:target={entityTemplate.permissionGroups}
      optionIdentifier="key"
      labelIdentifier="value"
      isComplex={true}
      />
    </Col>
    <Col>
      <MultiSelect  
      title="Notification" 
      source={groups} 
      bind:target={entityTemplate.notificationGroups} 
      optionIdentifier="key"
      labelIdentifier="value"
      isComplex={true}
      />
    </Col>
  </Row>

  <h3>Additional</h3>
  
  <Row>
    <Col>
      <MultiSelect 
      title="Disabled hooks" 
      source={hooks} 
      bind:target={entityTemplate.disabledHooks} 
      />
    </Col>
    <Col>
      <MultiSelect 
      title="Allowed file types" 
      source={filetypes} 
      bind:target={entityTemplate.allowedFileTypes} 
      /> 
    </Col>
  </Row>

  <Button type="submit" color="primary" {disabled}><Fa icon={faSave}/></Button>
  <Button type="button" color="danger" on:click={()=> dispatch("cancel")}><Fa icon={faTrashAlt}/></Button>
<!-- </Form> -->
</form>
{:else}
<Spinner color="info" size="sm" type ="grow" text-center />
{/if}
