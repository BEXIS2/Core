export class ReadGroupModel {
    id:number;
    name:string;
    description:string;
    creationDate:Date;
    modificationDate:Date;

    constructor(json: any)
    {
        this.id = json.id;
        this.name = json.name;
        this.description = json.description;
        this.creationDate = new Date(new Date().getDate() - Math.random()*(1e+12));
        this.modificationDate = new Date(new Date().getDate() - Math.random()*(1e+12));
    }
}

export class CreateGroupModel {
    name: string;
    description: string;

    constructor(json: any)
    {
        this.name = json.name;
        this.description = json.description;
    }
}