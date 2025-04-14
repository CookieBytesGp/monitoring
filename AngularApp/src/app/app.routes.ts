import { Routes } from '@angular/router';
import { DashHomeComponent } from './components/dashboard/dash-home/dash-home.component';
import { authGuard } from './guards/auth/auth.guard';
import { pageResolver } from './stores/pages/page.resolver'; // Import the resolver
import { toolResolver } from './stores/tools/tool.resolver'; // Import the tool resolver

export const routes: Routes = [
    {
        path: "",
        pathMatch: "full",
        component: DashHomeComponent,
        canActivate: [authGuard]
    },
    {
      path: 'dashboard',
      loadChildren: () => import('./components/dashboard/dashboard.routes').then(m => m.DASHBOARD_ROUTES),
      canActivate: [authGuard] // Guard applied to all inner routes
    },
    {
      path: 'auth',
      loadChildren: () => import('./components/auth/auth.routes').then(m => m.AUTH_ROUTES)
    },
    {
      path: 'pageBuilder',
      loadChildren: () => import('./components/pageBuilder/page-builder.routes').then(m => m.PAGE_BUILDER_ROUTES),
      canActivate: [authGuard],
      resolve: { pages: pageResolver} // Add the tool resolver here
    },
    {
      path: 'admin',
      loadChildren: () => import('./components/superAdmin/admin-route').then(m => m.admin_route) ,
      resolve:{tools:toolResolver} // Add the tool resolver here
    }
];
