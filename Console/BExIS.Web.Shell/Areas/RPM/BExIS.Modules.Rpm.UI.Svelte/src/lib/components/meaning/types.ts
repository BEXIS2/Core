
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