import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ScriptService } from '../../services/script.service';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.css']
})
export class AboutComponent implements OnInit {
  constructor(private scriptService: ScriptService) {}

  ngOnInit() {
    // Không cần load scripts ở đây nữa vì đã được xử lý trong ScriptService
  }
}
