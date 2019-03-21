import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { ClientComponent } from './clients/client.component';
import { UserComponent } from './users/user.component';
import { DiagnosticsComponent } from './diagnostics/diagnostics.component';

@NgModule({
    imports: [BrowserModule, FormsModule, HttpClientModule],
    declarations: [ClientComponent, UserComponent, DiagnosticsComponent],
    bootstrap: [ClientComponent]
})

export class AppModule { }