import { Component, OnInit } from '@angular/core';
import { CategoryService, Category } from '../../services/category.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.css'
})
export class FooterComponent implements OnInit {
  email: string = 'hhtapple@gmail.com';
  categories: Category[] = [];

  constructor(private categoryService: CategoryService, private router: Router) {}

  ngOnInit(): void {
    this.categoryService.getCategories().subscribe(categories => {
      this.categories = categories;
    });
  }

  scrollToTop(): void {
    window.scrollTo(0, 0);
  }

  onCategoryClick(category: Category) {
    this.router.navigate(['/shop'], { queryParams: { categoryId: category.id } }).then(() => {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    });
  }
}
