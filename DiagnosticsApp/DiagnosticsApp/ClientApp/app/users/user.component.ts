import { Component, OnInit } from '@angular/core';
import { DataService } from './user.service';
import { User } from './user';
import { ResponseJson } from '../responseJson';

@Component({
    selector: 'app-user',
    templateUrl: './user.component.html',
    providers: [DataService]
})
export class UserComponent implements OnInit {
    user: User = new User();   
    users: User[];               
    response: ResponseJson = new ResponseJson();
    tableMode: boolean = true;         

    constructor(private dataService: DataService) { }

    ngOnInit() {
        this.loadProducts();    // загрузка данных при старте компонента  
    }
    
    loadProducts() {
        this.dataService.getUsers()
            .subscribe((data: ResponseJson) => {
                this.users = data.value;
            });
    }
    
    save() {
        if (this.user.userId == null) {
            this.dataService.addUser(this.user)
                .subscribe((data: User) => this.users.push(data));
        } else {
            this.dataService.editUser(this.user)
                .subscribe(data => this.loadProducts());
        }
        this.cancel();
    }
    editUser(u: User) {
        this.user = u;
    }
    cancel() {
        this.user = new User();
        this.tableMode = true;
    }
    add() {
        this.cancel();
        this.tableMode = false;
    }
}