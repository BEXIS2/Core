
export function removeJsonPathIndices(path) {
			// Matches a dot followed by one or more digits
			// The '\b' ensures we only match whole numbers, not numbers embedded in words
			return path.replace(/\.\d+\b/g, '');
}

export function getParentPath(path) {
			if (typeof path !== 'string' || !path.includes('.')) {
        return ''; // Return empty if there's no dot to remove
    }

    // Find the position of the very last dot
    const lastDotIndex = path.lastIndexOf('.');

    // Slice the string from the start up to that last dot
    return path.substring(0, lastDotIndex);
}

export function getPartyIdFromParent(path) {
			
  // get party id from parent path, which is the last number in the path

  

}

