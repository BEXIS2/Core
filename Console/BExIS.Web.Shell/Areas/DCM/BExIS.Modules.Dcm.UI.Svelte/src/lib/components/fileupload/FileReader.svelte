<script lang="ts">
	import type { decimalCharacterType, orientationType } from '@bexis2/bexis2-core-ui';
 import Item from './Item.svelte';

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

<div class="grid grid-cols-2 py-3">
 <div>
  <Item name="Seperator" value={seperator?String.fromCharCode(seperator):null}/>
  <Item name="Decimal" value={decimal?String.fromCharCode(decimal):null}/>
  <Item name="Text Marker" value={textMarker?String.fromCharCode(textMarker):null}/>
 </div>
 <div>
  <!-- <Item name="Orientation" value={orientation?.toString()}/> -->
  <!-- <Item name="Offset" value={offset?.toString()}/> -->
  <Item name="Variables" value={variables}/>
  <Item name="Data" value={data?.toString()}/>
  <Item name="Unit" value={unit?.toString()}/>
  <Item name="Description" value={description?.toString()}/>
  <!-- <Item name="Dateformat" value={dateformat?.toString()}/> -->
</div>
</div>