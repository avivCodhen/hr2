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
  constructor(private httpClient: HttpClient) {

    this.searchInputEmit.pipe( debounceTime(2000)).subscribe((value) => {
      this.isLoading = true;
      this.httpClient.post<SearchModel>(environment.apiUrl + 'search', {text: value, Precision: this.isPrecision ? 0 : 1}).subscribe((val) => {
        this.model = val;
        this.isLoading = false;
      },error => this.isLoading = false)
    });
  }

  onSearchChange(value: any) {
    this.searchInputEmit.next(value);
  }

  openFile(path: string) {
    this.httpClient.post(environment.apiUrl + 'search/openFile', {text: path}).subscribe(value =>
      console.log('opened file'))
  }
}
