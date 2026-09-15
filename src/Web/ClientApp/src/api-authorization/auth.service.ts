import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';

export interface CurrentUser {
  isAuthenticated: boolean;
  userName?: string;
  roles: string[];
  permissions: string[];
}

const ANONYMOUS: CurrentUser = { isAuthenticated: false, roles: [], permissions: [] };

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private _currentUser = new BehaviorSubject<CurrentUser>(ANONYMOUS);
  currentUser$ = this._currentUser.asObservable();
  isAuthenticated$ = this.currentUser$.pipe(map(user => user.isAuthenticated));

  constructor(private http: HttpClient) {}

  // Keycloak's hosted login page is the login form — this just navigates there.
  initialize(): Observable<boolean> {
    return this.http.get<CurrentUser>('/account/user').pipe(
      tap(user => this._currentUser.next(user)),
      map(user => user.isAuthenticated),
      catchError(() => {
        this._currentUser.next(ANONYMOUS);
        return of(false);
      })
    );
  }

  login(returnUrl: string): void {
    window.location.href = `/account/login?returnUrl=${encodeURIComponent(returnUrl)}`;
  }

  logout(): void {
    window.location.href = `/account/logout?returnUrl=${encodeURIComponent('/')}`;
  }
}
