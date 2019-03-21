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
import { DataService } from './client.service';
import { Client } from './client';
import { ResponseJson } from '../responseJson';
var ClientComponent = /** @class */ (function () {
    function ClientComponent(dataService) {
        this.dataService = dataService;
        this.client = new Client();
        this.response = new ResponseJson();
        this.tableMode = true;
        this.showDiagnostics = false;
    }
    ClientComponent.prototype.ngOnInit = function () {
        this.loadClients(); // загрузка данных при старте компонента  
    };
    ClientComponent.prototype.loadClients = function () {
        var _this = this;
        this.dataService.getClients()
            .subscribe(function (data) {
            _this.clients = data.value;
        });
    };
    ClientComponent.prototype.save = function () {
        /*if (this.client.clientId == null) {
            this.dataService.addClient(this.client)
                .subscribe((data: Client) => this.clients.push(data));
        } else {
            this.dataService.editClient(this.client)
                .subscribe(data => this.loadClients());
        }*/
        this.cancel();
    };
    ClientComponent.prototype.editUser = function (u) {
        this.client = u;
    };
    ClientComponent.prototype.cancel = function () {
        this.client = new Client();
        this.tableMode = true;
    };
    ClientComponent.prototype.add = function () {
        this.cancel();
        this.tableMode = false;
    };
    ClientComponent.prototype.addDiagnostics = function (client) {
        this.showDiagnostics = true;
        this.client = client;
    };
    ClientComponent = __decorate([
        Component({
            selector: 'app',
            templateUrl: './client.component.html',
            providers: [DataService]
        }),
        __metadata("design:paramtypes", [DataService])
    ], ClientComponent);
    return ClientComponent;
}());
export { ClientComponent };
//# sourceMappingURL=client.component.js.map