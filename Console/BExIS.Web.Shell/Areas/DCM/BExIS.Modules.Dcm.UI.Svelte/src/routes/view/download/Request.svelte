<script lang="ts">
  import { onMount } from 'svelte';
	import Text from '../citation/Text.svelte';
	import { TextArea } from '@bexis2/bexis2-core-ui';
 import {sendRequest} from '../services'; 
 import Fa from "svelte-fa";
 import { faEnvelope, faHourglassHalf } from "@fortawesome/free-solid-svg-icons";

  export let id 
  export let exist: boolean = false;

  let intention: string = "";

  function send(id, intention) {
    sendRequest(id, intention)
      .then(response => {
        console.log('Request sent successfully:', response);
        exist = true; // Update the exist variable to true after sending the request
      })
      .catch(error => {
        console.error('Error sending request:', error);
      });
  }

</script>
{#if exist}
  <div class="card flex variant-ghost-warning items-center p-5 gap-5">
   <Fa class="text-warning-500" size="lg" icon={faHourglassHalf} />
   <span class="padding-left-5">You have already requested this dataset. Please wait for approval.</span>
  </div>
{:else}
  
  <button class="btn variant-filled-primary" disabled={intention.trim() === ''} on:click={() => send(id, intention)}>
    <Fa icon={faEnvelope} />
    <span class="padding-left-5">Send Request</span>
  </button>

  <TextArea bind:value={intention} label="Please provide a reason for your request" placeholder="Enter your reason here..." />
{/if}