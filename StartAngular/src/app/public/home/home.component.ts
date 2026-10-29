import { Component } from '@angular/core';

export interface FeatureItem {
  title: string;
  copy: string;
  accent: string;
}

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  brandName = 'Employee 360';
  headline = 'Everything your people need, in one clear view.';
  support =
    'Attendance, profiles, and day-to-day HR moments — designed to feel calm, fast, and human.';

  features: FeatureItem[] = [
    {
      title: 'Live attendance',
      copy: 'See who is in, who is remote, and what needs a nudge — without digging through sheets.',
      accent: '#1f9e92'
    },
    {
      title: 'People timeline',
      copy: 'From joining day to growth notes, every employee story stays connected and searchable.',
      accent: '#e3a008'
    },
    {
      title: 'Secure access',
      copy: 'JWT-ready sign-in with refresh tokens so sessions stay smooth and protected.',
      accent: '#2f6fed'
    }
  ];

  peopleNote =
    'Built for HR leads, managers, and employees who want less friction and more clarity.';
}
