<script lang="ts">
	import { TextInput } from "@bexis2/bexis2-core-ui";
 import {convertDisplayName} from '../../../lib/components/utils/metadata/metadataShared';


export let isKeyMapped: any;
export let isPartyMapped: any;
export let label: string;
export let value: any;
export let path: string = "";

let message: string = '';

if(isKeyMapped){
 message = 'This field will be filled automatically when saving the metadata and cannot be edited here.';
}
else if(isPartyMapped){
 message = 'This field will be filled by a party selection above and cannot be edited here.';
}

</script>

<TextInput 
    id={path}
    label={convertDisplayName(label)}
    bind:value
    disabled={true}
    description={message}
    />
