import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { LoginModel } from '../../Models/LoginModel';
import { UserModel } from '../../Models/UserModel';
import { ROLE_HOME_PATH, getPrimaryRole } from '../../constants/roles';
import { exhaustMap } from 'rxjs';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  userForm: FormGroup;
  returnUrl = '';
  submitted = false;
  loading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.userForm = this.fb.group({
      userName: ['', [Validators.required, Validators.pattern(/^NEO\d{4}$/)]],
      password: [
        '',
        [
          Validators.required,
          Validators.pattern(/^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$/)
        ]
      ]
    });
  }

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '';
  }

  get f() {
    return this.userForm.controls;
  }

  loginUser(): void {
    this.submitted = true;
    this.errorMessage = '';

    if (this.userForm.invalid) {
      return;
    }

    this.loading = true;
    const payload: LoginModel = {
      userName: this.userForm.value.userName,
      password: this.userForm.value.password
    };

    this.authService.ValidateUser(payload).subscribe({
      next: (response) => {
        this.loading = false;
        if (!response.body) {
          this.errorMessage = 'Login succeeded but no user data was returned.';
          return;
        }

        const data: UserModel = response.body;
        if (!data.roles?.length) {
          this.errorMessage = 'Your account has no role assigned. Contact Admin.';
          return;
        }

        this.authService.setAuthUser(data);

        const primary = getPrimaryRole(data.roles);
        const roleHome = primary ? ROLE_HOME_PATH[primary] : this.authService.getHomePath();
        const target = this.authService.resolvePostLoginPath(this.returnUrl || roleHome);

        this.router.navigateByUrl(target);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage =
          error?.error?.message ||
          (error?.status === 401
            ? 'Invalid username or password.'
            : 'Unable to sign in. Please try again.');
      }
    });
  }
}
