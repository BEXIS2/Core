// --- The function getContrastColor was generated using Copilot with GPT-4.1 ---
// It calculates the contrast color (black or white) for a given hex color and
// is used for the custom labels, where the curator can set custom colors,
// creating the necessary visual distinction.
export function getContrastColor(hex: string | undefined) {
	if (!hex) return '#000000';
	// Remove hash if present
	hex = hex.replace('#', '');
	// Expand shorthand form (e.g. "03F") to full form ("0033FF")
	if (hex.length === 3) {
		hex = hex
			.split('')
			.map((x) => x + x)
			.join('');
	}
	const r = parseInt(hex.substring(0, 2), 16);
	const g = parseInt(hex.substring(2, 4), 16);
	const b = parseInt(hex.substring(4, 6), 16);
	// Calculate luminance
	const luminance = 0.299 * r + 0.587 * g + 0.114 * b;
	return luminance > 186 ? '#000000' : '#ffffff';
}
// ------------------------------------------------------------------------------
