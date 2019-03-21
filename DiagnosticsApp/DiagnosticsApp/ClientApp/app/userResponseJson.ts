import { User } from './users/user';

export class UserResponseJson {
    constructor(
        public value?: User,
        public statusCode?: number) { }
}