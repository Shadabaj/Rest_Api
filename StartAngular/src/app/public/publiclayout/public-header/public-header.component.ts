import { Component, HostListener, OnInit } from '@angular/core';

export interface NavLink {
  label: string;
  path: string;
  fragment?: string;
}

@Component({
  selector: 'app-public-header',
  templateUrl: './public-header.component.html',
  styleUrls: ['./public-header.component.css']
})
export class PublicHeaderComponent implements OnInit {
  menuOpen = false;
  scrolled = false;
  brandName = 'Employee 360';

  links: NavLink[] = [
    { label: 'Home', path: '/', fragment: undefined },
    { label: 'Platform', path: '/', fragment: 'platform' },
    { label: 'People', path: '/', fragment: 'people' },
    { label: 'Contact', path: '/', fragment: 'contact' }
  ];

  ngOnInit(): void {
    this.onWindowScroll();
  }

  @HostListener('window:scroll')
  onWindowScroll(): void {
    this.scrolled = window.scrollY > 24;
  }

  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
  }

  closeMenu(): void {
    this.menuOpen = false;
  }
}
