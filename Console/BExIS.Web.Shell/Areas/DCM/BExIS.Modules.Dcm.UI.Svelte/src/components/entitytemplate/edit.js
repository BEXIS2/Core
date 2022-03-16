import {create, test, enforce, only} from 'vest'

const suite = create((data = {}, fieldName)=>
{
  only(fieldName);

  test("name","name is required",()=>{
    enforce(data.name).isNotBlank();
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