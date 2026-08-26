export interface ReadUserModel {
    id: number;
    email: string;
    userName: string;
    creationDate: string;
    modificationDate: string;
}

export interface CreateUserModel {
    email: string;
    userName: string;
}

export interface UpdateUserModel {
    id: number;
    email: string;
    userName: string;
}

export interface DeleteUserModel {
    id: number;
}