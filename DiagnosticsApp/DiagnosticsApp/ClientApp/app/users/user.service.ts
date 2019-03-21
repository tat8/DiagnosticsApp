import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { User } from './user';

@Injectable()
export class DataService {

    private url = "/api/users";

    constructor(private http: HttpClient) {
    }

    getUsers() {
        return this.http.get(this.url);
    }

    addUser(user: User) {
        return this.http.post(this.url + '/' + 'add', user);
    }

    editUser(user: User) {
        return this.http.post(this.url + '/' + 'edit', user);
    }

    filterUser(user: User) {
        return this.http.post(this.url + '/' + 'filter', user);
    }

    login(user: User) {
        return this.http.post(this.url + '/' + 'login', user);
    }
}