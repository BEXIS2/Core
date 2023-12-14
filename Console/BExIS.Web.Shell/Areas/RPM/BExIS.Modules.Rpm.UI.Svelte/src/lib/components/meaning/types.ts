import type { listItemType } from "@bexis2/bexis2-core-ui";

export class MeaningModel  {
    id:number;
    name: string;
    description: string;
    selectable: selectableType;
    approved: approvedType;
    externalLink: externalLinkType[];
    related_meaning: MeaningModel[];

    public constructor(data:any) {

        this.approved=2
        this.description=""
        this.selectable=2
        this.externalLink=[]
        this.related_meaning=[]

        if(data)
        {
            this.id = data.id,
            this.name = data.name
        }
        else
        {
            this.id=0
            this.name=""
        }
    }
}

export class externalLinkType  {
    id: number;
    uri: string;
    name: string;
    type: listItemType|undefined;
    prefix: prefixListItemType|undefined
    prefixCategory:prefixCategoryType|undefined

    public constructor() {

        this.id=0
        this.uri=""
        this.name=""
        this.type=undefined
        this.prefix=undefined
        this.prefixCategory=undefined
    }
}

export interface prefixListItemType  {
    id: number;
    text: string;
    description: string;
    url: string;
}

export interface prefixCategoryType  {
    id: number;
    name: string;
    description: string;
}

export enum approvedType {
    yes = 1,
    No = 2
}

export enum selectableType {
    yes = 1,
    No = 2
}

export enum externalLinkTypeEnum {

    prefix = 1,
    link = 2,
    entity = 3,
    characteristics = 4,
    vocabulary = 5,
    relationship = 6
    
}