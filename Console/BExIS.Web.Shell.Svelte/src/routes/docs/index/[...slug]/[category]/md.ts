import { marked } from 'marked';
import matter from 'gray-matter';
import { Buffer } from 'buffer'; // Import the Buffer polyfill

// Ensure Buffer is available globally
if (typeof window !== 'undefined') {
	window.Buffer = Buffer;
}

export { loadMDFiles };

let slug = '';

async function loadMDFiles(repoUrl) {
	// const repoUrl = 'https://api.github.com/repos/BEXIS2/Documents/contents/Docs';
	const response = await fetch(repoUrl);
	const files = await response.json();
	const allHeadings: [] = [];
	const allContent: string[][] = [];
	await Promise.all(
		files.map(async (file: any) => {
			if (file.download_url) {
				const fileResponse = await fetch(file.download_url);
				const content = await fileResponse.text();

				slug = file.name.replace('.md', '');
				// Extract metadata and content from the Markdown file
				const { headings, content_md } = extractMetadata(content);
				console.log(headings);
				allHeadings.push(headings);
				allContent.push(content_md);

				return {
					slug,
					content
				};
			} else {
				console.error('File has no download URL:', file);
				return null;
			}
		})
	);
	//const data = await entries.filter(entry => entry !== null);
	//console.log(allContent);
	console.log(allHeadings);

	return { allContent, allHeadings };
}

function extractMetadata(content_: string): { headings: string[]; content_md: string[] } {
	// Extract metadata using gray-matter
	// const { data, content: markdownContent } = matter(content);

	// Extract headings using marked
	const headings = [];
	const renderer = new marked.Renderer();
	const { data, content } = matter(content_);

	renderer.heading = (text) => {
		text.base = data.title;
		text.metadata = data;
		headings.push(text);
		//return `<h${level}>${text}</h${level}>`;
	};
	marked(content, { renderer });
	// Parse metadata using gray-matter

	// content.metadata = data;
	// console.log(content.metadata);
	return { headings: headings, content_md: [content, data] };
}
