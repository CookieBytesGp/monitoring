import { Component, OnInit } from '@angular/core';
import { PageService } from '../../../services/pagesServices/page.service';
import { PageModel } from '../../../models/PageBuilder/PageModel';
import { RouterModule } from '@angular/router';
import { PageFacade } from '../../../stores/pages/page.facade';
import { filter, map } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dash-home',
  standalone: true,
  imports: [RouterModule , CommonModule],
  templateUrl: './dash-home.component.html',
  styleUrl: './dash-home.component.css'
})
export class DashHomeComponent implements OnInit {
  pages$ = this.PageStore.pages$;
  loading$ = this.PageStore.loading$;
  constructor( private PageStore: PageFacade) { }
  ngOnInit() {
    this.PageStore.loadPages();
  }


}
