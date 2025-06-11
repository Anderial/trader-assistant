import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="modal-overlay" 
         [class.modal-show]="isOpen"
         (click)="onOverlayClick($event)">
      <div class="modal-content" 
           [class.modal-content-show]="isOpen">
        <div class="modal-header" *ngIf="title">
          <h3 class="modal-title">{{ title }}</h3>
          <button class="modal-close-btn" 
                  (click)="close()"
                  *ngIf="showCloseButton">
            <i class="fa fa-times"></i>
          </button>
        </div>
        
        <div class="modal-body">
          <ng-content></ng-content>
        </div>
        
        <div class="modal-footer" *ngIf="showFooter">
          <button class="btn btn-secondary" 
                  (click)="cancel()"
                  *ngIf="showCancelButton">
            {{ cancelText }}
          </button>
          <button class="btn" 
                  [class]="confirmButtonClass"
                  (click)="confirm()"
                  *ngIf="showConfirmButton">
            {{ confirmText }}
          </button>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./modal.component.scss']
})
export class ModalComponent {
  @Input() isOpen = false;
  @Input() title = '';
  @Input() showCloseButton = true;
  @Input() showFooter = true;
  @Input() showCancelButton = true;
  @Input() showConfirmButton = true;
  @Input() cancelText = 'Отмена';
  @Input() confirmText = 'Подтвердить';
  @Input() confirmButtonClass = 'btn-primary';
  @Input() closeOnOverlayClick = true;

  @Output() opened = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();
  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  close(): void {
    this.isOpen = false;
    this.closed.emit();
  }

  confirm(): void {
    this.confirmed.emit();
  }

  cancel(): void {
    this.cancelled.emit();
  }

  onOverlayClick(event: MouseEvent): void {
    if (this.closeOnOverlayClick && event.target === event.currentTarget) {
      this.close();
    }
  }
} 