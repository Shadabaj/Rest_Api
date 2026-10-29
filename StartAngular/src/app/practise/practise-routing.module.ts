import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PractiseComponent } from './practise.component';

const routes: Routes = [{path:'',component:PractiseComponent}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PractiseRoutingModule { }
