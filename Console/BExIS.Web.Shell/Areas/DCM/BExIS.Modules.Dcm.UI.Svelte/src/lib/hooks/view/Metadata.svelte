<script lang="ts">
	import { faFileLines } from "@fortawesome/free-solid-svg-icons";
	import Fa from "svelte-fa";


export let id = 0;
export let version = 1;
export let tag = 0;
export let hook;
export let description = '';


const url = `/dcm/view/metadata?id=${id}&version=${version}&tag=${tag}`;

</script>

  <div class="flex justify-end gap-1">
   <a href={url} target="_blank" class="badge variant-filled-primary">
    <Fa icon={faFileLines} />
    <span class="ml-2">Metadata</span>
   </a>
  </div>

  {description}
