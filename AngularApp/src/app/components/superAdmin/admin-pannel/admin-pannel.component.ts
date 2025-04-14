import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ToolModel } from '../../../models/PageBuilder/ToolModel';
import { ToolService } from '../../../services/toolsServices/tool.service';
import { Router } from '@angular/router';
import { filter, map, Observable } from 'rxjs';
import { ToolFacade } from '../../../stores/tools/tool.facade';

@Component({
  selector: 'app-admin-pannel',
  standalone: true,
  imports: [CommonModule],
  providers: [],
  templateUrl: './admin-pannel.component.html',
  styleUrl: './admin-pannel.component.css'
})
export class AdminPannelComponent implements OnInit {
  tools$ = this.toolFacade.tools$;
  loading$ = this.toolFacade.loading$;

  constructor(private toolFacade: ToolFacade, private router: Router) {}

  ngOnInit(): void {
    this.toolFacade.loadTools();
  }

  editTool(toolID: string): void {
    this.toolFacade.tools$
      .pipe(
        map((tools) => tools.find((tool) => tool.id === toolID)),
        filter((tool): tool is ToolModel => !!tool)
      )
      .subscribe((tool) => {
        this.toolFacade.selectTool(tool);
      });

    this.router.navigate(['/admin/editTool'], { queryParams: { id: toolID } });
  }

  createNewTool(): void {
    this.router.navigate(['/admin/createTool']);
  }
}
