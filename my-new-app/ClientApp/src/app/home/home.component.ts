import {Component, ElementRef, ViewChild} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {Subject} from "rxjs";
import {SearchModel} from "../search-model";
import {environment} from "../../environments/environment";
import {debounceTime, distinctUntilChanged} from "rxjs/operators";
import {MatDialog} from "@angular/material/dialog";
import {DialogConfirmComponent} from "../dialog-confirm/dialog-confirm.component";

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {

  public searchInputEmit: Subject<string> = new Subject<string>();

  @ViewChild('input', {
    static: true
  }) public input: ElementRef

  public model: SearchModel;
  isLoading: boolean;
  isPrecision: boolean = true;
  sortBy: string = 'results';

  constructor(private httpClient: HttpClient, public dialog: MatDialog) {

    this.searchInputEmit.pipe(debounceTime(2000)).subscribe((value) => {
      this.isLoading = true;
      this.httpClient.post<SearchModel>(environment.apiUrl + 'search', {
        text: value,
        Precision: this.isPrecision ? 0 : 1,
        sortBy: this.sortBy
      }).subscribe((val) => {
        this.model = val;
        this.isLoading = false;
      }, error => this.isLoading = false)
    });
  }

  onSearchChange(value: any) {
    this.searchInputEmit.next(value);
  }

  openFile(path: string) {
    this.httpClient.post(environment.apiUrl + 'search/openFile', {text: path}).subscribe(value =>
      console.log('opened file'))
  }


  sort($event) {
    if ($event.value == 'date') {
      this.model.files = this.model.files.sort((x1, x2) => (new Date(x2.creationTime)).getTime() - (new Date(x1.creationTime)).getTime())

    } else if ($event.value == 'results') {
      this.model.files = this.model.files.sort((x1, x2) => x2.score > x1.score ? 1 : -1)

    }
  }

  deleteItem(name: string) {
    this.openDialog(name);
  }


  editMode(name: string) {
    let file = this.model.files.find(x => x.name === name)
    file.isEditMode = true;
  }

  onSaveEditing(name: string, newName: string) {

  }

  onEditChange($event: any, fileName: string) {
    let file = this.model.files.find(x => x.name === fileName)
    if (file) {
      this.httpClient.put(environment.apiUrl + 'search/rename', {fileName: file.path, newName: $event}).subscribe((value: any) => {
          file.name = $event;
          file.isEditMode = false;
          file.path = value.path;
        }
      );

    }
  }

  openDialog(name:string) {
    const dialogRef = this.dialog.open(DialogConfirmComponent, {data: {name:name}});

    dialogRef.afterClosed().subscribe(result => {
      if(result){
        this.httpClient.put(environment.apiUrl + 'search/delete', {fileName: result.name}).subscribe(value => {
          debugger;
          const file = this.model.files.find(x => x.path === result.name)
          const indexOf = this.model.files.indexOf(file)

          if (indexOf != -1)
            this.model.files.splice(indexOf, 1)

        })
      }
    });
  }

}
