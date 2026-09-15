import { Component } from '@angular/core';
import { AuthService } from 'src/api-authorization/auth.service';

@Component({
  standalone: false,
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})
export class NavMenuComponent {
  isAuthenticated$ = this.authService.isAuthenticated$;

  constructor(private authService: AuthService) {}

  logout(event: Event): void {
    event.preventDefault();
    this.authService.logout();
  }
}
