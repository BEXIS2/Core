<script lang="ts">
	import { Api } from '@bexis2/bexis2-core-ui';

	export let card: {
		title: string;
		description: string;
		author: string;
		license: string;
		id: string;
		doi: string;
		entity: string;
		date: string;
	} = { title: '', description: '', author: '', license: '', id: '', doi: '', entity: '', date:'' };

	const { title, description, author, license, id, doi, entity, date } = card;

	// let author = '';
	// let authorsLabel = 'Authors';

	// const getAuthorText = (author: { firstName: string; initials: string; familyName: string }) =>
	// 	`${author.familyName}, ${author.initials.length > 0 ? author.initials : ''} ${author.firstName[0] + '.'}`;

	// // TODO: Ideally, we will get author info in bulk with the datasets info.
	// // Currently, we are fetching author info for each dataset individually, which is highly inefficient.
	// const getAuthors = async () => {
	// 	try {
	// 		const res = await fetch('/api/metadata/' + card?.id + '?simplifiedJson=1', {
	// 			headers: {
	// 				Accept: 'application/json',
	// 				'Content-Type': 'application/json'
	// 			}
	// 		});

	// 		if (!res.ok) {
	// 			throw new Error('Failed to fetch authors');
	// 		}

	// 		const data = await res.json();

	// 		const authorsData = data.generalInformation.authors;
	// 		const authors: { firstName: string; initials: string; familyName: string }[] = [];

	// 		for (let i = 0; i < authorsData.length; i++) {
	// 			authors.push({
	// 				firstName: authorsData[i].firstName['#text'],
	// 				initials: authorsData[i].initials['#text'],
	// 				familyName: authorsData[i].familyName['#text']
	// 			});
	// 		}

	// 		if (authors.length > 2) {
	// 			author = `${getAuthorText(authors[0])} et al.`;
	// 		} else if (authors.length === 2) {
	// 			author = `${getAuthorText(authors[0])} & ${getAuthorText(authors[1])}`;
	// 		} else if (authors.length === 1) {
	// 			author = getAuthorText(authors[0]);
	// 			authorsLabel = 'Author';
	// 		}
	// 	} catch (e) {
	// 		console.error(e);
	// 	}
	// };

	// $: getAuthors();
</script>

<div class="flex grow">
	{doi} {date}
	<div
		class="p-4 px-5 border rounded-md bg-neutral-50 border-neutral-200 grow cursor-pointer hover:border-primary-500"
		on:click={() => window.open(`/ddm/data/Showdata/${id}`)}
		on:keydown={() => window.open(`/ddm/data/Showdata/${id}`)}
		role="link"
		tabindex="0"
	>
		<div class="flex flex-col w-full gap-4">
			<div class="justify-between flex gap-2">
				<h1 class="text-xl font-semibold grow">
					{#if title && title.length > 0} {title} {:else} No title {/if}</h1>
				<p class="shrink">
					{#if date && date.length > 0} {date} {/if}
				</p>
			</div>

			<p class="text-sm line-clamp-3">
				{#if description && description.length > 0} {description} {:else} No description {/if}
			</p>
			{#if author.length > 0}
				<div class="flex gap-2 items-center">
					<span class="font-semibold">Author:</span>
					<p class="text-sm italic text-neutral-600">{author}</p>
				</div>
			{/if}

			<div class="flex">
				<div class="w-auto">
					{#if license && license.length > 0}
						<div class="rounded-full px-3 p-1 items-center flex bg-neutral-200">
							<p class="text-sm">{license.replace('Open - ', '')}</p>
						</div>
					{/if}
				</div>
				<div class="ml-auto flex gap-2">
		
					{#if entity && entity.length > 0}
						<div class="rounded-full px-3 p-1 bg-primary-500">
							<span class="text-sm font-semibold text-on-secondary-token"
								>{entity.toLowerCase()}</span
							>
						</div>
					{/if}


					{#if doi && doi.length > 0} 
					<div class="rounded-full px-3 p-1 items-center flex bg-secondary-500">
						<span class="text-sm font-semibold text-on-secondary-token">
							doi
						</span>
					</div>
					{/if}
				</div>
			</div>
		</div>
	</div>
</div>
