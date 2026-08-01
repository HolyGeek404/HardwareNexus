import {Routes} from '@angular/router';
import {ProductTypes} from './shared/models/enums';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () =>
            import('./shared/components/home/component/home.component').then(m => m.HomeComponent)
    },
    {
        path: 'products/:category',
        loadComponent: () =>
            import('./business/products/components/products/component/products.component').then(m => m.ProductsComponent)
    },
    {
        path: `products/${ProductTypes.CPU}/:id`,
        loadComponent: () =>
            import('./business/products/components/cpu/component/cpu.component').then(m => m.CpuComponent)
    },
    {
        path: `products/${ProductTypes.GPU}/:id`,
        loadComponent: () =>
            import('./business/products/components/gpu/component/gpu.component').then(m => m.GpuComponent)
    },
    {
        path: 'products/COOLER/:id',
        loadComponent: () =>
            import('./business/products/components/cooler/component/cooler.component').then(m => m.CoolerComponent)
    },
    {
        path: 'user/sign-in',
        loadComponent: () =>
            import('./business/user/components/sign-in/component/sign-in.component').then(m => m.SignInComponent)
    },
    {
        path: 'user/dashboard',
        loadComponent: () =>
            import('./business/user/components/dashboard/component/dashboard.component').then(m => m.DashboardComponent)
    }
];
