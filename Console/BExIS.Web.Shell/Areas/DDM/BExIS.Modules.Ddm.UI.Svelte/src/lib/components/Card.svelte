<script lang="ts">
	export let card: {
		title: string;
		description: string;
		author: string;
		license: string;
	} = { title: '', description: '', author: '', license: '' };

	const { title, description, author, license } = card;
</script>

<div class="flex grow">
	<div class="p-4 px-5 border rounded-md bg-surface-100 border-surface-500 w-[500px]">
		<div class="flex flex-col">
			<h1 class="text-xl font-semibold">{title}</h1>
			<p class="prose text-justify">{description}</p>
			<div class="flex items-center justify-between w-full">
				<p class="text-sm">{license}</p>
				<p class="text-sm">By <span class="font-semibold">{author}</span></p>
			</div>
		</div>
	</div>
</div>
