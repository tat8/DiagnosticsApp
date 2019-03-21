export class Diagnostics {
    constructor(
        public diagnosticsId?: number,
        public clientId?: number,
        public doctorId?: number,
        public hasExamination?: boolean,
        public originalImagesFiles?: FormData[],
        public test?: FormData) { }
}