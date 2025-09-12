export const editHelp = [
	{
		id: 'name',
		name: 'Template Name',
		description: 'Enter a unique name for the entity template.'
	},
	{
		id: 'entityType',
		name: 'Entity Type',
		description: 'This template belongs to which type of entity?'
	},
	{
		id: 'description',
		name: 'Description',
		description: 'Template description helps other people to choose the correct one.'
	},
	{
		id: 'metadataStructure',
		name: 'Metadata Schema',
		description: 'Metadata schema the dataset is based on.'
	},
	{
		id: 'metadataFields',
		name: 'Metadata input fields',
		description:
			'Set the minimum required information for your dataset.' +
			'The user must fill this field(s) before the creation of a dataset can start. ' +
			'The listed items (system keys) must be mapped against the selected metadata schema.' +
			'That is why they are grouped in mapped and unmapped.'
	},
	{
		id: 'invalidSaveMode',
		name: 'Invalid Save Mode',
		description:
			'If the user should be able to save metadata with empty required fields, this option needs to be activated.'
	},
	{
		id: 'permissions',
		name: 'Permissions',
		description:
			'Select the group(s) that will automatically get full access after creating the dataset.'
	},
	{
		id: 'notification',
		name: 'Notification',
		description: 'Select the group(s) to be notified when something is changed.'
	},
	{
		id: 'disabledHooks',
		name: 'Disable dataset components',
		description:
			'Select components/features that should not be displayed during editing and display of a dataset.'
	},
	{
		id: 'allowedFileTypes',
		name: 'Restrict allowed file type for upload',
		description: 'Select file types that are allowed during file upload.'
	},
	{
		id: 'hasDatastructure',
		name: 'Options for data structures',
		description:
			'<u>1. active vs not active</u>' +
			'<ul class="ul list-disc p-5"><li><b>active data structure</b> - tabular data will be uploaded</li>' +
			'<li><b>deactivate</b> - files will be uploaded directly and will not be processed further. Select features that should not be displayed during editing and display.</li></ul>' +
			'<u>2. selection or not</u>' +
			'<ul class="ul list-disc p-5"><li> If you activate data structure, you can define a selection of data structures from the system.</li>' +
			'<li>selected - only these structures can be used later, not selected - new structures can be selected.</li></ul>'
	},
	{
		id: 'activated',
		name: 'Visibility of entity templates when creating entities',
		description:
			'Only the activated entity templates are visible to users and they can only create entities based on these.'
	}
];
