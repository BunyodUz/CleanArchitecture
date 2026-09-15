import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  standalone: false,
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
  constructor(private authService: AuthService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    this.authService.login(returnUrl);
  }
}
