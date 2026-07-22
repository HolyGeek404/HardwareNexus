import {Component, input, ChangeDetectionStrategy} from '@angular/core';
import type {CpuModel} from '../../../../models/product/CpuModel';

@Component({
  selector: 'app-cpu-spec-details',
  imports: [],
  templateUrl: './cpu-spec-details.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './cpu-spec-details.css'
})
export class CpuSpecDetails {
  product = input.required<CpuModel>();
}
