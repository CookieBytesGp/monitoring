import { Routes } from "@angular/router";
import { DashHomeComponent } from "./dash-home/dash-home.component";

export const DASHBOARD_ROUTES: Routes = [
    {
      path: '',
      pathMatch: "full",
      component: DashHomeComponent // Default route for /dashboard
    }
  ];