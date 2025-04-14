import { Routes } from "@angular/router";
import { PBHomeComponent } from "./pbhome/pbhome.component";
import { PBSoftEditComponent } from "./pbsoft-edit/pbsoft-edit.component";
import { PBPreviewComponent } from "./pbpreview/pbpreview.component";
import { PBPageMakerComponent } from "./pbpage-maker/pbpage-maker.component";
import { toolResolver } from "../../stores/tools/tool.resolver";

export const PAGE_BUILDER_ROUTES: Routes = [
    {
      path: '',
      pathMatch: "full",
      component: PBHomeComponent
    },
    {
        path: "softEdit",
        component: PBSoftEditComponent
    },
    {
        path : "pageMaker",
        component: PBPageMakerComponent ,
        resolve:{ tools: toolResolver }
    },
    {
        path : "Preview",
        component: PBPreviewComponent
    }
  ];