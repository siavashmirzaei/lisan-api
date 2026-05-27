import { Sentry } from '../lib/sentry';
import { Stack } from 'expo-router';

function RootLayout() {
  return (
    <Sentry.ErrorBoundary>
      <Stack screenOptions={{ headerShown: false }} />
    </Sentry.ErrorBoundary>
  );
}

export default Sentry.wrap(RootLayout);
