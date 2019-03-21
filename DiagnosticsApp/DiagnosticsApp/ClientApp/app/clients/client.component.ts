import { Component, OnInit } from '@angular/core';
import { DataService } from './client.service';
import { Client } from './client';
import { ResponseJson } from '../responseJson';

@Component({
    selector: 'app',
    templateUrl: './client.component.html',
    providers: [DataService]
})
export class ClientComponent implements OnInit {

    client: Client = new Client();   
    clients: Client[];                
    response: ResponseJson = new ResponseJson();
    tableMode: boolean = true;
    showDiagnostics: boolean = false;

    constructor(private dataService: DataService) { }

    ngOnInit() {
        this.loadClients();    // загрузка данных при старте компонента  
    }
    
    loadClients() {
        this.dataService.getClients()
            .subscribe((data: ResponseJson) => {
                this.clients = data.value;
            });
    }
    
    save() {
        /*if (this.client.clientId == null) {
            this.dataService.addClient(this.client)
                .subscribe((data: Client) => this.clients.push(data));
        } else {
            this.dataService.editClient(this.client)
                .subscribe(data => this.loadClients());
        }*/
        this.cancel();
    }
    editUser(u: Client) {
        this.client = u;
    }
    cancel() {
        this.client = new Client();
        this.tableMode = true;
    }
    add() {
        this.cancel();
        this.tableMode = false;
    }
    addDiagnostics(client: Client) {
        this.showDiagnostics = true;
        this.client = client;
    }
}