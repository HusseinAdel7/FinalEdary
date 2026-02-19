import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44354/',
  redirectUri: baseUrl,
  clientId: 'Edary_App',
  responseType: 'code',
  scope: 'offline_access Edary',
  requireHttps: true,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'Edary',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://edaryabp-production.up.railway.app/',
      rootNamespace: 'Edary',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
