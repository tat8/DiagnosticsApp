export class User {
    constructor(
        public userId?: number,
        public firstName?: string,
        public fatherName?: string,
        public lastName?: string,
        public inn?: string,
        public roleId?: number,
        public roleName?: string,
        public password?: string,
        public phoneNumber?: string) { }
}