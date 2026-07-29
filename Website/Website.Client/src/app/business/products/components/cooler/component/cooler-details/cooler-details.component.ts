import {ChangeDetectionStrategy, Component, input} from '@angular/core';
import type {CoolerModel} from '../../models/cooler.model';

@Component({
  selector: 'app-cooler-details',
  imports: [],
  templateUrl: './cooler-details.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './cooler-details.component.css'
})
export class CoolerDetailsComponent {
  product = input.required<CoolerModel>();
}
