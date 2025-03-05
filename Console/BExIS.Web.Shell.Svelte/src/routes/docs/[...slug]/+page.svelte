<script lang="ts">
	import sanitizeHtml from 'sanitize-html'; // Sanitize HTML
	import { marked } from 'marked'; // Convert Markdown to HTML
	export let data;
    import matter from "gray-matter";
	import { onMount } from 'svelte';

	import Fa from 'svelte-fa';
	import { faSave } from '@fortawesome/free-solid-svg-icons';

	import {
		Page,
		notificationType,
		notificationStore,
		helpStore,
		pageContentLayoutType
	} from '@bexis2/bexis2-core-ui';
	import { ListBox, ListBoxItem } from '@skeletonlabs/skeleton';

    onMount(() => {
        // toogle visibility of headings
        const hash = window.location.hash;
        if (hash) {
            const anchor = hash.replace('#', '');
            const index = headings.findIndex((heading) => heading.text.toLowerCase().replace(/\s+/g, '-').replace(/[^a-z0-9\-]/g, '') === anchor);
            if (index !== -1) {
                toggleVisibility(index);
            }
        }

    });


	// Custom Renderer for Marked
	const renderer = new marked.Renderer();
	// Remote Repo URL (e.g., GitHub)
	const repoRawUrl = 'https://github.com/BEXIS2/Documents/raw/master/Manuals/DDM/';
	// Fix image URLs by prefixing the repository URL
	renderer.image = ({ href, title, text }) => {
		const url = String(href); // Ensure href is a string
		console.log(url);
		// If URL is absolute (http/https), use it directly; else prefix with repo URL
		//const imageUrl = /^https?:\/\//.test(url) ? url : `${repoRawUrl}${url}`;
		console.log(url);
		return `<img src="${url}" alt="${text}" title="${
			title || ''
		}" loading="lazy" style="max-width: 100%; height: auto;">`;
	};
	renderer.heading = ({ tokens, depth }) => {
		const text = tokens.map((token) => token.raw).join('');
		const level = depth;
		// Sanitize the text to be URL-friendly
		let anchor = text
			.toLowerCase()
			.replace(/\s+/g, '-')
			.replace(/[^a-z0-9\-]/g, ''); // remove unwanted characters


		return `<h${level} id="${anchor}">${text}</h${level}>`;
	};
    renderer.blockquote = ({text, tokens}) => {
console.log(text);
console.log(tokens);

const quote = text.trim();
console.log(quote);
      if (quote && text.startsWith("[!SETTING]")) {

        return `<blockquote class="not-prose border-l-4 p-4 bg-surface-200">${marked.parseInline(tokens[0].tokens[1].raw.replace("[!SETTING]", "").trim())}${marked.parseInline(tokens[0].tokens[3].raw.replace("[!SETTING]", "").trim())}${marked.parseInline(tokens[0].tokens[2].raw.trim())}</blockquote>`;

      }
      if (quote && quote.startsWith("[!WARNING]")) {
        return `<blockquote class="warning">${quote.replace("[!WARNING]", "").trim()}</blockquote>`;
      }
      if (quote && quote.startsWith("[!ERROR]")) {
        return `<blockquote class="error">${quote.replace("[!ERROR]", "").trim()}</blockquote>`;
      }
      return `<blockquote class="not-prose border-l-4 p-4 bg-surface-200">${marked.parseInline(quote)}</blockquote>`;
    }

	console.log(data);

    // Sort the headings by depth
    data.allHeadings = data.allHeadings.sort((a, b) => a[0].metadata.position - b[0].metadata.position);

    // remove metadata from content
    const {  content } = matter(data.content);
    data.content = content;

	// Extract headings from data
	const headings = data.allHeadings.flat().map((heading) => ({
		text: heading.text,
		level: heading.depth,
		isVisible: true,
		base: heading.base
	}));

    // remove all headings where headings text include "position" and "title"
    headings.forEach((heading, index) => {
        if (heading.text.includes("position") || heading.text.includes("title")) {
            headings.splice(index, 1);
        }
    });

	// Hide all headings lower than level 3
	for (let i = 0; i < headings.length; i++) {
		if (headings[i].level > 2) {
			headings[i].isVisible = false;
		}
	}

	function toggleVisibility(index) {
			// scroll to anchor
            const anchor = headings[index].text
			.toLowerCase()
			.replace(/\s+/g, '-')
			.replace(/[^a-z0-9\-]/g, '');

		// Update the URL hash
		console.log(window.location.pathname);
		console.log(headings[index].base);
		if (!window.location.pathname.includes(headings[index].base)) {
			console.log('here');
			window.location.href = '/docs/' + headings[index].base + '#' + anchor;
            return;
		} else {
			window.location.hash = anchor;
		}


        const currentLevel = headings[index].level;
		const isVisible = headings[index].isVisible;
		console.log(currentLevel);
		console.log(isVisible);
		//headings[index].isVisible = !isVisible;
		if (currentLevel < 2) {
			return;
		}
		for (let i = index + 1; i < headings.length; i++) {
			if (headings[i].level > currentLevel) {
				headings[i].isVisible = !headings[i].isVisible;
			} else {
				break;
			}
		}

		// location.hash = anchor;
	}
</script>

<Page title="help" contentLayoutType={pageContentLayoutType.full}>
	<div class="left-nav flex mr-4 border-r-2">
		<nav>
			<ul>
				{#each headings as heading, index}
					<li
						style="margin-left: {heading.level * 10}px; display: {heading.isVisible
							? 'block'
							: 'none'};"
						class={+heading.level === 1 ? 'text-primary-700 text-2xl font-semibold mt-10 ' : ' text-xl mt-3 '}{+heading.level < 3  ? ' font-semibold' : 'text-lg'}
					>
						<a
							href="#{heading.text
								.toLowerCase()
								.replace(/\s+/g, '-')
								.replace(/[^a-z0-9\-]/g, '')}"
							on:click|preventDefault={() => toggleVisibility(index)}
						>
							{heading.text}
						</a>
					</li>
				{/each}
			</ul>
		</nav>
	</div>

	<div>
		<div class="content">
			<!-- using the content -->
			<div class="w-full h-full variant-filled-secondary-secondary text-justify">
				<div class="prose prose-slate lg:prose-lg max-w-none ml-7 pl-5 pr-10">
					{@html sanitizeHtml(
						marked(data.content, {
							renderer
						}),
						{
							allowedTags: [
								'b',
								'i',
								'em',
								'strong',
								'a',
								'p',
								'img',
								'h1',
								'h2',
								'h3',
								'h4',
								'h5',
								'h6',
								'ul',
								'ol',
								'li',
								'code',
								'pre',
								'br',
								'blockquote',
								'hr'
							],
							allowedAttributes: {
								a: ['href', 'title'], // Allow 'href' and 'title' attributes on anchor tags
								img: ['src', 'alt', 'title', 'loading'], // Allow 'src', 'alt', 'title', 'loading' on images
								'*': ['id', 'class'] // Allow `id` attributes for all elements
							}
						}
					)}
				</div>
			</div>
		</div>
	</div></Page
>

<style>
	.container {
		display: flex;
		flex-direction: row;
		height: 100vh; /* Ensure the container takes the full height of the viewport */
	}

	.left-nav {
		position: fixed;
		width: 300px; /* Adjust the width as needed */
		height: 100%; /* Ensure the left nav takes the full height */
		overflow-y: auto; /* Add scroll if content overflows */
	}

	.content {
		margin-left: 300px; /* Same as the width of the left-nav */
		padding: 20px;
		flex-grow: 1;

	}

	a {
		display: inline-block; /* Ensure anchors do not span full width */
		max-width: 100%; /* Prevent anchors from exceeding the width of their container */
		word-wrap: break-word; /* Break long URLs */
	}

	.markdown-container {
		max-width: 800px; /* Set a max-width for the container */
		margin: 0 auto; /* Center the container */
	}

    .note-blockquote {
    border-left: 4px solid #0078d4; /* Blue left border */
    padding: 1em;
    position: relative;
}
</style>
