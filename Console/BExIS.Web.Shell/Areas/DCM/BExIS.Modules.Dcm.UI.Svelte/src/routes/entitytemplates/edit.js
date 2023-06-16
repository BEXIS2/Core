import {create, test, enforce, only} from 'vest'
import { get } from 'svelte/store';
import {entityTemplatesStore} from './store'

const suite = create((data = {}, fieldName)=>
{
  only(fieldName);

  const listOfEntityTemplates = get(entityTemplatesStore).map(e => e.name);
 
  test("name","name is required",()=>{
    enforce(data.name).isNotBlank();
  })
  test("name","name allready exist",()=>{
    return enforce(data.name).notInside(listOfEntityTemplates);
  })

  test("description","description is required",()=>{
    enforce(data.description).isNotBlank();
  })

  test("metadataStructure","metadatastructure is required",()=>{
    enforce(data.metadataStructure).isNotNull();
    enforce(data.metadataStructure).isNotUndefined();
  })

  test("entityType","entity is required",()=>{
    enforce(data.entityType).isNotNull();
    enforce(data.entityType).isNotUndefined();
  })
})

export default suite