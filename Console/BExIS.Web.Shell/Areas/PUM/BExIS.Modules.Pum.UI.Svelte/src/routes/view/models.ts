export interface ResourceArray {
	//ID: number;
	Name: string;
	ResourceType: string;
	SubmissionDate: string; //Date???
	EmbargoEnd: string; //Date???
	URI: string;
	URIHealth: string; //boolean?
	DOI: string;
	DOIHealth: string; //boolean?
	RepositoryName: string;
	Licence: string;
	ProgrammingLanguage: string;
}

export enum types {
	uri = 'uri',
	doi = 'doi'
}

export interface LinkType {
	type: types;
}
