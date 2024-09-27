<script lang="ts">
	export let card: {
		title: string;
		description: string;
		author: string;
		license: string;
		id: string;
	} = { title: '', description: '', author: '', license: '', id: '' };

	const { title, description, author, license, id } = card;
</script>

<div class="flex grow">
	<div
		class="p-4 px-5 border rounded-md bg-surface-100 border-surface-500 min-w-[500px] max-w-[800px] grow cursor-pointer hover:border-primary-500"
		on:click={() => window.open(`/ddm/data/Showdata/${id}`)}
		on:keydown={() => window.open(`/ddm/data/Showdata/${id}`)}
		role="link"
		tabindex="0"
	>
		<div class="flex flex-col w-full">
			<h1 class="text-xl font-semibold">{title}</h1>
			<p class=" line-clamp-3">
				{description}
			</p>
			<div class="flex items-center justify-between w-full">
				<p class="text-sm">{license}</p>
				<p class="text-sm">By <span class="font-semibold">{author}</span></p>
			</div>
		</div>
	</div>
</div>
