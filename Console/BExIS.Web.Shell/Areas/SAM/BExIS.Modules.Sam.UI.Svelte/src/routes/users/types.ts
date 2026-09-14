import type { GroupModel } from "../groups/types";

export interface ReadUserModel {
    id: number;
    email: string;
    userName: string;
    creationDate: string;
    modificationDate: string;
    groupIds: number[];
}

export type UserModel = {
    id: number;
    email: string;
    userName: string;
    creationDate: string;
    modificationDate: string;
}
export interface CreateUserModel {
    email: string;
    userName: string;
    groupIds: number[];
}

export interface UpdateUserModel {
    id: number;
    email: string;
    userName: string;
    groupIds: number[];
}

export interface DeleteUserModel {
    id: number;
}