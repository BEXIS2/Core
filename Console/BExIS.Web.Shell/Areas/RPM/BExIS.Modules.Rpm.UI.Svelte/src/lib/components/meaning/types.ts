
export class MeaningModel  {
    id:number;
    name: string;
    shortName: string;
    description: string;
    selectable: selectableType;
    approved: approvedType;
    externalLink: externalLinkType[];
    related_meaning: MeaningModel[];

    public constructor() {
     this.id=0
     this.name=""
     this.shortName=""
     this.approved=2
     this.description=""
     this.selectable=2
     this.externalLink=[]
     this.related_meaning=[]
    }
}

export interface externalLinkType  {
    id: number;
    uri: string;
    name: string;
    type: string;
}

export enum approvedType {
    yes = 1,
    No = 2
}

export enum selectableType {
    yes = 1,
    No = 2
}