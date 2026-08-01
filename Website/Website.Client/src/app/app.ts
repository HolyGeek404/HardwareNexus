import {ChangeDetectionStrategy, Component, signal} from '@angular/core';
import {RouterOutlet} from '@angular/router';
import {Nav} from "./shared/components/nav/component/nav.component";

@Component({
    selector: 'app-root',
    imports: [Nav, RouterOutlet],
    templateUrl: './app.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './app.css'
})
export class App {
    protected readonly title = signal('HardwareNexusWebsite');
}
