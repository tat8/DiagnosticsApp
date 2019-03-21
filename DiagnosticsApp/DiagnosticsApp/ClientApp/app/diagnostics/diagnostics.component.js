var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
import { Component, Input } from '@angular/core';
import { DataService } from './diagnostics.service';
import { Diagnostics } from './diagnostics';
import { ResponseJson } from '../responseJson';
import { Client } from '../clients/client';
var DiagnosticsComponent = /** @class */ (function () {
    function DiagnosticsComponent(dataService) {
        this.dataService = dataService;
        this.response = new ResponseJson();
    }
    DiagnosticsComponent.prototype.ngOnInit = function () {
    };
    DiagnosticsComponent.prototype.add = function () {
        var diagnostics = new Diagnostics();
        diagnostics.doctorId = this.doctorId;
        diagnostics.clientId = this.client.clientId;
        diagnostics.hasExamination = false;
        var formData = this.filesDcm;
        formData.append('doctorId', JSON.stringify(this.doctorId));
        formData.append('clientId', JSON.stringify(this.client.clientId));
        formData.append('hasExamination', JSON.stringify(false));
        formData.append('searchNow', JSON.stringify(true));
        this.dataService.addDiagnostics(formData)
            .subscribe(function (data) {
            console.log(data.value);
        });
    };
    DiagnosticsComponent.prototype.find = function () {
    };
    DiagnosticsComponent.prototype.fileChange = function (event) {
        var fileListTarget = event.target || event.srcElement;
        var fileList = fileListTarget.files;
        if (fileList) {
            var files = fileList;
            var formData = new FormData();
            for (var i = 0; i < files.length; i++) {
                formData.append('originalImagesFiles', files[i]);
            }
            this.filesDcm = formData;
        }
        //let formDataList: FormData[] = new Array(fileList.length);
        //for (var i = 0; i < fileList.length; i++) { 
        //    let file: File = fileList[i];
        //    let formData: FormData = new FormData();
        //    formData.append(file.name, file);
        //    //formDataList.push(formData);
        //    this.filesDcm = formData;
        //}
        //console.log(formDataList);
        //this.filesDcm = formDataList;
    };
    __decorate([
        Input(),
        __metadata("design:type", Client)
    ], DiagnosticsComponent.prototype, "client", void 0);
    __decorate([
        Input(),
        __metadata("design:type", Number)
    ], DiagnosticsComponent.prototype, "doctorId", void 0);
    DiagnosticsComponent = __decorate([
        Component({
            selector: 'app-diagnostics',
            templateUrl: './diagnostics.component.html',
            providers: [DataService]
        }),
        __metadata("design:paramtypes", [DataService])
    ], DiagnosticsComponent);
    return DiagnosticsComponent;
}());
export { DiagnosticsComponent };
//# sourceMappingURL=diagnostics.component.js.map