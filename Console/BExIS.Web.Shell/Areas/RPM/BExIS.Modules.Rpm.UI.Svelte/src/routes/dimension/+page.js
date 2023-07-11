import { setApiConfig } from '@bexis2/bexis2-core-ui';

export function load() {
	if (import.meta.env.DEV) {
		setApiConfig('https://localhost:44345', '*', '*');
	}

	return {};
}
