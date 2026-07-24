import { Component, signal, ChangeDetectionStrategy } from '@angular/core';
import {Nav} from './nav/nav';
import {RouterOutlet} from '@angular/router';

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
