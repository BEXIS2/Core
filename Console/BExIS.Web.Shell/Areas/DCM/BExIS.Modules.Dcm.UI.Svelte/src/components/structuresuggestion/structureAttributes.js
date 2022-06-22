import {create, test, enforce, only} from 'vest'

const suite = create((data = {}, fieldName)=>
{
  only(fieldName);

  test("title","title is required",()=>{
    enforce(data.title).isNotBlank();
  })

  test("description","description is required",()=>{
    enforce(data.description).isNotBlank();
  })

})

export default suite