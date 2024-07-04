<script lang="ts">
	export let value: any[];

	let linkUrl: URL | null = null;
	$: linkUrl =  value.uri === undefined || value.uri == '' ? null : converttoURL(value.uri);

	function converttoURL(uri:string): URL | null
	{
		try{
			return new URL(uri,undefined );
		}
		catch
		{
			return null
		}
	}
</script>

{#if value != undefined && value.name != undefined}
	{#if linkUrl != null && (linkUrl.protocol == 'http:' ||  linkUrl.protocol == 'https:')}
		<a href="{value.uri}" target="_blank">{value.name}</a>	
	{:else}
		{value.name}	
	{/if}
{/if}
