import { Routes } from "@angular/router";
import { AdminLogComponent } from "./admin-log/admin-log.component";
import { AdminPannelComponent } from "./admin-pannel/admin-pannel.component";
import { EditToolComponent } from "./edit-tool/edit-tool.component";
import { CreateToolComponent } from "./create-tool/create-tool.component";

export const admin_route : Routes = [
    {
        path:'',
        pathMatch: 'full',
        component:AdminPannelComponent
    },
    {
        path:'login',
        component:AdminLogComponent
    },
    {
        path:'editTool',
        component:EditToolComponent
    },
    {
        path:'createTool',
        component:CreateToolComponent
    }
]