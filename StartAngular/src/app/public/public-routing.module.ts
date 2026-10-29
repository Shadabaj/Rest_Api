import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PubliclayoutComponent } from './publiclayout/publiclayout.component';
import { HomeComponent } from './home/home.component';

const routes: Routes = [
  {path:'',component:PubliclayoutComponent,children:[
    {path:'',component:HomeComponent},
  ]}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PublicRoutingModule { }
