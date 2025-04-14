import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, GuardResult, MaybeAsync, Router, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../../services/authServices/auth.service';

@Injectable({
  providedIn: 'root', // Ensure this is provided at the root level
})
export class authGuard implements CanActivate {

  constructor(public authService: AuthService, public router: Router) { }
 
  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): MaybeAsync<GuardResult> {
    // Check if the user is authenticated
    if (this.authService.isAuthenticated()) {
      // User is authenticated, allow route activation
      return true;
    } else {
      // User is not authenticated, redirect to login
      console.warn('Access denied. Redirecting to login.');
      this.router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }
  }
  
}
