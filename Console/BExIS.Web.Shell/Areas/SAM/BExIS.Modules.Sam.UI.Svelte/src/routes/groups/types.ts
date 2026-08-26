export interface ReadGroupModel {
    id: number;
    name: string;
    description: string;
    creationDate: string;
    modificationDate: string;
}

export interface CreateGroupModel {
    name: string;
    description: string;
}

export interface UpdateGroupModel {
    id: number;
    name: string;
    description: string;
}

export interface DeleteGroupModel {
    id: number;
}