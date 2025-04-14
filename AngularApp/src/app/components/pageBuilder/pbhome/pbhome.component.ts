import { Component, OnInit } from '@angular/core';
import { PageModel } from '../../../models/PageBuilder/PageModel';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PageFacade } from '../../../stores/pages/page.facade';
import { LoaderComponent } from '../../shared/loader/loader/loader.component';
import { Observable, tap } from 'rxjs';

@Component({
  selector: 'app-pbhome',
  standalone: true,
  imports: [RouterModule , CommonModule , LoaderComponent],
  templateUrl: './pbhome.component.html',
  styleUrl: './pbhome.component.css'
})
export class PBHomeComponent  {
  pages$: Observable<PageModel[]> = this.PageStore.pages$.pipe(
    tap(pages => console.log('Pages updated:', pages))
  );
  
  loading$ = this.PageStore.loading$.pipe(
    tap(loading => console.log('Loading state:', loading))
  );

  constructor(
    private PageStore: PageFacade,
    private router: Router,
  ) { }

  goToSoftEdit(page: PageModel) {
    this.PageStore.selectPage(page);
    this.router.navigate(['/pageBuilder/softEdit']);
  }
}
