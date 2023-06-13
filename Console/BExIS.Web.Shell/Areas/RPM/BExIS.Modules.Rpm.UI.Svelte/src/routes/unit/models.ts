export interface UnitListItem {
 id: number;
 name: string;
 description:string;
 abbreviation: string;
 dimension: DimensionListItem;
 datatypes: DataTypeListItem[];
 measurementSystem: string;
}

export interface DataTypeListItem {
    id: number;
    name: string;
    description: string;
    systemType: string;
}

export interface DimensionListItem{
    id: number;
    name: string;
}