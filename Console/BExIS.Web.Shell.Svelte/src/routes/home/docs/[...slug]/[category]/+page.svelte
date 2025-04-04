<script lang="ts">
	import { onMount } from 'svelte';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { get, writable } from 'svelte/store';

	import {
		Page,
		notificationType,
		notificationStore,
		helpStore,
		pageContentLayoutType
	} from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';
	import { faBook } from '@fortawesome/free-solid-svg-icons';
	import { faTools } from '@fortawesome/free-solid-svg-icons';

	import sanitizeHtml from 'sanitize-html';
	import { marked } from 'marked';

	export let data;
	let content_complete = '';
	let headings = [];

	interface DataType {
		allHeadings: Array<{
			text: string;
			base: string;
			depth: number;
			metadata: { position: number };
		}>;
		data: Array<[string, { title: string }]>;
	}

	const store = writable<DataType>({ allHeadings: [], data: [] });
	// const headingStore = writable([]);

	let container;
	let version;
	$: version;

	onMount(() => {
		// get data from parent
		container = document.getElementById('docs');
		version = container?.getAttribute('version');

		// Sort the headings by position written in the md metadata
		data.allHeadings = data.allHeadings.sort(
			(a, b) => a[0].metadata.position - b[0].metadata.position
		);

		// Extract headings from data
		headings = data.allHeadings.flat().map((heading) => ({
			text: heading.text,
			level: heading.depth,
			isVisible: true,
			base: heading.base
		}));

		// Remove all headings where headings text include "position" and "title" (metadata)
		headings.forEach((heading, index) => {
			if (heading.text.includes('position') || heading.text.includes('title')) {
				headings.splice(index, 1);
			}
		});

		// Hide all headings lower than level 2
		for (let i = 0; i < headings.length; i++) {
			if (headings[i].level > 2) {
				headings[i].isVisible = false;
			}
		}
		console.log('headings');
		console.log(headings);
		// Toggle visibility of headings
		const hash = window.location.hash;
		if (hash) {
			const anchor = hash.replace('#', '');
			const index = headings.findIndex(
				(heading) =>
					heading.text
						.toLowerCase()
						.replace(/\s+/g, '-')
						.replace(/[^a-z0-9\-]/g, '') === anchor
			);
			if (index !== -1) {
				console.log('index');
				console.log(index);
				// console.log(headings[index]);
				toggleVisibility(index, true);
			}
		}

		// Store the data
		store.set(data);

		// Get slug parameter
		const params = $page.params;
		console.log('params', params);

		setContent(params.category);

		// Adjust layout and set height of sidebar and content
		adjustLayout();
		// Run on page load and whenever window resizes
		window.addEventListener('load', adjustLayout);
		window.addEventListener('resize', adjustLayout);

		// Add event listener to rendered content
		document.getElementById('content').addEventListener('click', handleLinkClick);
	});

	// Select and render the currently needed content
	async function setContent(base_path: string) {
		const data = get(store);
		console.log('🚀 ~ setContent ~ data:', data);
		for (let i = 0; i < data.data.length; i++) {
			console.log('base-path', base_path);
			console.log('data-title', data.data[i][1].title.toLowerCase());
			if (base_path.toLowerCase().includes(data.data[i][1].title.toLowerCase())) {
				console.log('here');
				content_complete = await marked(data.data[i][0], {
					renderer
				});
			}
		}
	}
	function extractFirstPart(text) {
		const regex = /\(([^)]+)\)/;
		const match = text.match(regex);
		if (match) {
			return match[1].split(',')[0].trim();
		}
		return null;
	}
	// Custom Renderer for Marked
	const renderer = new marked.Renderer();

	// Fix image URLs by prefixing the repository URL
	renderer.image = ({ href, title, text }) => {
		const url = String(href); // Ensure href is a string
		console.log(url);
		// If URL is absolute (http/https), use it directly; else prefix with repo URL
		//const imageUrl = /^https?:\/\//.test(url) ? url : `${repoRawUrl}${url}`;
		console.log(url);
		return `<img src="${url}" alt="${text}" title="${
			title || ''
		}" loading="lazy" style="width:100%">`;
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

	renderer.blockquote = ({ text, tokens }) => {
		// console.log(text);
		// console.log(tokens);

		const quote = text.trim();
		// console.log(quote);
		if (quote && text.startsWith('[!SETTING]')) {
			const svg =
				'<svg class="svelte-fa svelte-fa-base undefined" viewBox="0 0 512 512" aria-hidden="true" role="img" xmlns="http://www.w3.org/2000/svg"><g transform="translate(256 256)" transform-origin="128 0"><g transform="translate(0,0) scale(1,1)"><path d="M495.9 166.6c3.2 8.7 .5 18.4-6.4 24.6l-43.3 39.4c1.1 8.3 1.7 16.8 1.7 25.4s-.6 17.1-1.7 25.4l43.3 39.4c6.9 6.2 9.6 15.9 6.4 24.6c-4.4 11.9-9.7 23.3-15.8 34.3l-4.7 8.1c-6.6 11-14 21.4-22.1 31.2c-5.9 7.2-15.7 9.6-24.5 6.8l-55.7-17.7c-13.4 10.3-28.2 18.9-44 25.4l-12.5 57.1c-2 9.1-9 16.3-18.2 17.8c-13.8 2.3-28 3.5-42.5 3.5s-28.7-1.2-42.5-3.5c-9.2-1.5-16.2-8.7-18.2-17.8l-12.5-57.1c-15.8-6.5-30.6-15.1-44-25.4L83.1 425.9c-8.8 2.8-18.6 .3-24.5-6.8c-8.1-9.8-15.5-20.2-22.1-31.2l-4.7-8.1c-6.1-11-11.4-22.4-15.8-34.3c-3.2-8.7-.5-18.4 6.4-24.6l43.3-39.4C64.6 273.1 64 264.6 64 256s.6-17.1 1.7-25.4L22.4 191.2c-6.9-6.2-9.6-15.9-6.4-24.6c4.4-11.9 9.7-23.3 15.8-34.3l4.7-8.1c6.6-11 14-21.4 22.1-31.2c5.9-7.2 15.7-9.6 24.5-6.8l55.7 17.7c13.4-10.3 28.2-18.9 44-25.4l12.5-57.1c2-9.1 9-16.3 18.2-17.8C227.3 1.2 241.5 0 256 0s28.7 1.2 42.5 3.5c9.2 1.5 16.2 8.7 18.2 17.8l12.5 57.1c15.8 6.5 30.6 15.1 44 25.4l55.7-17.7c8.8-2.8 18.6-.3 24.5 6.8c8.1 9.8 15.5 20.2 22.1 31.2l4.7 8.1c6.1 11 11.4 22.4 15.8 34.3zM256 336a80 80 0 1 0 0-160 80 80 0 1 0 0 160z" fill="currentColor" transform="translate(-256 -256)"></path></g></g></svg>';
			const firstPart = extractFirstPart(quote);

			// check if the first part is not null
			if (!firstPart) {
				return `<blockquote class="not-prose border-l-4 p-4 bg-surface-200">${marked.parseInline(quote, { renderer })}</blockquote>`;
			}
			const secondPart = quote
				.replace(firstPart, '')
				.replace('[!SETTING]', '')
				.replace('(', '')
				.replace(')', '');
			return `<blockquote class="not-prose border-l-4 p-4 border-blue-500 bg-surface-300"><div class="flex"><div class="mr-4 mt-2">${svg}</div><div>${marked.parseInline(secondPart, { renderer })}</div><div class="ml-auto -mt-3 text-sm">${marked.parseInline(firstPart, { renderer })}</div></div></blockquote>`;
		}
		if (quote && text.startsWith('[!ROLE]')) {
			const svg =
				'<svg class="svelte-fa svelte-fa-base undefined svelte-bvo74f" viewBox="0 0 448 512" aria-hidden="true" role="img" xmlns="http://www.w3.org/2000/svg"><g transform="translate(224 256)" transform-origin="112 0"><g transform="translate(0,0) scale(1,1)"><path d="M224 256A128 128 0 1 0 224 0a128 128 0 1 0 0 256zm-45.7 48C79.8 304 0 383.8 0 482.3C0 498.7 13.3 512 29.7 512l388.6 0c16.4 0 29.7-13.3 29.7-29.7C448 383.8 368.2 304 269.7 304l-91.4 0z" fill="currentColor" transform="translate(-224 -256)"></path></g></g></svg>';
			return `<blockquote class="not-prose border-l-4 p-4 border-green-500 bg-surface-300"><div class="flex"><div class="mr-4 mt-2">${svg}</div><div>${marked.parseInline(quote.replace('[!ROLE]', ''), { renderer })}</div></div></blockquote>`;
		}
		if (quote && quote.startsWith('[!ERROR]')) {
			return `<blockquote class="error">${quote.replace('[!ERROR]', '').trim()}</blockquote>`;
		}
		return `<blockquote class="not-prose border-l-4 p-4 bg-surface-200">${marked.parseInline(quote, { renderer })}</blockquote>`;
	};

	renderer.link = ({ href, title, text }) => {
		// If URL is absolute (http/https), use it directly; else prefix with repo URL
		//const url = /^https?:\/\//.test(href) ? href : `${repoRawUrl}${href}`;
		// console.log(href);
		// console.log(text);
		const svg_add = '<svg class="svelte-fa svelte-fa-base undefined svelte-bvo74f inline" viewBox="0 0 448 512" aria-hidden="true" role="img" xmlns="http://www.w3.org/2000/svg"><g transform="translate(224 256)" transform-origin="112 0"><g transform="translate(0,0) scale(1,1)"><path d="M256 80c0-17.7-14.3-32-32-32s-32 14.3-32 32l0 144L48 224c-17.7 0-32 14.3-32 32s14.3 32 32 32l144 0 0 144c0 17.7 14.3 32 32 32s32-14.3 32-32l0-144 144 0c17.7 0 32-14.3 32-32s-14.3-32-32-32l-144 0 0-144z" fill="currentColor" transform="translate(-224 -256)"></path></g></g></svg>'
		const svg_edit = '<svg class="svelte-fa svelte-fa-base undefined svelte-bvo74f inline" viewBox="0 0 512 512" aria-hidden="true" role="img" xmlns="http://www.w3.org/2000/svg"><g transform="translate(256 256)" transform-origin="128 0"><g transform="translate(0,0) scale(1,1)"><path d="M471.6 21.7c-21.9-21.9-57.3-21.9-79.2 0L362.3 51.7l97.9 97.9 30.1-30.1c21.9-21.9 21.9-57.3 0-79.2L471.6 21.7zm-299.2 220c-6.1 6.1-10.8 13.6-13.5 21.9l-29.6 88.8c-2.9 8.6-.6 18.1 5.8 24.6s15.9 8.7 24.6 5.8l88.8-29.6c8.2-2.7 15.7-7.4 21.9-13.5L437.7 172.3 339.7 74.3 172.4 241.7zM96 64C43 64 0 107 0 160L0 416c0 53 43 96 96 96l256 0c53 0 96-43 96-96l0-96c0-17.7-14.3-32-32-32s-32 14.3-32 32l0 96c0 17.7-14.3 32-32 32L96 448c-17.7 0-32-14.3-32-32l0-256c0-17.7 14.3-32 32-32l96 0c17.7 0 32-14.3 32-32s-14.3-32-32-32L96 64z" fill="currentColor" transform="translate(-256 -256)"></path></g></g></svg>'
		const svg_view = '<svg class="svelte-fa svelte-fa-base undefined svelte-bvo74f inline" viewBox="0 0 576 512" aria-hidden="true" role="img" xmlns="http://www.w3.org/2000/svg"><g transform="translate(288 256)" transform-origin="144 0"><g transform="translate(0,0) scale(1,1)"><path d="M288 32c-80.8 0-145.5 36.8-192.6 80.6C48.6 156 17.3 208 2.5 243.7c-3.3 7.9-3.3 16.7 0 24.6C17.3 304 48.6 356 95.4 399.4C142.5 443.2 207.2 480 288 480s145.5-36.8 192.6-80.6c46.8-43.5 78.1-95.4 93-131.1c3.3-7.9 3.3-16.7 0-24.6c-14.9-35.7-46.2-87.7-93-131.1C433.5 68.8 368.8 32 288 32zM144 256a144 144 0 1 1 288 0 144 144 0 1 1 -288 0zm144-64c0 35.3-28.7 64-64 64c-7.1 0-13.9-1.2-20.3-3.3c-5.5-1.8-11.9 1.6-11.7 7.4c.3 6.9 1.3 13.8 3.2 20.7c13.7 51.2 66.4 81.6 117.6 67.9s81.6-66.4 67.9-117.6c-11.1-41.5-47.8-69.4-88.6-71.1c-5.8-.2-9.2 6.1-7.4 11.7c2.1 6.4 3.3 13.2 3.3 20.3z" fill="currentColor" transform="translate(-288 -256)"></path></g></g></svg>'
		const svg_delete = '<svg class="svelte-fa svelte-fa-base undefined svelte-bvo74f inline" viewBox="0 0 448 512" aria-hidden="true" role="img" xmlns="http://www.w3.org/2000/svg"><g transform="translate(224 256)" transform-origin="112 0"><g transform="translate(0,0) scale(1,1)"><path d="M135.2 17.7L128 32 32 32C14.3 32 0 46.3 0 64S14.3 96 32 96l384 0c17.7 0 32-14.3 32-32s-14.3-32-32-32l-96 0-7.2-14.3C307.4 6.8 296.3 0 284.2 0L163.8 0c-12.1 0-23.2 6.8-28.6 17.7zM416 128L32 128 53.2 467c1.6 25.3 22.6 45 47.9 45l245.8 0c25.3 0 46.3-19.7 47.9-45L416 128z" fill="currentColor" transform="translate(-224 -256)"></path></g></g></svg>'
		const svg_manage = '<svg class="svelte-fa svelte-fa-base undefined svelte-bvo74f inline" viewBox="0 0 512 512" aria-hidden="true" role="img" xmlns="http://www.w3.org/2000/svg"><g transform="translate(256 256)" transform-origin="128 0"><g transform="translate(0,0) scale(1,1)"><path d="M78.6 5C69.1-2.4 55.6-1.5 47 7L7 47c-8.5 8.5-9.4 22-2.1 31.6l80 104c4.5 5.9 11.6 9.4 19 9.4l54.1 0 109 109c-14.7 29-10 65.4 14.3 89.6l112 112c12.5 12.5 32.8 12.5 45.3 0l64-64c12.5-12.5 12.5-32.8 0-45.3l-112-112c-24.2-24.2-60.6-29-89.6-14.3l-109-109 0-54.1c0-7.5-3.5-14.5-9.4-19L78.6 5zM19.9 396.1C7.2 408.8 0 426.1 0 444.1C0 481.6 30.4 512 67.9 512c18 0 35.3-7.2 48-19.9L233.7 374.3c-7.8-20.9-9-43.6-3.6-65.1l-61.7-61.7L19.9 396.1zM512 144c0-10.5-1.1-20.7-3.2-30.5c-2.4-11.2-16.1-14.1-24.2-6l-63.9 63.9c-3 3-7.1 4.7-11.3 4.7L352 176c-8.8 0-16-7.2-16-16l0-57.4c0-4.2 1.7-8.3 4.7-11.3l63.9-63.9c8.1-8.1 5.2-21.8-6-24.2C388.7 1.1 378.5 0 368 0C288.5 0 224 64.5 224 144l0 .8 85.3 85.3c36-9.1 75.8 .5 104 28.7L429 274.5c49-23 83-72.8 83-130.5zM56 432a24 24 0 1 1 48 0 24 24 0 1 1 -48 0z" fill="currentColor" transform="translate(-256 -256)"></path></g></g></svg>'
		const svg_configure = '<svg class="svelte-fa svelte-fa-base undefined inline" viewBox="0 0 512 512" aria-hidden="true" role="img" xmlns="http://www.w3.org/2000/svg"><g transform="translate(256 256)" transform-origin="128 0"><g transform="translate(0,0) scale(1,1)"><path d="M495.9 166.6c3.2 8.7 .5 18.4-6.4 24.6l-43.3 39.4c1.1 8.3 1.7 16.8 1.7 25.4s-.6 17.1-1.7 25.4l43.3 39.4c6.9 6.2 9.6 15.9 6.4 24.6c-4.4 11.9-9.7 23.3-15.8 34.3l-4.7 8.1c-6.6 11-14 21.4-22.1 31.2c-5.9 7.2-15.7 9.6-24.5 6.8l-55.7-17.7c-13.4 10.3-28.2 18.9-44 25.4l-12.5 57.1c-2 9.1-9 16.3-18.2 17.8c-13.8 2.3-28 3.5-42.5 3.5s-28.7-1.2-42.5-3.5c-9.2-1.5-16.2-8.7-18.2-17.8l-12.5-57.1c-15.8-6.5-30.6-15.1-44-25.4L83.1 425.9c-8.8 2.8-18.6 .3-24.5-6.8c-8.1-9.8-15.5-20.2-22.1-31.2l-4.7-8.1c-6.1-11-11.4-22.4-15.8-34.3c-3.2-8.7-.5-18.4 6.4-24.6l43.3-39.4C64.6 273.1 64 264.6 64 256s.6-17.1 1.7-25.4L22.4 191.2c-6.9-6.2-9.6-15.9-6.4-24.6c4.4-11.9 9.7-23.3 15.8-34.3l4.7-8.1c6.6-11 14-21.4 22.1-31.2c5.9-7.2 15.7-9.6 24.5-6.8l55.7 17.7c13.4-10.3 28.2-18.9 44-25.4l12.5-57.1c2-9.1 9-16.3 18.2-17.8C227.3 1.2 241.5 0 256 0s28.7 1.2 42.5 3.5c9.2 1.5 16.2 8.7 18.2 17.8l12.5 57.1c15.8 6.5 30.6 15.1 44 25.4l55.7-17.7c8.8-2.8 18.6-.3 24.5 6.8c8.1 9.8 15.5 20.2 22.1 31.2l4.7 8.1c6.1 11 11.4 22.4 15.8 34.3zM256 336a80 80 0 1 0 0-160 80 80 0 1 0 0 160z" fill="currentColor" transform="translate(-256 -256)"></path></g></g></svg>';


		if (text.startsWith('[!LINK_VIEW]')) {
			return `<div class="inline-flex not-prose mt-2"><div class="mr-1 ">${svg_view}</div><a class="underline" href="${href}" title="${title || ''} ">${text.replace('[!LINK_VIEW]', '').trim()}</a></div>`;
		}
		if (text.startsWith('[!LINK_EDIT]')) {
			return `<div class="inline-flex not-prose"><div class="mr-1">${svg_edit}</div><a class="underline" href="${href}" title="${title || ''} ">${text.replace('[!LINK_EDIT]', '').trim()}</a></div>`;
		}
		if (text.startsWith('[!LINK_CREATE]')) {
			return `<div class="inline-flex not-prose"><div class="mr-1 ">${svg_add}</div><a class="underline" href="${href}" title="${title || ''} ">${text.replace('[!LINK_CREATE]', '').trim()}</a></div>`;
		}
		if (text.startsWith('[!LINK_DELETE]')) {
			return `<div class="inline-flex not-prose"><div class="mr-1 ">${svg_delete}</div><a class="underline" href="${href}" title="${title || ''} ">${text.replace('[!LINK_DELETE]', '').trim()}</a></div>`;
		}
		if (text.startsWith('[!LINK_MANAGE]')) {
			return `<div class="inline-flex not-prose"><div class="mr-1 ">${svg_manage}</div><a class="underline" href="${href}" title="${title || ''} ">${text.replace('[!LINK_MANAGE]', '').trim()}</a></div>`;
		}
		if (text.startsWith('[!LINK_CONFIGURE]')) {
			return `<div class="inline-flex not-prose"><div class="mr-1 ">${svg_configure}</div><a class="underline" href="${href}" title="${title || ''} ">${text.replace('[!LINK_CONFIGURE]', '').trim()}</a></div>`;
		}
		return `<a class="underline" href="${href}" title="${title || ''}">${text}</a>`;
	};

	async function toggleVisibility(index: number, init = false) {
		console.log(index);

		const anchor = headings[index].text
			.toLowerCase()
			.replace(/\s+/g, '-')
			.replace(/[^a-z0-9\-]/g, '');

		// Update the URL hash
		// console.log(window.location.pathname);
		// console.log(headings[index].base);
		if (!init) {
			// if the current page is not the same as the heading base, change the page
			if (!window.location.pathname.includes(headings[index].base)) {
				console.log('here');
				await setContent(headings[index].base);
				//goto('/home/docs/' + headings[index].base + '#' + anchor);

				document.querySelector('#' + anchor).scrollIntoView({
					behavior: 'smooth'
				});
				// change url, but do not reload
				window.history.pushState(null, '', '/home/docs/' + headings[index].base + '#' + anchor);
				// if the current page is the same as the heading base, change the anchor
			} else {
				//goto('#' + anchor);

				document.querySelector('#' + anchor).scrollIntoView({
					behavior: 'smooth'
				});
				window.history.pushState(null, '', '/home/docs/' + headings[index].base + '#' + anchor);
			}
		} else {
			// search for level 2 headings based on the current heading
			// level 2 before the current heading
			let level2 = 0;
			for (let i = index; i >= 0; i--) {
				if (headings[i].level === 2) {
					level2 = i;
					console.log('level2');
					console.log(level2);
					// index = level1;
					break;
				}
			}

			// level 2 after the current heading
			let level1 = 0;
			for (let i = index; i <= headings.length; i++) {
				if (headings[i].level === 2) {
					level1 = i;
					console.log('level1');
					console.log(level1);
					// index = level1;
					break;
				}
			}

			// show all headings between the two level 2 headings
			for (let i = level2; i <= level1; i++) {
				headings[i].isVisible = true;
			}

			window.history.pushState(null, '', '/home/docs/' + headings[index].base + '#' + anchor);
		}

		const currentLevel = headings[index].level;
		const isVisible = headings[index].isVisible;
		// console.log(currentLevel);
		// console.log(isVisible);
		//headings[index].isVisible = !isVisible;
		if (currentLevel < 2) {
			return;
		}
		// console.log(index);
		// console.log(headings.length);
		let last_index = 0;
		// Toggle visibility of headings
		for (let i = index + 1; i < headings.length; i++) {
			console.log(i);
			console.log(headings[i].level);
			if (headings[i].level > currentLevel) {
				console.log('here');
				headings[i].isVisible = !headings[i].isVisible;
			} else {
				last_index = i;
				break;
			}
		}
		window.history.pushState(null, '', '/home/docs/' + headings[index].base + '#' + anchor);
	}
	function adjustLayout() {
		const elementsAbove = document.querySelectorAll('.top-div, .header, #shell-header'); // Select all elements above sidebar
		console.log('elements above');
		console.log(elementsAbove);
		let lastElement = null;

		// Find the last non-empty element (with a height > 0)
		for (let i = elementsAbove.length - 1; i >= 0; i--) {
			if (elementsAbove[i].offsetHeight > 0) {
				lastElement = elementsAbove[i];
				break;
			}
		}

		// If no valid element is found, default to 0
		let lastElementBottom = lastElement
			? lastElement.getBoundingClientRect().bottom + window.scrollY
			: 0;

		//  let lastElement = elementsAbove[elementsAbove.length - 1]; // Get last one
		// console.log('last element');
		// console.log(lastElement);
		// console.log(lastElement.getBoundingClientRect());
		// let lastElementBottom = lastElement.getBoundingClientRect().bottom + window.scrollY; // Get bottom position

		const sidebar = document.getElementById('left-nav');
		const content = document.getElementById('content');

		sidebar.style.top = lastElementBottom + 'px'; // Move sidebar below last element
		sidebar.style.height = `calc(100vh - ${lastElementBottom + 50}px)`; // Adjust sidebar height

		//  content.style.marginTop = lastElementBottom + "px"; // Push content below last element
		content.style.height = `calc(100vh - ${lastElementBottom + 50}px)`; // Adjust content height
	}

	function handleLinkClick(event) {
		const target = event.target;

		if (target.tagName === 'A') {
			const href = target.getAttribute('href');
			// console.log(href);
			if (href && href.startsWith('../')) {
				// If the link is a relative link, navigate to it
				const anchor = href.split('#')[1];
				const base = href.split('#')[0].split('/')[2];
				const index = headings.findIndex(
					(heading) =>
						heading.text
							.toLowerCase()
							.replace(/\s+/g, '-')
							.replace(/[^a-z0-9\-]/g, '') === anchor && heading.base === base
				);
				if (index !== -1) {
					toggleVisibility(index);
					event.preventDefault();
				}
			}
		}
	}
</script>

<Page title="Docs" contentLayoutType={pageContentLayoutType.full} footer={false}>
	<div class="container">
		<!-- using the left navigation -->
		<div id="left-nav" class="left-nav mr-4">
			<div class="flex text-lg ml-4 mt-6">
				<div class="mt-1 mr-2"><Fa icon={faBook} /></div>
				<div>Documentation (v{version})</div>
			</div>
			<nav>
				<ul>
					{#each headings as heading, index}
						<li
							style="margin-left: {heading.level * 10 + 5}px; display: {heading.isVisible
								? 'block'
								: 'none'};"
							class="{+heading.level === 1
								? 'text-primary-700 text-xl font-semibold mt-4 '
								: ' text-base mt-1 '}{+heading.level < 3 ? ' font-semibold' : 'text-base'}"
						>
							<a
								href="/home/docs/{heading.base}#{heading.text
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

		<div id="content" class="content">
			<!-- using the content -->

			<div class="prose prose-slate lg:prose-lg max-w-none">
				{@html content_complete}
			</div>
		</div>
	</div>
</Page>

<style>
	.left-nav {
		position: fixed;
		left: 0;
		width: 300px;
		/*	height: calc(100vh - 180px); set vai function */
		overflow-y: auto;
		scrollbar-width: thin; /* Makes scrollbar smaller in Firefox */
		scrollbar-color: rgba(0, 0, 0, 0.3) transparent; /* Colors scrollbar */
	}

	.content {
		flex-grow: 1;
		overflow-y: auto;

		/*  height: calc(100vh - 180px); /* Subtracts header height set via function  */

		/* padding-top: 20px; */
		/* background: #f4f4f4; */
		width: calc(100% - 400px);
		margin-left: 300px;
		overflow-y: scroll; /* Allow scrolling */
		scrollbar-width: none; /* Hide scrollbar (Firefox) */
	}
	/*
	.test{
		max-width: calc(100% - 400px);
	} */

	a {
		display: inline-block; /* Ensure anchors do not span full width */
		max-width: 100%; /* Prevent anchors from exceeding the width of their container */
		word-wrap: break-word; /* Break long URLs */
	}
	/* Main content area */
	.container {
		display: flex;
		flex: 1;
		overflow: hidden; /* Prevents scrolling issues */
	}
	/* Customize scrollbar for Chrome, Edge, Safari */
	.left-nav::-webkit-scrollbar {
		width: 1px; /* Makes scrollbar thinner */
	}

	.left-nav::-webkit-scrollbar-thumb {
		background: rgba(0, 0, 0, 0.3); /* Darker for visibility */
		border-radius: 3px;
	}

	.left-nav::-webkit-scrollbar-thumb:hover {
		background: rgba(0, 0, 0, 0.5); /* Darker when hovered */
	}

	.content::-webkit-scrollbar {
		display: none; /* Hide scrollbar (Chrome, Edge, Safari) */
	}
</style>
