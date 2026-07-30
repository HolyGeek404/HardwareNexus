import {ChangeDetectionStrategy, Component, signal} from '@angular/core';
import {SessionService} from '../../../../../shared/services/session.service';
import {UserModel} from '../../../../../shared/models/models';

@Component({
  selector: 'app-dashboard',
  imports: [],
  templateUrl: './dashboard.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  user = signal<UserModel|null>(null)
  panels = ['Overview', 'Orders', 'Settings', 'Security', 'Support'];
  activePanel = 'Overview';

  constructor(userSessionService: SessionService) {
    this.user.set(userSessionService.getUserDataFromLocalStorage())
  }

  selectPanel(panel: string): void {
    this.activePanel = panel;
  }

  signOut(): void {

  }
}
