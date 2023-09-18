import type { listItemType } from '@bexis2/bexis2-core-ui';

export interface DataStructureModel {
	id: number;
	title: string;
	description: string;
	linkedTo: number[];
}

export interface DataStructureCreationModel {
	entityId: number;
	title: string;
	description: string;
	file: string;
	delimeter: number;
	decimal: number;
	textMarker: number;
	delimeters: listItemType[];
	decimals: listItemType[];
	textMarkers: listItemType[];
	preview: string[];
	total: number;
	skipped: number;
	markers: markerType[];
	variables: VariableInstanceModel[];
	missingValues: missingValueType[];
}

export interface missingValueType {
	displayName: string;
	description: string;
}

export interface markerType {
	type: string;
	row: number;
	cells: boolean[];
}

class VariableModel{
				id: number;
    name: string;
    description: string;
    systemType: string;
    dataType: listItemType| undefined;
    unit: unitListItemType| undefined;;
    displayPattern: listItemType | undefined;
				missingValues: listItemType[];
				approved: boolean;

				public constructor() {
					this.id = 0
					this.name = ""
					this.description = ""
					this.systemType = ""
					this.dataType = undefined//{id:0,text:"",group:""}
					this.unit = undefined
					this.displayPattern = undefined
					this.missingValues = [];
					this.approved = false;
				}
}


export class VariableTemplateModel extends VariableModel{

	public constructor() {
		super()
	}
}

export class VariableInstanceModel extends VariableModel {

	template: listItemType;
	isKey: boolean;
	isOptional: boolean;
	possibleUnits: listItemType[];
	possibleTemplates: listItemType[];
	possibleDisplayPattern: listItemType[];


	public constructor() {
		super()
		this.isOptional = false
		this.template = {id:0,text:"",group:""}
		this.isKey = false
		this.possibleUnits = []
		this.possibleTemplates = []
		this.possibleDisplayPattern = []
	}
}

export interface unitListItemType extends listItemType {
		dataTypes:string[]	
}
