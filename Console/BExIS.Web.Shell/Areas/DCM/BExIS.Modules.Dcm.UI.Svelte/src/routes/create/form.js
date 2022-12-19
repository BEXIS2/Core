import {create, test, enforce,each, only} from 'vest'

const suite = create((data = {}, fieldName)=>
{
  only(fieldName);
  each(data, item => {
    
    test(item.name, item.name+" is required",()=>{
        enforce(item.value).isNotBlank();
    },
    item.index)
  });
})

export default suite