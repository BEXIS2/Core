<script>
  import {FormGroup, Label} from 'sveltestrap';

  export let source;
  export let target;
  export let title;
  export let description="";
  export let key;

</script>

<Label for="{key}">{title}</Label>

<FormGroup style="max-height: 210px; overflow:auto; overflow-x:hidden;">
  {#each source as item}
    <div class="form-check" >
      <input class="form-check-input" type=checkbox bind:group={target} value={item}>
      <label class="form-check-label" for="{key}">{item}</label>
    </div>
  {/each}
</FormGroup>
