import { loadMDFiles } from './md.js';
import { get, writable } from 'svelte/store';
const store = writable({});
export const prerender = false; // Disable prerendering for this page

let md;

export const load = async ({ params }) => {
	const storedData = localStorage.getItem('mdData');
	if (storedData) {
		md = JSON.parse(storedData);
		store.set(md);
	} else {
		// Check if store is not set
		const storeValue = get(store);
		if (Object.keys(storeValue).length > 0) {
			md = storeValue;
		} else {
			// Load md files
			md = await loadMDFiles('https://api.github.com/repos/BEXIS2/Documents/contents/Docs');
			// Save to store and localStorage
			store.set({ md });
			// localStorage.setItem('mdData', JSON.stringify(md));
		}
	}

	const param = params.slug.toLowerCase();
	console.log(param);
	//const post = md.data.find((post) => post.slug.toLowerCase() + "/" === params.slug.toLowerCase());

	const post: { data?: any; allHeadings?: any } = {};
	post.data = md.allContent;
	post.allHeadings = md.allHeadings;
	console.log(post);
	return post;
};
