export class ReadSettingModel {
    id:string;
    name:string;
    description:string;
    entries:Array<ReadEntryModel>

    constructor(json: any)
    {
        this.id = json.id;
        this.name = json.name;
        this.description = json.description;

        json.entries.forEach(function (entry) {
            this.entries.push(entry)
          });
    }
}

export class ReadEntryModel{
    key:string;
    value:any;
    type:string;
    description:string;

    constructor(json: any)
    {
        this.key = json.key;
        this.value = json.value;
        this.type = json.type;
        this.description = json.description;
    }
}