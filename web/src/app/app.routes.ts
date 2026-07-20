import { Routes } from '@angular/router';

import { Shell } from './layout/shell/shell';

export const routes: Routes = [
  {
    path: '',
    component: Shell,
    children: [
      { path: '', loadComponent: () => import('./layout/home/home').then((m) => m.Home) },
      {
        path: '**',
        loadComponent: () => import('./layout/not-found/not-found').then((m) => m.NotFound),
      },
    ],
  },
];
