import {ChangeDetectionStrategy, Component, input} from '@angular/core';
import type {CpuModel} from '../../models/cpu.model';

@Component({
  selector: 'app-cpu-details',
  imports: [],
  templateUrl: './cpu-details.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './cpu-details.component.css'
})
export class CpuDetailsComponent {
  product = input.required<CpuModel>();
}
