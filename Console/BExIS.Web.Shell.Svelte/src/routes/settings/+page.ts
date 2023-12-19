import {setApiConfig} from '@bexis2/bexis2-core-ui';


/** @type {import('./$types').PageLoad} */
export async function load() {

	if (import.meta.env.DEV) {
		// setApiConfig('https://dev.bexis2.uni-jena.de/', 'Admin', '123456');
		setApiConfig('https://localhost:44345/', 'sventhiel', 'proq3dm6');

	}

	return {};
}