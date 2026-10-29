import { Directive, ElementRef, Host, HostListener } from '@angular/core';

@Directive({
  selector: '[appCustom],.appCustom'
})
export class CustomDirective {

  constructor(private elementRef: ElementRef  ) { }


  @HostListener('mouseenter') onMouseEnter() {
    this.elementRef.nativeElement.style.backgroundColor = 'yellow';
  }

  @HostListener('mouseleave') onMouseLeave() {
    this.elementRef.nativeElement.style.backgroundColor = '';
  }

}
