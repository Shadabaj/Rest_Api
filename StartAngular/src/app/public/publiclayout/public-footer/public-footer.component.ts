import { Component } from '@angular/core';

export interface FooterLink {
  label: string;
  path: string;
  fragment?: string;
}

export interface FooterColumn {
  title: string;
  links: FooterLink[];
}

@Component({
  selector: 'app-public-footer',
  templateUrl: './public-footer.component.html',
  styleUrls: ['./public-footer.component.css']
})
export class PublicFooterComponent {
  brandName = 'Employee 360';
  year = new Date().getFullYear();
  blurb = 'One calm place for attendance, growth, and everyday people work.';

  columns: FooterColumn[] = [
    {
      title: 'Product',
      links: [
        { label: 'Platform', path: '/', fragment: 'platform' },
        { label: 'People view', path: '/', fragment: 'people' },
        { label: 'Sign in', path: '/login' }
      ]
    },
    {
      title: 'Company',
      links: [
        { label: 'About', path: '/', fragment: 'people' },
        { label: 'Contact', path: '/', fragment: 'contact' },
        { label: 'Support', path: '/', fragment: 'contact' }
      ]
    },
    {
      title: 'Legal',
      links: [
        { label: 'Privacy', path: '/' },
        { label: 'Terms', path: '/' }
      ]
    }
  ];

  contactLines = [
    'hello@employee360.local',
    'Mon–Fri · 9:00–18:00 IST'
  ];
}
