<script lang="ts">
	import { TextInput } from "@bexis2/bexis2-core-ui";
 import {convertDisplayName} from '../metadataShared';


export let isKeyMapped: any;
export let isPartyMapped: any;
export let label: string;
export let value: any;
export let path: string = "";

let message: string = '';

if(isKeyMapped){
 message = 'This field is mapped to a system key and cannot be edited here.';
}
else if(isPartyMapped){
 message = 'This field is mapped to a party and cannot be edited here.';
}

</script>

<TextInput 
    id={path}
    label={convertDisplayName(label)}
    bind:value
    disabled={true}
    description={message}
    />
