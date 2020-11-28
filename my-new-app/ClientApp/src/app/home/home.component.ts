import {Component, ElementRef, ViewChild} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {Subject} from "rxjs";
import {SearchModel} from "../search-model";
import {environment} from "../../environments/environment";
import {debounceTime, distinctUntilChanged} from "rxjs/operators";

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

  constructor(private httpClient: HttpClient) {

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
}
