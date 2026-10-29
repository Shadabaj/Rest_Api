import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PublicModule } from './public/public.module';
import { UnauthorizedComponent } from './public/unauthorized/unauthorized.component';

const routes: Routes = [
{path:'',loadChildren:()=>PublicModule},
{path:'practise',loadChildren:()=>import('./practise/practise.module').then(m=>m.PractiseModule)}
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: true })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
