import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-thank-you',
  templateUrl: './thank-you.component.html',
  styleUrls: ['./thank-you.component.css']
})
export class ThankYouComponent {
  constructor(private router: Router) {}

  goToShop() {
    this.router.navigate(['/shop']);
  }

  goToOrderHistory() {
    this.router.navigate(['/order-history']);
  }
} 