export const editHelp = [
	{
		id: 'name',
		name: 'Name',
		description: 'Enter a unquie name for the entity template.'
	},
	{
		id: 'entityType',
		name: 'Entity',
		description: 'This template belongs to wich entity type?'
	},
	{
		id: 'description',
		name: 'Description',
		description: 'Template description help other people to choose the correct one.'
	},
	{
		id: 'metadataFields',
		name: 'Metadata input fields',
		description:
			'Set the minimum required informations for your Subject.' +
			'The user need to enter this field before the creations go. ' +
			'the items (systemkeys) in the list need to map against the selected metadata structure.' +
			'Thats why the are grouped in mapped and unmapped'
	},
	{
		id: 'invalidSaveMode',
		name: 'Invalid Save Mode',
		description:
			'if the user should be able to save invalid metadata, this option need to be activated.'
	},
	{
		id: 'permissions',
		name: 'Permissions',
		description: 'Select the groups that will automatically get access after creating the subject.'
	},
	{
		id: 'notification',
		name: 'Notification',
		description: 'Select the groups to be notified when something is changed.'
	},
	{
		id: 'disabledHooks',
		name: 'Disabled Hooks',
		description: 'Select features that should not be displayed during editing and display.'
	},
	{
		id: 'allowedFileTypes',
		name: 'Allowed file types',
		description: 'Select features that should not be displayed during editing and display.'
	},
	{
		id: 'hasDatastructure',
		name: 'Options for datastructures',
		description:
			'<u>1. active vs not active</u>' +
			'<ul class="ul list-disc p-5"><li><b>active datastructure</b> - tabular data will be uploaded</li>' +
			'<li><b>deactivate</b> - files will be uploaded directly and will not be processed furtherSelect features that should not be displayed during editing and display.</li></ul>' +
			'<u>2. selection or not</u>' +
			'<ul class="ul list-disc p-5"><li>if you activate datastructure, you can define a selection of datastructures from the system.</li>' +
			'<li>selected - only these structures can be used later on,	not selected - new structures can be selected.</li></ul>'
	}
];
