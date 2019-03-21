import { Component, OnInit } from '@angular/core';
import { UserResponseJson } from './userResponseJson';
import { DataService } from './users/user.service';
import { User } from './users/user';


@Component({
    selector: 'app',
    templateUrl: './app.component.html'
})

export class UserComponent implements OnInit {
    user: User = new User();
    userLogin: User = new User();
    response: UserResponseJson = new UserResponseJson();
    authorized: boolean = false;

    constructor(private dataService: DataService) { }

    ngOnInit() {
    
    }

    login() {
        this.dataService.login(this.userLogin)
            .subscribe((data: UserResponseJson) => {
                if (data.statusCode == 200) {
                    this.user = data.value;
                    this.authorized = true;
                }
            });
    }
}