// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import { host } from '@bexis2/bexis2-core-ui';

// go to a internal action
export const goTo = async (url) => {
	window.open(host + url, '_self').focus();
};
