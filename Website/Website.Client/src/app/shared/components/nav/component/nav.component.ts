import {ChangeDetectionStrategy, Component, OnInit, signal} from '@angular/core';
import {NgOptimizedImage} from '@angular/common';
import {RouterLink} from '@angular/router';
import {UserSessionService} from '../../services/user-session-service';

@Component({
  selector: 'app-nav',
  imports: [
    NgOptimizedImage,
    RouterLink
  ],
  templateUrl: './nav.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './nav.component.css'
})
export class NavComponent implements OnInit {
  isLoggedIn = signal<boolean>(false);

  constructor(private userSessionService: UserSessionService) {
  }

  ngOnInit() {
    const result = this.userSessionService.checkSession().subscribe(session => {
      this.isLoggedIn.set(session);
    });
  }
}
