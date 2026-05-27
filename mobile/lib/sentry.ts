import * as Sentry from '@sentry/react-native';

const dsn = process.env.EXPO_PUBLIC_SENTRY_DSN;

Sentry.init({
  dsn: dsn ?? '',
  enabled: Boolean(dsn),
  environment: process.env.EXPO_PUBLIC_APP_ENV ?? 'development',
  sendDefaultPii: false,
  beforeSend(event) {
    if (event.user) {
      delete event.user.email;
      delete event.user.ip_address;
      delete event.user.username;
    }
    return event;
  },
  tracesSampleRate: 0,
});

export { Sentry };
