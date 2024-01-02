// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import { host } from '@bexis2/bexis2-core-ui';

// go to a internal action
export const goTo = async (url, intern = true) => {
	if (intern == true) {
		// go to inside bexis2
		if (window != null && host != null && url != null) {
			window.open(host + url, '_self')?.focus();
		}
	} // go to a external page
	else {
		window.open(url, '_blank')?.focus();
	}
};
