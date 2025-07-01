import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ScriptService } from '../../services/script.service';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.css']
})
export class ContactComponent implements OnInit {
  constructor(private scriptService: ScriptService) {}

  ngOnInit() {
    // Không cần load scripts ở đây nữa vì đã được xử lý trong ScriptService
  }
}
