import {ChangeDetectionStrategy, Component, inject} from '@angular/core';
import {SessionStore} from "../../../../../shared/store/session.store";

@Component({
    selector: 'app-dashboard',
    imports: [],
    templateUrl: './dashboard.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
    session = inject(SessionStore);
    user = this.session.user;
    panels = ['Overview', 'Orders', 'Settings', 'Security', 'Support'];
    activePanel = 'Overview';

    selectPanel(panel: string): void {
        this.activePanel = panel;
    }

    signOut(): void {
        this.session.logout();
    }
}
