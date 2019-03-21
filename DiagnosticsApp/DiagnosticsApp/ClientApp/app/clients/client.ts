export class Client {
    constructor(
        public clientId?: number,
        public firstName?: string,
        public fatherName?: string,
        public lastName?: string,
        public snils?: string,
        public birthdate?: Date,
        public isMale?: boolean,
        public passport?: string,
        public phoneNumber?: string,
        public diagnosticsList?: []) { }
}