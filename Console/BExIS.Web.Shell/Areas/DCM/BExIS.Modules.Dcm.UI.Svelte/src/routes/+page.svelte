<script lang="ts">
 import {ListView, FileUploader, type user, type FileUploaderModel} from '@bexis2/BEXIS2-core-ui/src/lib/index'

let u:user={
 name:"david"
}

let model:FileUploaderModel={
 accept:[""],
 descriptionType:1,
 existingFiles:[],
 lastModification: new Date(),
 maxSize:0,
 multiple:true
}

</script>

<h1>Welcome to SvelteKit {u.name}</h1>

<ListView></ListView>
<FileUploader data = {model}  />

<h1>Hello and welcome to my site!</h1>
<a href="/about">About my site</a>