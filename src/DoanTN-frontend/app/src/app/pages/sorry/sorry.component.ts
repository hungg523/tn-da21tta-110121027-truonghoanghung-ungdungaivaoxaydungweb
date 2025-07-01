import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-sorry',
  templateUrl: './sorry.component.html',
  styleUrls: ['./sorry.component.css']
})
export class SorryComponent {
  constructor(private router: Router) {}

  goToHome() {
    this.router.navigate(['/']);
  }

  goToCart() {
    this.router.navigate(['/cart']);
  }
} 