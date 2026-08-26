export interface ViewChoiceOption {
	key: string;
	value: string;
	display: string;
}

export function getViewChoiceOptions(choiceComponent: any, path: string): ViewChoiceOption[] {
	const options: ViewChoiceOption[] = [];

	if (!choiceComponent?.oneOf) {
		return options;
	}

	choiceComponent.oneOf.forEach((entry: any) => {
		const properties = entry?.properties ?? {};
		Object.entries(properties).forEach(([key, property]: [string, any]) => {
			const refValue = property?.['$ref'] ?? '';
			const display = refValue.split('/').filter(Boolean).pop() ?? key;
			const branchPath = path ? `${path}.${key}` : key;

			options.push({
				key,
				value: branchPath,
				display
			});
		});
	});

	return options;
}

export function resolveViewModeSelection(
	choiceComponent: any,
	path: string,
	hasValueAtPath: (branchPath: string) => boolean
): string {
	const options = getViewChoiceOptions(choiceComponent, path);

	for (const option of options) {
		const branchPath = option.value;
		if (hasValueAtPath(branchPath)) {
			return option.key;
		}
	}

	return '';
}
