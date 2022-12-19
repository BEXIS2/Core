
<script >
import { onMount } from 'svelte';
// import FileInfo from '@bexis2/svelte-bexis2-core-ui/src/lib/components/File/FileInfo.svelte'
import {ListView, TableView, FileIcon, FileInfo, host, username, password, setApiConfig} from '@bexis2/bexis2-core-ui/src/lib/index'

let a = "abcd"
$:h = "no host";
$:u = "no user";
$:p = "no password";

onMount(async () => {
  
 setApiConfig(window.location.origin,"david","123456")
  
		console.log("on mount", h);
		console.log("host", host);
  h=host
  u=username;
  p=password;

	});


</script>

<h1>{h} - {a}</h1>
<h1>hello</h1>
<h2>{u} - {p}</h2>
<ListView></ListView>
<TableView></TableView>
<FileIcon type="excel"/>
<FileInfo name="abcd" size=12334 type="excel"/>
