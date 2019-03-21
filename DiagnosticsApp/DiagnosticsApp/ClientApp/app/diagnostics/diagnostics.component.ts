import { Component, Input, OnInit } from '@angular/core';
import { DataService } from './diagnostics.service';
import { Diagnostics } from './diagnostics';
import { ResponseJson } from '../responseJson';
import { Client } from '../clients/client';

@Component({
    selector: 'app-diagnostics',
    templateUrl: './diagnostics.component.html',
    providers: [DataService]
})
export class DiagnosticsComponent implements OnInit {
    @Input() client: Client;
    @Input() doctorId: number;
    response: ResponseJson = new ResponseJson();
    filesDcm: FormData;

    constructor(private dataService: DataService) { }

    ngOnInit() {
        
    }

    add() {
        var diagnostics = new Diagnostics();
        diagnostics.doctorId = this.doctorId;
        diagnostics.clientId = this.client.clientId;
        diagnostics.hasExamination = false;

        let formData = this.filesDcm;
        formData.append('doctorId', JSON.stringify(this.doctorId));
        formData.append('clientId', JSON.stringify(this.client.clientId));
        formData.append('hasExamination', JSON.stringify(false));
        formData.append('searchNow', JSON.stringify(true));

        this.dataService.addDiagnostics(formData)
            .subscribe((data: ResponseJson) => {
                console.log(data.value);
            });
    }

    find() {

    }

    fileChange(event: any) {
        let fileListTarget = event.target || event.srcElement;
        let fileList = fileListTarget.files;
        if (fileList) {
            let files: FileList = fileList;
            const formData = new FormData();
            for (let i = 0; i < files.length; i++) {
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
    }
}