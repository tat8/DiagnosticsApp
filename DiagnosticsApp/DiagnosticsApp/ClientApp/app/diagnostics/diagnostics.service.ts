import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Diagnostics } from './diagnostics';

@Injectable()
export class DataService {

    private url = "/api/diagnostics";

    constructor(private http: HttpClient) {
    }

    addDiagnostics(formData: FormData) {
        return this.http.post(this.url + '/' + 'add', formData);
    }

    findCalcinatesRegions(diagnostics: Diagnostics) {
        return this.http.post(this.url + '/' + 'find', diagnostics);
    }
}