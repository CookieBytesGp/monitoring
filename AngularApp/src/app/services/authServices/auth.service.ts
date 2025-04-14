import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly AUTH_KEY = 'isLoggedIn';

  constructor() {
    // Initialize the logged-in state from localStorage
    const storedState = localStorage.getItem(this.AUTH_KEY);
    this.loggedIn = storedState === 'true';
  }

  private loggedIn = false;

  isAuthenticated(): boolean {
    // Return the current logged-in state
    return this.loggedIn;
  }

  login(): void {
    // Simulate a login and persist the state
    this.loggedIn = true;
    localStorage.setItem(this.AUTH_KEY, 'true');
  }

  logout(): void {
    // Simulate a logout and clear the persisted state
    this.loggedIn = false;
    localStorage.setItem(this.AUTH_KEY, 'false');
  }
}
