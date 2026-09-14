export interface ReadGroupModel {
    id: number;
    name: string;
    description: string;
    creationDate: string;
    modificationDate: string;
    userIds: number[];
}

export interface GroupModel {
    id: number;
    name: string;
    description: string;
    creationDate: string;
    modificationDate: string;
}
export interface CreateGroupModel {
    name: string;
    description: string;
    userIds: number[];
}

export interface UpdateGroupModel {
    id: number;
    name: string;
    description: string;
    userIds: number[];
}

export interface DeleteGroupModel {
    id: number;
}