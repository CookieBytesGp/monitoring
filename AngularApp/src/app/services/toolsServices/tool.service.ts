import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ToolModel } from '../../models/PageBuilder/ToolModel';

@Injectable({
  providedIn: 'root'
})
export class ToolService {
  private apiUrl = 'http://localhost:5000/pagebuilder/tool';

  constructor(private http: HttpClient) { }
  
  getAllTools(): Observable<ToolModel[]> {
    return this.http.get<ToolModel[]>(this.apiUrl);
  }
  getToolById(id: string): Observable<ToolModel> {
    return this.http.get<ToolModel>(`${this.apiUrl}/${id}`);
  }
  updateTool(tool: ToolModel): Observable<ToolModel> {
    return this.http.put<ToolModel>(`${this.apiUrl}/${tool.id}`, tool);
  }
}
