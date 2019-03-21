import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Client } from './client';

@Injectable()
export class DataService {

    private url = "/api/clients";

    constructor(private http: HttpClient) {
    }

    getClients() {
        return this.http.get(this.url);
    }

    filterClients(client: Client) {
        return this.http.post(this.url + '/' + 'filter', client);
    }
}