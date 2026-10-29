import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { PractiseRoutingModule } from './practise-routing.module';
import { PractiseComponent } from './practise.component';
import { FormsModule } from '@angular/forms';
import { CustomDirective } from './custom.directive';


@NgModule({
  declarations: [PractiseComponent, CustomDirective],
  imports: [
    CommonModule,
    PractiseRoutingModule,
    FormsModule
  ]
})
export class PractiseModule { }
