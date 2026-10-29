import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { PublicRoutingModule } from './public-routing.module';
import { LoginComponent } from './login/login.component';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { HomeComponent } from './home/home.component';
import { PubliclayoutComponent } from './publiclayout/publiclayout.component';
import { PublicHeaderComponent } from './publiclayout/public-header/public-header.component';
import { PublicFooterComponent } from './publiclayout/public-footer/public-footer.component';
import { RouterModule } from '@angular/router';


@NgModule({
  declarations: [
    LoginComponent,
    UnauthorizedComponent,
    HomeComponent,
    PubliclayoutComponent,
    PublicHeaderComponent,
    PublicFooterComponent
  ],
  imports: [
    CommonModule,
    PublicRoutingModule,
    RouterModule
  ]
})
export class PublicModule { }
