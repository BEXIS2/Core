import { setApiConfig } from '@bexis2/bexis2-core-ui';

/** @type {import('./$types').PageLoad} */
export function load() {
	if (import.meta.env.DEV) {
		//console.log('dev');
		setApiConfig('https://localhost:44345', 'davidschoene', '123456');
	}

	return {};
}
