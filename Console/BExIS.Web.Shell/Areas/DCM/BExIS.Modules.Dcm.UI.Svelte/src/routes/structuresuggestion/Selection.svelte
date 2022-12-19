<script>

//  import Fa from 'svelte-fa/src/fa.svelte'

 import { onMount, createEventDispatcher } from 'svelte'; 

 import {Spinner,  FormGroup, Input, Label,Table, Button, Col, Row } from 'sveltestrap'
//  import {faTrashAlt, faPenToSquare } from '@fortawesome/free-regular-svg-icons'

 import {store, load}  from './caller.js'
 import MissingValues from '../structuresuggestion/MissingValues.svelte'

 export let id;
 export let file="";


 let model = null;

 let isDrag = false;
 let state = [];
 $:selection = [];
 let cLength = 0;
 let rLength = 0;
 let selectionsupport = false;
 
 let selectedRowIndex = 0;
 
 // currently only one requirement exit
 // variable need to be selected
 let isValid = false;
 
 const MARKER_TYPE = {
   VARIABLE: "variable",
   DESCRIPTION: "description",
   UNIT: "unit",
   MISSING_VALUES:  "missing-values",
   DATA:  "data"
 }
 
 const dispatch = createEventDispatcher();

 onMount(async () => {
  console.log("start selection suggestion");
  init();
 })
 
 async function init()
 {
   console.log("load selection");
   model = await load(id,file);
   
   setTableInfos(model.preview, String.fromCharCode(model.delimeter));
   setMarkers(model.markers);

 }
 
 function setTableInfos(rows, delimeter)
 {
   console.log("set tbale infos");
    //number of columns
   cLength = rows[0].split(delimeter).length;
   //number of rows
   rLength = rows.length;
 
   state = new Array(rLength).fill(false);
 
   for (var i = 0; i < state.length; i++) {
     state[i] = new Array(cLength).fill(false);
   }
 
   console.log(state);
   
 }

 function setMarkers(markers)
 {
    for (var i = 0; i < markers.length; i++) {
      let marker = markers[i]
      console.log("marker",marker);
      updateSelection(marker.type, marker.row-1, marker.cells);

      // check if varaible is set, then activet store
      if(marker.type==MARKER_TYPE.VARIABLE){isValid=true}
   }

 }
 
 function beginDrag(e) { isDrag = true; }
 
 function endDrag(e){ isDrag = false; }
 
 const mouseDownHandler = (r, c) => (e) => {
 
     console.log(e.type);
 
     if (e.which === 3 || e.button === 2)
     {
         console.log('Right mouse button at ' + e.clientX + 'x' + e.clientY);
     }
 
     if(r != selectedRowIndex)
     {
       clean();
     }
 
   if (isDrag || e.type === 'mousedown') {
 
       selectedRowIndex = r;
 
       //left mouse click
       if (e.which === 1 || e.button === 0)
       {
         selectCell(c);
       }
 
       // right mouse click
       if (e.which === 3 || e.button === 2)
       {
         deselectCell(c);
       }
   }
 }
 
 const mouseHandler = (r, c) => (e) => {
 
   if (isDrag || e.type === 'mousedown') {
 
     //left mouse click
       if (e.which === 1 || e.button === 0)
       {
         selectCell(c);
       }
 
       // right mouse click
       if (e.which === 3 || e.button === 2)
       {
         deselectCell(c);
       }
   }
 }
 
 const dbclickHandler = (c) => (e) => {
     console.log("dblclick", e.type,isDrag);
   if (isDrag || e.type === 'dblclick') {
       selectRow(selectedRowIndex);
   }
 }
 
 const selectCell = (c) => {
   state[selectedRowIndex][c] = true;
 }
 
 const deselectCell = (c) => {
   state[selectedRowIndex][c] = false;
 
     console.log("deselect");
     // if a selection is active , also delect it
     if(selection.length>0)
     {
       console.log("selection l:", selection.length);
 
       if(selectionsupport) // update alle cells in all stored selections
       {
         for (let index = 0; index < selection.length; index++) {
           const selectObj = selection[index];
           if(selectObj)
           {
             selectObj.cells[c] = false;
             updateSelection(selectObj.type,selectObj.row, selectObj.cells)
           }
         }
       }
       else
       {
 
         var selectObj = selection.find(e=>e.row == selectedRowIndex);
         if(selectObj)
         {
           selectObj.cells[c] = false;
           updateSelection(selectObj.type,selectObj.row, selectObj.cells)
         }
       }
     }
 }
 
 const selectRow = (r) => {
   console.log("set true");
   for (var i = 0; i < cLength; i++) {
     console.log(i);
     state[r][i] = true;
   }
 }
 
 function clean(){
   state = new Array(rLength).fill(false);
 
   for (var i = 0; i < state.length; i++) {
     state[i] = new Array(cLength).fill(false);
   }
 }
 
 function cleanSelection(){
   selection = [];
   isValid=false;
 }
 
 function getMarkerLayout(r){
   var element = selection.find(e=>e.row === r)
 
   if(element)
   {
     console.log(element.type)
     return element.type;
   }
 
   return "variable";
 }
 
 function onclickHandler(type)
 {
 
   // get selected cells
   let selectedCells = state[selectedRowIndex];
 
   if(type==MARKER_TYPE.VARIABLE){isValid=true}
 
   // if selectionsupport is true and one entry exist, means that the cells selection is the same
   // like the stored one
   if(selectionsupport && selection.length>0){ selectedCells = selection[0].cells;}
 
   updateSelection(type, selectedRowIndex, selectedCells);
 
 }
 
 function updateSelection(type, index, cells)
 {
 
   let obj={
     type:type,
     row:index,
     cells:cells
   } 
 
  // if exist row, remove entry
  var exist = selection.find(e=>e.row == obj.row);
   if(exist)
   {
     selection = selection.filter(e=>e.row !== obj.row);
   }
 
   // if exist, remove entry
   exist = selection.find(e=>e.type == obj.type);
   if(exist)
   {
      selection = selection.filter(e=>e.type !== obj.type);
   }
 
   // add obj to list and return new list
   selection = [...selection, obj];
 }
 
 async function save()
 {
   model.markers = selection;

   console.log("save selection", model);
   let res = await store(model);
   console.log(res);

   if(res!=false)
  {
    console.log("selection", res);
    dispatch("saved", model);
  }

 }
 
 </script>
 
 {#if !model || state.length==0} <!--if the model == false, access denied-->
   <Spinner color="primary" size="sm" type ="grow" />
 {:else}  <!-- load page -->
 
 <div id="structure-suggestion-container" on:mousedown="{beginDrag}" on:mouseup="{endDrag}">
  <form on:submit|preventDefault={save}>
  <Row>
      <Col>
        <FormGroup>
          <Label>Delimeter</Label>
          <Input type="select" id="delimeter" bind:value={model.delimeter}>
            <option value={null}>-- Please select --</option>
            {#each model.delimeters as item}
                <option value={item.id}>{item.text}</option>
            {/each}
          </Input>
          </FormGroup>
      </Col>
    
      <Col>
        <FormGroup>
            <Label>Decimal</Label>
            <Input type="select" id="decimal" bind:value={model.decimal} >
              <option value={null}>-- Please select --</option>
              {#each model.decimals as item}
                  <option value={item.id} >{item.text}</option>
              {/each}
            </Input>
        </FormGroup>
      </Col>
      <Col>
        <FormGroup>
          <Label>TextMarker</Label>
          <Input type="select" id="decimal" bind:value={model.textMarker} >
            <option value={null}>-- Please select --</option>
            {#each model.textMarkers as item}
                <option value={item.id} >{item.text}</option>
            {/each}
          </Input>
        </FormGroup>
      </Col>
 
    </Row>
    <FormGroup>
     <Button type="button"  color="danger" on:click={()=> onclickHandler(MARKER_TYPE.VARIABLE)}>Variable</Button>
     <Button type="button"  color="success" on:click={()=>onclickHandler(MARKER_TYPE.UNIT)}>Unit</Button>
     <Button type="button"  color="warning" on:click={()=>onclickHandler(MARKER_TYPE.DESCRIPTION)}>Description</Button>
     <Button type="button"  color="info" on:click={()=>onclickHandler(MARKER_TYPE.MISSING_VALUES)}>Missing Values</Button>
     <Button type="button"  color="primary" on:click={()=>onclickHandler(MARKER_TYPE.DATA)}>Data</Button>
     <Button type="button"  on:click={cleanSelection}>delete</Button> 
     <Button disabled={!isValid}>edit</Button>
     <!--<Fa icon={faTrashAlt}/> <Fa icon={faPenToSquare}/> -->
   </FormGroup>
   <FormGroup>
     <Input id="c1" type="switch" label="selection support"  bind:checked={selectionsupport}/>  
   </FormGroup>

 
   <Row>
     <Col>
      <!-- Missing Values-->
      <MissingValues bind:list={model.missingValues}/>
    </Col>
    <Col>
      <FormGroup style="float:right">
        <Label><b>Controls</b></Label><br/>
        <Label><b>Selection:</b> left mouse button</Label> <br/>
        <Label><b>Drag:</b> left mouse button down and drag</Label><br/>
        <Label><b>Select Row:</b> double click left mouse button </Label><br/>
        <Label><b>Deselect:</b> right mouse button click </Label>
      </FormGroup>
    </Col>
   </Row>
   <Row>
    <Col xs=3>
      <FormGroup>
        <Label><b>Total:</b> {model.total}</Label>
        <Label><b>Found:</b> {model.total-model.skipped}</Label>
        <Label><b>Skipped:</b> {model.skipped}</Label>
      </FormGroup>
    </Col>
  </Row>
 
   <div class="table-container flipped">
     <div class="content">
       <Table>
         <tbody>
           {#each model.preview as row, r}
             <tr>
                 {#each row.split(String.fromCharCode(model.delimeter)) as cell, c}
                   <td 
                     on:dblclick={dbclickHandler(r)} 
                     on:mousedown={mouseDownHandler(r, c)} 
                     on:mouseenter={mouseHandler(r,c)} 
                     class:variable="{selection.find(e=>e.row === r && e.cells[c]===true)?.type ===MARKER_TYPE.VARIABLE}"
                     class:unit="{selection.find(e=>e.row === r  && e.cells[c]===true)?.type ===MARKER_TYPE.UNIT}"
                     class:description="{selection.find(e=>e.row === r  && e.cells[c]===true)?.type ===MARKER_TYPE.DESCRIPTION}"
                     class:missing-values="{selection.find(e=>e.row === r  && e.cells[c]===true)?.type ===MARKER_TYPE.MISSING_VALUES}"
                     class:data="{selection.find(e=>e.row === r  && e.cells[c]===true)?.type ===MARKER_TYPE.DATA}"
                     class:selected="{state[r][c]}"
                   >
                     {cell}
                   </td>
                 {/each} 
             </tr>
           {/each}
         </tbody>
       </Table>
     </div>
   </div>
 </form>
 </div>
 
 {/if}
 <style>
 
 .table-container
 {
   width:100%;
   overflow-x: scroll;
 
 }
 
 .content
 {
   width:100%;
 }
 
 .flipped, .flipped .content
 {
   transform:rotateX(180deg);
   -ms-transform:rotateX(180deg); /* IE 9 */
   -webkit-transform:rotateX(180deg); /* Safari and Chrome */
 }
 
 table, tr, td{
   -webkit-touch-callout: none; /* iOS Safari */
     -webkit-user-select: none; /* Safari */
      -khtml-user-select: none; /* Konqueror HTML */
        -moz-user-select: none; /* Old versions of Firefox */
         -ms-user-select: none; /* Internet Explorer/Edge */
             user-select: none; 
 }
 
  tr:hover
  {
    background-color: #efefef;
    cursor: pointer;
  }
  
  tr:scope
  {
    background-color: seagreen;
  }
 
  .selected
  {
    background-color: lightgrey;
  }
 
  .variable
  {
    background-color: var(--bs-danger);
    color:white;
  }
 
  .unit
  {
    background-color: var(--bs-success);
    color:white;
  }
 
  .description
  {
    background-color: var(--bs-warning);
  }
 
 .missing-values
 {
   background-color: var(--bs-info);
 }
 
 .data
 {
   background-color: var(--bs-primary);
   color:white;
 }
 
 </style>
 
 
 
 