import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  // Specific feature areas MUST be registered before the public '' catch-all
  {
    path: 'portal',
    loadChildren: () => import('./portal/portal.module').then((m) => m.PortalModule)
  },
  {
    path: 'practise',
    loadChildren: () => import('./practise/practise.module').then((m) => m.PractiseModule)
  },
  {
    path: '',
    loadChildren: () => import('./public/public.module').then((m) => m.PublicModule)
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: true })],
  exports: [RouterModule]
})
export class AppRoutingModule {}
