import {create, test, enforce, only} from 'vest'

const suite = create((data = {}, fieldName)=>
{
  only(fieldName);


  test("description","description is required",()=>{
    enforce(data.description).isNotBlank();
  })

  test("dataType","datatype is required",()=>{

   enforce(data.dataType).isNotNull();
   enforce(data.dataType).isNotUndefined();
  })

  test("unit","unit is required",()=>{
   enforce(data.unit).isNotNull();
   enforce(data.unit).isNotUndefined();
  })

})

export default suite