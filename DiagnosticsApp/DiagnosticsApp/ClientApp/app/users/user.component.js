var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
import { Component } from '@angular/core';
import { DataService } from './user.service';
import { User } from './user';
import { ResponseJson } from '../responseJson';
var UserComponent = /** @class */ (function () {
    function UserComponent(dataService) {
        this.dataService = dataService;
        this.user = new User();
        this.response = new ResponseJson();
        this.tableMode = true;
    }
    UserComponent.prototype.ngOnInit = function () {
        this.loadProducts(); // загрузка данных при старте компонента  
    };
    UserComponent.prototype.loadProducts = function () {
        var _this = this;
        this.dataService.getUsers()
            .subscribe(function (data) {
            _this.users = data.value;
        });
    };
    UserComponent.prototype.save = function () {
        var _this = this;
        if (this.user.userId == null) {
            this.dataService.addUser(this.user)
                .subscribe(function (data) { return _this.users.push(data); });
        }
        else {
            this.dataService.editUser(this.user)
                .subscribe(function (data) { return _this.loadProducts(); });
        }
        this.cancel();
    };
    UserComponent.prototype.editUser = function (u) {
        this.user = u;
    };
    UserComponent.prototype.cancel = function () {
        this.user = new User();
        this.tableMode = true;
    };
    UserComponent.prototype.add = function () {
        this.cancel();
        this.tableMode = false;
    };
    UserComponent = __decorate([
        Component({
            selector: 'app-user',
            templateUrl: './user.component.html',
            providers: [DataService]
        }),
        __metadata("design:paramtypes", [DataService])
    ], UserComponent);
    return UserComponent;
}());
export { UserComponent };
//# sourceMappingURL=user.component.js.map