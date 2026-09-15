export interface ReadUserModel {
    id: number;
    email: string;
    userName: string;
    registrationDate: string;
    modificationDate: string;
    groupIds: number[];
}

export type UserModel = {
    id: number;
    email: string;
    userName: string;
    registrationDate: string;
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