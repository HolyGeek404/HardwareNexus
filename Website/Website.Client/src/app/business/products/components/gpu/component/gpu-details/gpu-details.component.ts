import {ChangeDetectionStrategy, Component, input} from '@angular/core';
import {GpuModel} from '../../models/gpu.model';

@Component({
  selector: 'app-gpu-details',
  imports: [],
  templateUrl: './gpu-details.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './gpu-details.component.css'
})
export class GpuDetailsComponent {
  product = input.required<GpuModel>();

}
