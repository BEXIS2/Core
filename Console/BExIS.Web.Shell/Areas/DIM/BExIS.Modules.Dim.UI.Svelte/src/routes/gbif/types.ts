export interface GbifPublicationModel {
	publicationId: number;
	datasetVersionId: number;
	datasetVersionNr: number;
	datasetId: number;
	brokerRef: number;
 repositoryRef: number
	title: string;
	status: string;
	response: string;
	link: string;
	type: string;
}
