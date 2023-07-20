<script lang="ts">
	import type { decimalCharacterType, orientationType, textMarkerType, textSeperatorType } from '@bexis2/bexis2-core-ui';
 import Item from './ItemValue.svelte';


	export let seperator: textSeperatorType;
	export let textMarker: textMarkerType;
	export let decimal: decimalCharacterType;
	export let orientation: orientationType;
	export let offset: number;
	export let variables: number;
	export let data: number;
	export let unit: number;
	export let description: number;
	export let dateformat: string;

</script>

 <div class="flex gap-5">
		<b>Seperator / Decimal / Text Marker :</b>

		<Item value="{seperator?String.fromCharCode(seperator):null}"/>
		/
		<Item value="{decimal?String.fromCharCode(decimal):null}"/>
		/
		<Item value="{textMarker?String.fromCharCode(textMarker):null}"/>


		<b>Variables / Data / Unit / Description : </b>
  <Item value={variables}/> 
		/ 
  <Item value={data?.toString()}/> 
		/ 
  <Item value={unit?.toString()}/> 
		/ 
  <Item value={description?.toString()}/>
 </div>
