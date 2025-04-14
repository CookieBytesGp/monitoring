import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PageModel } from '../../models/PageBuilder/PageModel';

@Injectable({
  providedIn: 'root'
})
export class PageService {
  private apiUrl = 'http://localhost:5000/pagebuilder/page';

  constructor(private http: HttpClient) { }

  getAllPages(): Observable<PageModel[]> {
    return this.http.get<PageModel[]>(this.apiUrl);
  }
  getPageById(id:string): Observable<PageModel> {
    return this.http.get<PageModel>(this.apiUrl + "/" + id);
  }
  updatePage(id:string , page : PageModel): Observable<PageModel> {
    return this.http.put<PageModel>(`${this.apiUrl}/${id}`, page);
  }
}