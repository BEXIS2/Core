import { marked } from 'marked';
import matter from "gray-matter";
import { Buffer } from 'buffer'; // Import the Buffer polyfill

// Ensure Buffer is available globally
if (typeof window !== 'undefined') {
  window.Buffer = Buffer;
}

export { loadMDFiles };
export interface Metadata {
    title: string;
};

interface Md {
    metadata: Metadata;
    content: string;
}

let slug = '';



async function loadMDFiles(repoUrl) {
    // const repoUrl = 'https://api.github.com/repos/BEXIS2/Documents/contents/Docs';
    const response = await fetch(repoUrl);
    const files = await response.json();
    const allHeadings: string[][] = []

    const entries = await Promise.all(files.map(async (file: any) => {
        if (file.download_url) {
            const fileResponse = await fetch(file.download_url);
            const content = await fileResponse.text();

            slug = file.name.replace('.md', '');
            // Extract metadata and content from the Markdown file
            const metadata = extractMetadata(content);
            allHeadings.push(metadata.headings);


            return {
                slug,
                ...metadata,
                content
            };
        } else {
            console.error('File has no download URL:', file);
            return null;
        }
    }));
    const data = await entries.filter(entry => entry !== null);
    return { data, allHeadings};
}

function extractMetadata(content_: string): { headings: string[] } {
   // const { data, content: markdownContent } = matter(content);

    // Extract headings using marked
    const headings: string[] = [];
    const renderer = new marked.Renderer();
    const {  data, content } = matter(content_);

    renderer.heading = (text, level) => {
        text.base = slug

        text.metadata = data;
        headings.push(text);
        return `<h${level}>${text}</h${level}>`;
    };
    marked(content, { renderer });

    // Parse metadata using gray-matter


    return {  headings };
}




function extractMetadata2(content: string): Metadata {
    // Implement metadata extraction logic here
    // For example, using front-matter or similar library
    return {
        title: 'Example Title' // Replace with actual metadata extraction
    };
}
