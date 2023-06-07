export interface UnitListItem {
 id: number;
 name: string;
 description:string;
 abbreviation: string;
 dimension: DimensionItem;
 datatypes: DataTypeListItem[];
 measurementSystem: string;
}

export interface DataTypeListItem {
    id: number;
    name: string;
    description: string;
    systemType: string;
}

interface DimensionItem{
    id: number;
    name: string;
}