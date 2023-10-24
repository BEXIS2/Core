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
	id:number;
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
				missingValues: missingValueType[];
				meanings: listItemType[];
				approved: boolean;
				inUse: boolean;

				public constructor() {
					this.id = 0
					this.name = ""
					this.description = ""
					this.systemType = ""
					this.dataType = undefined//{id:0,text:"",group:""}
					this.unit = undefined
					this.missingValues = [];
					this.meanings = []
					this.approved = false;
					this.inUse = false;
				}
}


export class VariableTemplateModel extends VariableModel{

	public constructor() {
		super()
	}
}

export class VariableInstanceModel extends VariableModel {

	template: templateListItemType| undefined;
	isKey: boolean;
	isOptional: boolean;
	displayPattern: listItemType | undefined;
	possibleUnits: unitListItemType[];
	possibleTemplates: templateListItemType[];
	possibleDisplayPattern: listItemType[];

	public constructor() {
		super()
		this.isOptional = false
		this.template = undefined
		this.isKey = false
		this.displayPattern = undefined
		this.possibleUnits = []
		this.possibleTemplates = []
		this.possibleDisplayPattern = []
	}
}

export interface unitListItemType extends listItemType {
		dataTypes:string[]	
}

export interface templateListItemType extends listItemType {
	dataTypes:string[]
	units:string[]	
}