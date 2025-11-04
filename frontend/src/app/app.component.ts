import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoanService, Loan } from './services/loan.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  loans: Loan[] = [];

  constructor(private loanService: LoanService) {}

  ngOnInit(): void {
    this.loadLoans();
  }

  loadLoans(): void {
    this.loanService.getLoans().subscribe({
      next: (loans) => {
        this.loans = loans;
      },
      error: (error) => {
        console.error('Error loading loans:', error);
      }
    });
  }
}
