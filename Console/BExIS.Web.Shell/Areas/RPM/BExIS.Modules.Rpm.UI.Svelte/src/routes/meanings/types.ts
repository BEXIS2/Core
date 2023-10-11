
export class MeaningModel  {
    Id:number;
    Name: string;
    ShortName: string;
    Description: string;
    Selectable: Selectable;
    Approved: Approved;
    ExternalLink: ExternalLink[];
    Related_meaning: MeaningModel[];

    public constructor() {
     this.Id=0
     this.Name=""
     this.ShortName=""
     this.Approved=2
     this.Description=""
     this.Selectable=2
     this.ExternalLink=[]
     this.Related_meaning=[]
    }
}

export interface ExternalLink  {
    URI: string;
    Name: string;
    Type: string;
}

export enum Approved {
    yes = 1,
    No = 2
}

export enum Selectable {
    yes = 1,
    No = 2
}